using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public GameObject selectText;
    public GameObject selectPCButton;
    public GameObject selectMobileButton;

    public void OnPressStartButton()
    {
        selectText.SetActive(true);
        selectPCButton.SetActive(true);
        selectMobileButton.SetActive(true);
    }

    public void SelectPC()
    {
        PlayerPrefs.SetString("ControlScheme", "PC");
        PlayerPrefs.Save();
        CharacterIntroduction();
    }
    public void SelectMobile()
    {
        PlayerPrefs.SetString("ControlScheme", "Mobile");
        PlayerPrefs.Save();
        CharacterIntroduction();
    }

    public void CharacterIntroduction()
    {
        SceneManager.LoadScene("CharacterIntroductionScene");
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
        {
            OnPressStartButton();
        }
    }
}
