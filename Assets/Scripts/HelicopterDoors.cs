using UnityEngine;

public class HelicopterDoors : MonoBehaviour
{
    [SerializeField] bool requireHelicopterLanded = true;
    [SerializeField] Helicopter helicopter;

    void Awake()
    {
        if (helicopter == null)
        {
            helicopter = GetComponentInParent<Helicopter>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Transform root = other.transform.root;
        if (!root.CompareTag("Player"))
        {
            return;
        }

        if (!requireHelicopterLanded || helicopter == null || helicopter.HasLanded)
        {
            GameManager.WinPlayer();
        }
    }
}
