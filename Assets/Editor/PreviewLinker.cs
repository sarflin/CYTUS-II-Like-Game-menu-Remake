using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 扫描 Assets/SongsPreview/ 下的 *_Preview.mp3，
/// 按 "角色-艺术家 - 歌曲名_Preview" 命名规则解析，
/// 自动匹配 CharacterData 并生成/更新 SongData（放入 Resources/Songs/）。
/// 菜单: Cytus2 > Audio > Link Song Previews
/// </summary>
public static class PreviewLinker
{
    private const string PreviewFolder = "Assets/SongsPreview";
    private const string SongDataFolder = "Assets/Resources/Songs";
    private const string CharacterDataFolder = "Assets/Resources/Characters";
    private const string CoverFolder = "Assets/Art/SongCovers";
    private const string PreviewSuffix = "_Preview";
    private const string EventListFileName = "EventList_SongPreviews.txt";

    [MenuItem("Cytus2/Audio/Link Song Previews")]
    public static void LinkPreviews()
    {
        if (!Directory.Exists(PreviewFolder))
        {
            Debug.LogError($"[PreviewLinker] 未找到预览目录: {PreviewFolder}");
            return;
        }

        EnsureDirectory(SongDataFolder);

        var characters = LoadCharacters();
        if (characters.Count == 0)
        {
            Debug.LogError("[PreviewLinker] 未找到任何 CharacterData");
            return;
        }

        var existingSongs = LoadExistingSongs();

        // 权威事件名清单（Wwise 中的真实事件名，保留大小写），按(角色,歌名)键匹配
        var eventMap = LoadEventNames();

        var files = Directory.GetFiles(PreviewFolder, "*.mp3", SearchOption.TopDirectoryOnly);
        if (files.Length == 0)
        {
            Debug.LogWarning("[PreviewLinker] SongsPreview 中没有 mp3 文件");
            return;
        }

        int created = 0, updated = 0, unmatched = 0;
        var unmatchedNames = new List<string>();
        int eventMatched = 0, eventFallback = 0;
        int coversMatched = 0;
        var missingCovers = new List<string>();
        var expectedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            if (!TryParseFileName(Path.GetFileNameWithoutExtension(file),
                out string characterName, out string artist, out string songName))
            {
                unmatchedNames.Add(Path.GetFileName(file));
                unmatched++;
                continue;
            }

            var character = FindCharacter(characters, characterName);
            if (character == null)
            {
                unmatchedNames.Add($"{Path.GetFileName(file)}  (角色 '{characterName}' 未匹配)");
                unmatched++;
                continue;
            }

            var audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                GetAssetPath(file));
            if (audioClip == null)
            {
                Debug.LogWarning($"[PreviewLinker] 无法加载音频: {file}，请确保 Unity 已导入 mp3");
            }

            var song = FindOrCreateSong(existingSongs, character, songName, out bool isNew);
            song.songName = songName;
            song.artist = artist;
            song.character = character;
            song.previewClip = audioClip;
            expectedKeys.Add(KeyOf(character.characterName, songName));

            TryAssignCover(song, characterName, songName, missingCovers);
            if (song.coverArt != null) coversMatched++;

            // 优先按(角色,歌名)取 Wwise 真实事件名；找不到时按解析名生成（仅供回退测试）
            string eventName = null;
            if (eventMap != null)
                eventMap.TryGetValue(KeyOf(characterName, songName), out eventName);
            song.wwiseEventPreview = eventName ?? BuildEventName(characterName, songName);
            if (eventName != null) eventMatched++; else eventFallback++;
            EditorUtility.SetDirty(song);

