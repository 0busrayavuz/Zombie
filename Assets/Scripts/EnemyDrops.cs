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
        medikit.transform.localScale = new Vector3(0.7f, 0.45f, 0.9f);

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
            renderer.material = CreateMaterial("Dropped Medikit Green", new Color(0.08f, 0.68f, 0.22f));
        }

        CreateCrossBar(medikit.transform, new Vector3(0.42f, 0.08f, 0.12f));
        CreateCrossBar(medikit.transform, new Vector3(0.12f, 0.08f, 0.42f));
        Object.Destroy(medikit, 25f);
    }

    static void CreateCrossBar(Transform parent, Vector3 scale)
    {
        GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.name = "White Cross";
        bar.transform.SetParent(parent);
        bar.transform.localPosition = new Vector3(0f, 0.28f, 0f);
        bar.transform.localRotation = Quaternion.identity;
        bar.transform.localScale = scale;

        Collider collider = bar.GetComponent<Collider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }

        Renderer renderer = bar.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = CreateMaterial("Medikit White", Color.white);
        }
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
}
