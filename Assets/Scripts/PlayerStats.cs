using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    int zombiesKilled;

    public int ZombiesKilled
    {
        get { return zombiesKilled; }
        set { zombiesKilled = Mathf.Max(0, value); }
    }
}
