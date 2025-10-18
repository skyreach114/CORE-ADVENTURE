using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterIntroductionManager : MonoBehaviour
{
    public void OnPressGameStartSelect()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}
