using UnityEngine;

public class EnemyDrops : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)] float chanceToDrop = 30f;

    public void OnDeath()
    {
        if (Random.Range(0f, 100f) > chanceToDrop)
        {
            return;
        }

        CreateMedikit(transform.position + Vector3.up * 0.45f);
    }

    static void CreateMedikit(Vector3 position)
    {
        GameObject medikit = GameObject.CreatePrimitive(PrimitiveType.Cube);
        medikit.name = "Dropped Medikit";
        medikit.transform.position = position;
        medikit.transform.localScale = new Vector3(0.52f, 0.3f, 0.68f);

        Collider collider = medikit.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        Rigidbody body = medikit.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        medikit.AddComponent<MedikitPickup>();

        Renderer renderer = medikit.GetComponent<Renderer>();
        if (renderer != null)
        {
            Texture2D texture = Resources.Load<Texture2D>("Textures/MedikitTexture");
            renderer.material = texture != null
                ? CreateTextureMaterial("Medikit White Cross", texture)
                : CreateMaterial("Medikit White", Color.white);
        }

        Object.Destroy(medikit, 25f);
    }

    static Material CreateMaterial(string materialName, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.name = materialName;
        material.color = color;
        return material;
    }

    static Material CreateTextureMaterial(string materialName, Texture texture)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Texture");
        }

        Material material = new Material(shader);
        material.name = materialName;
        material.mainTexture = texture;
        return material;
    }
}
