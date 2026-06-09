#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ZombieSceneBuilderEditor
{
    const string MaterialFolder = "Assets/Generated/ZombieSceneMaterials";

    [InitializeOnLoadMethod]
    static void ScheduleSceneCleanup()
    {
        EditorApplication.delayCall += RemoveStartingMedikitsSilently;
    }

    [MenuItem("Tools/Zombie/Sahneyi Guncelle")]
    public static void UpdatePersistentScene()
    {
        int removed = RemoveStartingMedikits();
        EditorUtility.DisplayDialog(
            "Zombie Scene Builder",
            removed > 0
                ? removed + " baslangic can kiti sahneden kaldirildi ve sahne kaydedildi."
                : "Sahne zaten guncel. Baslangic can kiti bulunmuyor.",
            "Tamam"
        );
    }

    [MenuItem("Tools/Zombie/Sahneyi Kalici Olustur")]
    public static void BuildPersistentScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog(
                "Zombie Scene Builder",
                "Once Play modundan cikin, sonra bu komutu tekrar calistirin.",
                "Tamam"
            );
            return;
        }

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            EditorUtility.DisplayDialog(
                "Zombie Scene Builder",
                "Sahnede zaten bir Player var. Tekrar olusturma yapilmadi.",
                "Tamam"
            );
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Build Zombie Scene");

        GameObject builderObject = new GameObject("ZombieSurvivalBootstrapper");
        Undo.RegisterCreatedObjectUndo(builderObject, "Create Zombie Scene Builder");
        ZombieSurvivalBootstrapper builder =
            Undo.AddComponent<ZombieSurvivalBootstrapper>(builderObject);

        builder.BuildScene();
        SaveGeneratedMaterials();

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        Undo.CollapseUndoOperations(undoGroup);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Selection.activeGameObject = player != null ? player : builderObject;
        SceneView.lastActiveSceneView?.FrameSelected();

        EditorUtility.DisplayDialog(
            "Zombie Scene Builder",
            "Sahne kalici olarak olusturuldu ve kaydedildi.",
            "Tamam"
        );
    }

    static void SaveGeneratedMaterials()
    {
        EnsureFolder(MaterialFolder);
        Dictionary<Material, Material> savedMaterials = new Dictionary<Material, Material>();

        foreach (Renderer renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            Material[] materials = renderer.sharedMaterials;
            bool changed = false;

            for (int i = 0; i < materials.Length; i++)
            {
                Material source = materials[i];
                if (source == null || AssetDatabase.Contains(source))
                {
                    continue;
                }

                if (!savedMaterials.TryGetValue(source, out Material saved))
                {
                    saved = new Material(source);
                    string safeName = MakeSafeFileName(source.name);
                    string path = AssetDatabase.GenerateUniqueAssetPath(
                        $"{MaterialFolder}/{safeName}.mat"
                    );
                    AssetDatabase.CreateAsset(saved, path);
                    savedMaterials.Add(source, saved);
                }

                materials[i] = saved;
                changed = true;
            }

            if (changed)
            {
                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
            }
        }

        AssetDatabase.SaveAssets();
    }

    static void RemoveStartingMedikitsSilently()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        RemoveStartingMedikits();
    }

    static int RemoveStartingMedikits()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || string.IsNullOrEmpty(activeScene.path))
        {
            return 0;
        }

        int removed = 0;
        foreach (GameObject root in activeScene.GetRootGameObjects())
        {
            if (root.name != "Medikit")
            {
                continue;
            }

            Object.DestroyImmediate(root);
            removed++;
        }

        if (removed > 0)
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
        }

        return removed;
    }

    static void EnsureFolder(string folder)
    {
        string[] parts = folder.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }

    static string MakeSafeFileName(string value)
    {
        foreach (char invalid in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalid, '_');
        }

        return string.IsNullOrWhiteSpace(value) ? "Generated Material" : value;
    }
}
#endif
