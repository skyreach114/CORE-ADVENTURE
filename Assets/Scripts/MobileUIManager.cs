using UnityEngine;

public class MobileUIManager : MonoBehaviour
{
    public GameObject mobileUIPrefab; // Joystick + Buttons parent prefab

    void Awake()
    {
        string scheme = PlayerPrefs.GetString("ControlScheme", "PC");
        if (scheme == "Mobile")
        {
            Instantiate(mobileUIPrefab);
        }
    }
}
