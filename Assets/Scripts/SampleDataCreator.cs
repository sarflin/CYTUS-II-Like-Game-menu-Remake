using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class SampleDataCreator : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Cytus2/Create Sample Data")]
    public static void CreateSampleData()
    {
        EnsureDirectories();

        // === 创建 8 个角色（Switch 名大小写必须与 Wwise 完全一致：paff/neko 是小写） ===
        CreateSampleCharacter("Paff", "An AI virtual singer with a unique voice.", new Color(0.8f, 0.2f, 0.8f), 0, "paff");
        CreateSampleCharacter("Neko", "A mysterious cat-eared girl who loves dark music.", new Color(0.2f, 0.2f, 0.8f), 1, "neko");
        CreateSampleCharacter("RObo_head", "A robot DJ specializing in electronic music.", new Color(0.2f, 0.8f, 0.2f), 2, "Robo_Head");
        CreateSampleCharacter("Cherry", "A pop star with electrifying stage presence.", new Color(0.9f, 0.2f, 0.2f), 3);
        CreateSampleCharacter("Xenon", "A warrior from another dimension.", new Color(0.1f, 0.5f, 0.9f), 4);
        CreateSampleCharacter("ConneR", "A classical pianist with a dark past.", new Color(0.8f, 0.4f, 0.1f), 5);
        CreateSampleCharacter("Nai Long", "A legendary dragon warrior mastering ancient rhythms.", new Color(0.0f, 0.7f, 0.3f), 6, "NaiLong");
        CreateSampleCharacter("sarflin", "A mysterious digital entity from the cyber realm.", new Color(0.0f, 0.8f, 0.9f), 7);

        AssetDatabase.Refresh();
        Debug.Log("Sample data created! 8 characters. (歌曲数据请用 Cytus2 > Audio > Link Song Previews 生成)");
    }

    [MenuItem("Cytus2/Refresh Character Data")]
    public static void RefreshCharacterData()
    {
        EnsureDirectories();
        int updated = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:CharacterData", new[] { "Assets/Resources/Characters" }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var character = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
            if (character == null) continue;

            if (string.IsNullOrEmpty(character.wwiseSwitchName))
                character.wwiseSwitchName = DefaultSwitchName(character.characterName);

            character.portrait = FindPortrait(character.characterName);

            EditorUtility.SetDirty(character);
            updated++;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Character data refreshed: {updated} 个角色已补齐 portrait / wwiseSwitchName");
    }

    private static string DefaultSwitchName(string displayName)
    {
        switch (displayName)
        {
            case "Paff": return "paff";
            case "Neko": return "neko";
            case "Nai Long": return "NaiLong";
            case "RObo_head": return "Robo_Head";
            default: return null;
        }
    }

    private static Sprite FindPortrait(string characterName)
    {
        const string artDir = "Assets/Art/Characters";
        if (!Directory.Exists(artDir)) return null;

        string norm = new string((characterName ?? "").ToLowerInvariant()
            .Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrEmpty(norm)) return null;

        foreach (var file in Directory.GetFiles(artDir, "*.png"))
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string fileNorm = new string(fileName.ToLowerInvariant()
                .Where(char.IsLetterOrDigit).ToArray());
            if (string.IsNullOrEmpty(fileNorm)) continue;

            // 精确/包含匹配，例如 RObo_head ↔ robo.png、Paff ↔ paff.png
            if (norm == fileNorm ||
                (norm.Contains(fileNorm) || fileNorm.Contains(norm)))
            {
                EnsureSpriteImport(file);
                return AssetDatabase.LoadAssetAtPath<Sprite>(file.Replace('\\', '/'));
            }
        }
        return null;
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

    private static void EnsureDirectories()
    {
        if (!Directory.Exists("Assets/Resources"))
            Directory.CreateDirectory("Assets/Resources");
        if (!Directory.Exists("Assets/Resources/Characters"))
            Directory.CreateDirectory("Assets/Resources/Characters");
        if (!Directory.Exists("Assets/Resources/Songs"))
            Directory.CreateDirectory("Assets/Resources/Songs");
    }

    private static void CreateSampleCharacter(string name, string desc, Color color, int sortOrder = 0, string wwiseSwitchName = null)
    {
        var character = ScriptableObject.CreateInstance<CharacterData>();
        character.characterName = name;
        character.description = desc;
        character.themeColor = color;
        character.sortOrder = sortOrder;
        character.wwiseSwitchName = wwiseSwitchName;

        string path = $"Assets/Resources/Characters/{name}.asset";
        if (AssetDatabase.LoadAssetAtPath<CharacterData>(path) != null)
        {
            Debug.Log($"[SampleData] 已存在，跳过：{name}（如需改字段请用 Cytus2 > Refresh Character Data）");
            return;
        }
        AssetDatabase.CreateAsset(character, path);
    }
#endif
}
