using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;

public class DataEditorWindow : EditorWindow
{
    // Character fields
    private string charName = "";
    private string charDesc = "";
    private int charSortOrder;
    private Texture2D charPortrait;
    private Texture2D charFullArt;
    private Color charColor = Color.white;

    // Song fields
    private string songName = "";
    private string songArtist = "";
    private int songSortOrder;
    private Texture2D songCover;
    private string characterName;
    private float bpm = 120f;
    private int easyLv, hardLv, chaosLv, glitchLv;

    private Vector2 scrollPos;

    [MenuItem("Cytus2/Data Editor")]
    public static void ShowWindow()
    {
        var w = GetWindow<DataEditorWindow>("Cytus2 Data Editor");
        w.minSize = new Vector2(500, 600);
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawCharacterSection();
        EditorGUILayout.Space(20);
        DrawSongSection();

        EditorGUILayout.EndScrollView();
    }

    // ===== Character =====

    private void DrawCharacterSection()
    {
        GUILayout.Label("CREATE CHARACTER", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        charName = EditorGUILayout.TextField("Name", charName);
        charDesc = EditorGUILayout.TextField("Description", charDesc);
        charSortOrder = EditorGUILayout.IntField("Sort Order", charSortOrder);
        charColor = EditorGUILayout.ColorField("Theme Color", charColor);

        charPortrait = (Texture2D)EditorGUILayout.ObjectField("Portrait Image", charPortrait, typeof(Texture2D), false);
        charFullArt = (Texture2D)EditorGUILayout.ObjectField("Full Art (optional)", charFullArt, typeof(Texture2D), false);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Character", GUILayout.Height(30)))
            CreateCharacter();
        if (GUILayout.Button("Clear", GUILayout.Height(30)))
            ClearCharacterFields();
        EditorGUILayout.EndHorizontal();
    }

    private void CreateCharacter()
    {
        if (string.IsNullOrEmpty(charName))
        {
            EditorUtility.DisplayDialog("Error", "Character name is required.", "OK");
            return;
        }

        EnsureDir("Assets/Resources/Characters");

        var character = ScriptableObject.CreateInstance<CharacterData>();
        character.characterName = charName;
        character.description = charDesc;
        character.sortOrder = charSortOrder;
        character.themeColor = charColor;

        // Import and assign portrait
        if (charPortrait != null)
        {
            string portraitPath = $"Assets/Resources/Characters/{charName}_portrait.png";
            SaveTextureAsSprite(charPortrait, portraitPath);
            character.portrait = AssetDatabase.LoadAssetAtPath<Sprite>(portraitPath);
        }

        // Import and assign full art
        if (charFullArt != null)
        {
            string fullArtPath = $"Assets/Resources/Characters/{charName}_fullart.png";
            SaveTextureAsSprite(charFullArt, fullArtPath);
            character.fullArt = AssetDatabase.LoadAssetAtPath<Sprite>(fullArtPath);
        }

        string assetPath = $"Assets/Resources/Characters/{charName}.asset";
        AssetDatabase.CreateAsset(character, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Character '{charName}' created at {assetPath}");
        ClearCharacterFields();
    }

    // ===== Song =====

    private void DrawSongSection()
    {
        GUILayout.Label("CREATE SONG", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        songName = EditorGUILayout.TextField("Song Name", songName);
        songArtist = EditorGUILayout.TextField("Artist", songArtist);
        songSortOrder = EditorGUILayout.IntField("Sort Order", songSortOrder);
        characterName = EditorGUILayout.TextField("Character Name", characterName);
        bpm = EditorGUILayout.FloatField("BPM", bpm);

        songCover = (Texture2D)EditorGUILayout.ObjectField("Cover Art Image", songCover, typeof(Texture2D), false);

        EditorGUILayout.LabelField("Difficulties", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        easyLv = EditorGUILayout.IntField("Easy", easyLv);
        hardLv = EditorGUILayout.IntField("Hard", hardLv);
        chaosLv = EditorGUILayout.IntField("Chaos", chaosLv);
        glitchLv = EditorGUILayout.IntField("Glitch", glitchLv);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Song", GUILayout.Height(30)))
            CreateSong();
        if (GUILayout.Button("Clear", GUILayout.Height(30)))
            ClearSongFields();
        EditorGUILayout.EndHorizontal();
    }

    private void CreateSong()
    {
        if (string.IsNullOrEmpty(songName))
        {
            EditorUtility.DisplayDialog("Error", "Song name is required.", "OK");
            return;
        }

        EnsureDir("Assets/Resources/Songs");

        var song = ScriptableObject.CreateInstance<SongData>();
        song.songName = songName;
        song.artist = string.IsNullOrEmpty(songArtist) ? characterName : songArtist;
        song.sortOrder = songSortOrder;
        song.bpm = bpm;
        song.easyLevel = easyLv;
        song.hardLevel = hardLv;
        song.chaosLevel = chaosLv;
        song.glitchLevel = glitchLv;

        // Import and assign cover art
        if (songCover != null)
        {
            string coverPath = $"Assets/Resources/Songs/{songName}_cover.png";
            SaveTextureAsSprite(songCover, coverPath);
            song.coverArt = AssetDatabase.LoadAssetAtPath<Sprite>(coverPath);
        }

        // Link character
        string charAssetPath = $"Assets/Resources/Characters/{characterName}.asset";
        var character = AssetDatabase.LoadAssetAtPath<CharacterData>(charAssetPath);
        if (character != null)
            song.character = character;
        else
            Debug.LogWarning($"Character '{characterName}' not found at {charAssetPath}");

        string assetPath = $"Assets/Resources/Songs/{songName}.asset";
        AssetDatabase.CreateAsset(song, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Song '{songName}' created at {assetPath}");
        ClearSongFields();
    }

    // ===== Helpers =====

    private void SaveTextureAsSprite(Texture2D tex, string path)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        AssetDatabase.ImportAsset(path);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }
    }

    private void EnsureDir(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    private void ClearCharacterFields()
    {
        charName = "";
        charDesc = "";
        charSortOrder = 0;
        charPortrait = null;
        charFullArt = null;
        charColor = Color.white;
    }

    private void ClearSongFields()
    {
        songName = "";
        songArtist = "";
        songSortOrder = 0;
        songCover = null;
        characterName = "";
        bpm = 120f;
        easyLv = hardLv = chaosLv = glitchLv = 0;
    }
}
