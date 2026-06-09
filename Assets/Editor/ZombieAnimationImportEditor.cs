#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ZombieAnimationImportEditor
{
    static readonly string[] ModelPaths =
    {
        "Assets/Resources/Models/Player/Player.FBX",
        "Assets/Resources/Models/Zombie/ZombieMixamo.fbx"
    };

    static ZombieAnimationImportEditor()
    {
        EditorApplication.delayCall += PrepareAnimationsAutomatically;
    }

    [MenuItem("Tools/Zombie/Animasyonlari Hazirla")]
    public static void PrepareAnimations()
    {
        int changedCount = PrepareModelImports();

        EditorUtility.DisplayDialog(
            "Zombie Animasyon",
            changedCount > 0
                ? "Player kosu klipleri ve zombie animasyonlari hazirlandi."
                : "Animasyon import ayarlari zaten hazirdi.",
            "Tamam"
        );
    }

    static void PrepareAnimationsAutomatically()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        PrepareModelImports();
    }

    static int PrepareModelImports()
    {
        int changedCount = 0;

        foreach (string modelPath in ModelPaths)
        {
            ModelImporter importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null)
            {
                continue;
            }

            if (modelPath.EndsWith("Player.FBX"))
            {
                if (ConfigurePlayer(importer))
                {
                    importer.SaveAndReimport();
                    changedCount++;
                }
            }
            else if (ConfigureZombie(importer))
            {
                importer.SaveAndReimport();
                changedCount++;
            }
        }

        return changedCount;
    }

    static bool ConfigurePlayer(ModelImporter importer)
    {
        ModelImporterClipAnimation[] desired =
        {
            CreateClip("Idle", 0f, 74f),
            CreateClip("RunForward", 76f, 94f),
            CreateClip("RunBackward", 96f, 114f),
            CreateClip("RunRight", 116f, 134f),
            CreateClip("RunLeft", 136f, 154f)
        };

        bool clipsMatch = ClipsMatch(importer.clipAnimations, desired);
        bool settingsMatch =
            importer.animationType == ModelImporterAnimationType.Legacy &&
            importer.animationWrapMode == WrapMode.Loop;

        if (clipsMatch && settingsMatch)
        {
            return false;
        }

        importer.animationType = ModelImporterAnimationType.Legacy;
        importer.animationWrapMode = WrapMode.Loop;
        importer.clipAnimations = desired;
        return true;
    }

    static bool ConfigureZombie(ModelImporter importer)
    {
        if (importer.animationType == ModelImporterAnimationType.Legacy &&
            importer.animationWrapMode == WrapMode.Loop)
        {
            return false;
        }

        importer.animationType = ModelImporterAnimationType.Legacy;
        importer.animationWrapMode = WrapMode.Loop;
        return true;
    }

    static ModelImporterClipAnimation CreateClip(string name, float firstFrame, float lastFrame)
    {
        return new ModelImporterClipAnimation
        {
            name = name,
            takeName = "Take 001",
            firstFrame = firstFrame,
            lastFrame = lastFrame,
            loopTime = true,
            loopPose = true,
            lockRootHeightY = true,
            lockRootPositionXZ = true,
            lockRootRotation = true,
            keepOriginalPositionY = false,
            keepOriginalPositionXZ = false,
            keepOriginalOrientation = false
        };
    }

    static bool ClipsMatch(
        ModelImporterClipAnimation[] existing,
        ModelImporterClipAnimation[] desired)
    {
        if (existing == null || existing.Length != desired.Length)
        {
            return false;
        }

        for (int i = 0; i < desired.Length; i++)
        {
            if (existing[i].name != desired[i].name ||
                !Mathf.Approximately(existing[i].firstFrame, desired[i].firstFrame) ||
                !Mathf.Approximately(existing[i].lastFrame, desired[i].lastFrame))
            {
                return false;
            }
        }

        return true;
    }
}
#endif