            if (isNew)
            {
                AssetDatabase.CreateAsset(song, $"{SongDataFolder}/{character.characterName} - {songName}.asset");
                created++;
            }
            else
            {
                updated++;
            }
        }

        int pruned = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:SongData", new[] { SongDataFolder }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var stale = AssetDatabase.LoadAssetAtPath<SongData>(path);
            if (stale == null) continue;

            bool keep = stale.character != null &&
                        expectedKeys.Contains(KeyOf(stale.character.characterName, stale.songName));
            if (!keep)
            {
                AssetDatabase.DeleteAsset(path);
                pruned++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string summary = $"[PreviewLinker] 完成: 新建 {created} 首, 更新 {updated} 首, 未匹配 {unmatched} 个文件, 事件匹配 {eventMatched} 个, 回退生成 {eventFallback} 个, 封面匹配 {coversMatched} 张, 清理冗余 {pruned} 个";
        Debug.Log(summary);
        if (unmatchedNames.Count > 0)
        {
            Debug.LogWarning("[PreviewLinker] 未匹配文件:\n" + string.Join("\n", unmatchedNames));
        }
        if (missingCovers.Count > 0)
        {
            Debug.LogWarning("[PreviewLinker] 未找到封面的歌曲（把封面放到 "
                + CoverFolder + "，命名为 “角色-歌曲名.png”）:\n"
                + string.Join("\n", missingCovers));
        }
    }

    [MenuItem("Cytus2/Audio/Link Cover Art")]
    public static void LinkCovers()
    {
        var songs = LoadExistingSongs();
        if (songs.Count == 0)
        {
            Debug.LogWarning("[PreviewLinker] 没有 SongData 可绑定封面，请先跑 Cytus2 > Audio > Link Song Previews");
            return;
        }

        int matched = 0;
        var missing = new List<string>();
        foreach (var song in songs)
        {
            string charName = song.character != null ? song.character.characterName : "";
            if (string.IsNullOrEmpty(charName))
            {
                missing.Add($"(无角色) {song.songName}");
                continue;
            }
            TryAssignCover(song, charName, song.songName, missing);
            if (song.coverArt != null) matched++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[PreviewLinker] Link Cover Art 完成: 匹配 {matched}/{songs.Count} 张封面");
        if (missing.Count > 0)
        {
            Debug.LogWarning("[PreviewLinker] 未找到封面的歌曲:\n" + string.Join("\n", missing));
        }
    }

    [MenuItem("Cytus2/Audio/Link Cover Art", true)]
    private static bool LinkCoversValidation()
    {
        return !Application.isPlaying;
    }

    /// <summary>
    /// 按命名约定自动匹配封面并赋给 song.coverArt。
    /// 约定（不区分大小写，忽略标点）：
    ///  1) 首选 “角色-歌曲名.png”（例如 Cherry-0scill4tor (feat_ Mas Kimura & MEG).png）
    ///  2) 回退：文件名含完整歌曲名（如带艺术家的 “艺术家 - 歌曲名.png”），同名多角色时优先含角色名的那个。
    /// </summary>
    private static void TryAssignCover(SongData song, string characterName, string songName,
        List<string> missing)
    {
        if (!Directory.Exists(CoverFolder))
        {
            missing.Add($"{characterName} - {songName}");
            return;
        }

        var files = Directory.GetFiles(CoverFolder, "*.png", SearchOption.TopDirectoryOnly)
            .Concat(Directory.GetFiles(CoverFolder, "*.jpg", SearchOption.TopDirectoryOnly))
            .Concat(Directory.GetFiles(CoverFolder, "*.jpeg", SearchOption.TopDirectoryOnly))
            .ToArray();
        if (files.Length == 0)
        {
            missing.Add($"{characterName} - {songName}");
            return;
        }

        string charNorm = Normalize(characterName);
        string songNorm = Normalize(songName);
        if (charNorm.Length == 0 || songNorm.Length == 0)
        {
            missing.Add($"{characterName} - {songName}");
            return;
        }

        string pick = null;
        var contains = new List<string>();
        foreach (var file in files)
        {
            string baseNorm = Normalize(Path.GetFileNameWithoutExtension(file));
            if (baseNorm.Length == 0) continue;
            if (baseNorm == charNorm + songNorm)   // 首选: 角色-歌曲名
            {
                pick = file;
                break;
            }
            if (baseNorm.Contains(songNorm)) contains.Add(file);   // 回退: 文件名含完整歌曲名
        }

        if (pick == null && contains.Count > 0)
        {
            // 同名歌曲多角色时，优先文件名里含角色名的那个
            pick = contains.FirstOrDefault(f =>
                Normalize(Path.GetFileNameWithoutExtension(f)).Contains(charNorm));
            if (pick == null) pick = contains[0];
        }

        if (pick == null)
        {
            missing.Add($"{characterName} - {songName}");
            return;
        }

        string assetPath = pick.Replace('\\', '/');
        EnsureSpriteImport(assetPath);
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite == null)
        {
            missing.Add($"{characterName} - {songName}  (无法作为 Sprite 加载: {assetPath})");
            return;
        }

        song.coverArt = sprite;
        EditorUtility.SetDirty(song);
    }

    private static void EnsureSpriteImport(string path)
    {
        string assetPath = path.Replace('\\', '/');
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;
        if (importer.spriteImportMode != SpriteImportMode.Single ||
            importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }
    }

    [MenuItem("Cytus2/Audio/Link Song Previews", true)]
    private static bool LinkPreviewsValidation()
    {
        return !Application.isPlaying;
    }

    private static List<CharacterData> LoadCharacters()
    {
        var list = new List<CharacterData>();
        if (!Directory.Exists(CharacterDataFolder)) return list;
        foreach (var guid in AssetDatabase.FindAssets("t:CharacterData", new[] { CharacterDataFolder }))
        {
            var asset = AssetDatabase.LoadAssetAtPath<CharacterData>(
                AssetDatabase.GUIDToAssetPath(guid));
            if (asset != null) list.Add(asset);
        }
        return list;
    }

    private static List<SongData> LoadExistingSongs()
    {
        var list = new List<SongData>();
        if (!Directory.Exists(SongDataFolder)) return list;
        foreach (var guid in AssetDatabase.FindAssets("t:SongData", new[] { SongDataFolder }))
        {
            var asset = AssetDatabase.LoadAssetAtPath<SongData>(
                AssetDatabase.GUIDToAssetPath(guid));
            if (asset != null) list.Add(asset);
        }
        return list;
    }

    /// <summary>从 Wwise 工程的 EventList 清单读入真实事件名，建 (角色,歌名)→事件名 字典。</summary>
    private static Dictionary<string, string> LoadEventNames()
    {
        string wprojDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "cytus2menuremake_WwiseProject");
        string listFile = Path.Combine(wprojDir, EventListFileName);
        if (!File.Exists(listFile))
        {
            Debug.LogWarning($"[PreviewLinker] 未找到事件清单: {listFile}");
            return null;
        }

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string pendingChar = null, pendingArtistSong = null;
        foreach (var rawLine in File.ReadAllLines(listFile, Encoding.UTF8))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // 描述行:  N. [角色] 艺术家 - 歌名
            var desc = Regex.Match(line, @"^\d+\.\s*\[([^\]]+)\]\s*(.+)$");
            if (desc.Success)
            {
                pendingChar = desc.Groups[1].Value.Trim();
                pendingArtistSong = desc.Groups[2].Value.Trim();
                continue;
            }

            // 事件行: Event: Play_Preview_xxx
            var ev = Regex.Match(line, @"^Event:\s*(\S+)");
            if (!ev.Success || pendingChar == null) continue;

            string songName = LastDashSegment(pendingArtistSong);
            map[KeyOf(pendingChar, songName)] = ev.Groups[1].Value.Trim();
            pendingChar = null;
        }

        if (map.Count == 0) return null;
        Debug.Log($"[PreviewLinker] 从 {listFile} 读取 {map.Count} 个事件名");
        return map;
    }

    /// <summary>取 "a - b - c" 里最后一段作为歌名，与 TryParseFileName 的取法保持一致。</summary>
    private static string LastDashSegment(string s)
    {
        int idx = s.LastIndexOf(" - ", StringComparison.Ordinal);
        return idx >= 0 ? s.Substring(idx + 3).Trim() : s;
    }

    private static string KeyOf(string characterName, string songName)
    {
        return Normalize(characterName) + "|" + Normalize(songName);
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }

    private static string GetAssetPath(string fullPath)
    {
        string rel = fullPath.Replace('\\', '/');
        int idx = rel.IndexOf("Assets/", StringComparison.Ordinal);
        return idx >= 0 ? rel.Substring(idx) : "Assets/" + Path.GetFileName(fullPath);
    }

    /// <summary>
    /// 解析 "角色-艺术家 - 歌曲名_Preview"。艺术家可含 " _ " 分隔的多人。
    /// </summary>
    private static bool TryParseFileName(string fileName,
        out string characterName, out string artist, out string songName)
    {
        characterName = artist = songName = "";

        if (fileName.EndsWith(PreviewSuffix, StringComparison.Ordinal))
            fileName = fileName.Substring(0, fileName.Length - PreviewSuffix.Length);

        var parts = fileName.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return false;

        songName = parts[parts.Length - 1].Trim();

        string head = string.Join(" - ", parts.Take(parts.Length - 1));
        int dash = head.IndexOf('-');
        if (dash <= 0 || dash >= head.Length - 1) return false;

        characterName = head.Substring(0, dash).Trim();
        string artists = head.Substring(dash + 1).Trim();
        artist = artists.Replace(" _ ", ", ").Trim();

        return !string.IsNullOrEmpty(characterName) && !string.IsNullOrEmpty(songName);
    }

    private static CharacterData FindCharacter(List<CharacterData> characters, string name)
    {
        string normalized = Normalize(name);
        return characters.FirstOrDefault(c => Normalize(c.characterName) == normalized);
    }

    private static string Normalize(string s) =>
        new string((s ?? "").ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

    private static SongData FindOrCreateSong(List<SongData> existing,
        CharacterData character, string songName, out bool isNew)
    {
        foreach (var song in existing)
        {
            if (song.character == character &&
                string.Equals(song.songName, songName, StringComparison.OrdinalIgnoreCase))
            {
                isNew = false;
                return song;
            }
        }
        isNew = true;
        return ScriptableObject.CreateInstance<SongData>();
    }

    private static string BuildEventName(string characterName, string songName)
    {
        string clean = Normalize(characterName) + "_" + Normalize(songName);
        if (clean.Length > 40) clean = clean.Substring(0, 40);
        return "Play_Preview_" + clean;
    }
}
