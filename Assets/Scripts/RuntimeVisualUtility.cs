using UnityEngine;

public static class RuntimeVisualUtility
{
    public static GameObject AttachModel(
        Transform parent,
        string modelResourcePath,
        float targetHeight,
        string baseTexturePath,
        string normalTexturePath = null,
        string secondaryTexturePath = null,
        string secondaryNormalPath = null,
        float localFloorOffset = 0f)
    {
        GameObject prefab = Resources.Load<GameObject>(modelResourcePath);
        if (prefab == null)
        {
            return null;
        }

        GameObject model = Object.Instantiate(prefab, parent);
        model.name = prefab.name + " Visual";
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        DisableImportedPhysics(model);
        FitToHeight(model, parent, targetHeight, localFloorOffset);
        ApplyMaterials(model, baseTexturePath, normalTexturePath, secondaryTexturePath, secondaryNormalPath);
        return model;
    }

    public static Material CreateLitMaterial(string materialName, Texture baseTexture, Texture normalTexture = null)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.name = materialName;

        if (baseTexture != null)
        {
            material.SetTexture("_BaseMap", baseTexture);
            material.color = Color.white;
        }

        if (normalTexture != null)
        {
            material.SetTexture("_BumpMap", normalTexture);
            material.EnableKeyword("_NORMALMAP");
        }

        return material;
    }

    static void ApplyMaterials(
        GameObject model,
        string baseTexturePath,
        string normalTexturePath,
        string secondaryTexturePath,
        string secondaryNormalPath)
    {
        Texture2D baseTexture = string.IsNullOrEmpty(baseTexturePath)
            ? null
            : Resources.Load<Texture2D>(baseTexturePath);
        Texture2D normalTexture = string.IsNullOrEmpty(normalTexturePath)
            ? null
            : Resources.Load<Texture2D>(normalTexturePath);
        Texture2D secondaryTexture = string.IsNullOrEmpty(secondaryTexturePath)
            ? null
            : Resources.Load<Texture2D>(secondaryTexturePath);
        Texture2D secondaryNormal = string.IsNullOrEmpty(secondaryNormalPath)
            ? null
            : Resources.Load<Texture2D>(secondaryNormalPath);

        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
        {
            bool useSecondary = secondaryTexture != null &&
                (renderer.name.ToLowerInvariant().Contains("weapon") ||
                 renderer.name.ToLowerInvariant().Contains("blade"));

            Texture selectedTexture = useSecondary ? secondaryTexture : baseTexture;
            Texture selectedNormal = useSecondary ? secondaryNormal : normalTexture;
            Material[] materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = CreateLitMaterial(
                    model.name + " Material " + i,
                    selectedTexture,
                    selectedNormal
                );
            }

            renderer.materials = materials;
        }
    }

    static void FitToHeight(GameObject model, Transform parent, float targetHeight, float localFloorOffset)
    {
        Bounds bounds;
        if (!TryGetBounds(model, out bounds) || bounds.size.y <= 0.001f)
        {
            return;
        }

        float scale = targetHeight / bounds.size.y;
        model.transform.localScale *= scale;

        if (TryGetBounds(model, out bounds))
        {
            float parentFloor = parent.position.y + localFloorOffset;
            model.transform.position += Vector3.up * (parentFloor - bounds.min.y);
        }
    }

    static bool TryGetBounds(GameObject root, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            bounds = new Bounds();
            return false;
        }

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return true;
    }

    static void DisableImportedPhysics(GameObject model)
    {
        foreach (Collider collider in model.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }

        foreach (Rigidbody body in model.GetComponentsInChildren<Rigidbody>(true))
        {
            body.isKinematic = true;
            body.detectCollisions = false;
        }
    }
}
