using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 一键把 WwiseIntegration + AudioSystemManager 加入当前场景，
/// 并确保 WwiseGlobal（AkInitializer）存在。
/// 菜单: Cytus2 > Audio > Setup Audio Scene
/// </summary>
public static class AudioSceneSetup
{
    private const string AudioManagerName = "AudioManager";

    [MenuItem("Cytus2/Audio/Setup Audio Scene")]
    public static void Setup()
    {
        EnsureWwiseGlobal();
        EnsureAudioManager();

        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[AudioSetup] 完成: 已确保 WwiseGlobal 与 AudioManager 存在于当前场景");
    }

    private static void EnsureWwiseGlobal()
    {
        if (Object.FindObjectOfType<AkInitializer>() != null)
            return;

        var go = new GameObject("WwiseGlobal");
        go.AddComponent<AkInitializer>();
        Undo.RegisterCreatedObjectUndo(go, "Create WwiseGlobal");
    }

    private static void EnsureAudioManager()
    {
        var existing = Object.FindObjectOfType<AudioSystemManager>();
        if (existing != null) return;

        GameObject go = GameObject.Find(AudioManagerName);
        if (go == null)
        {
            go = new GameObject(AudioManagerName);
            Undo.RegisterCreatedObjectUndo(go, "Create AudioManager");
        }

        if (go.GetComponent<WwiseIntegration>() == null)
            Undo.AddComponent<WwiseIntegration>(go);
        if (go.GetComponent<AudioSystemManager>() == null)
            Undo.AddComponent<AudioSystemManager>(go);
    }
}
