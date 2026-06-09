using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    RifleWeapon rifle;
    MissileWeapon missile;
    int selectedWeapon;

    public string CurrentWeaponName
    {
        get { return selectedWeapon == 0 ? "RIFLE [1]" : "ROCKET [2]"; }
    }

    void Awake()
    {
        rifle = GetComponent<RifleWeapon>();
        missile = GetComponent<MissileWeapon>();
        SelectWeapon(0);
    }

    void Update()
    {
        if (MainMenuGui.IsMenuOpen)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectWeapon(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectWeapon(1);
        }
    }

    void SelectWeapon(int index)
    {
        selectedWeapon = Mathf.Clamp(index, 0, 1);
        if (rifle != null)
        {
            rifle.enabled = selectedWeapon == 0;
        }

        if (missile != null)
        {
            missile.enabled = selectedWeapon == 1;
        }
    }
}
