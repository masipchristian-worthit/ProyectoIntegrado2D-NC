using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private PlayerController2D playerController;
    [SerializeField] private int mainMenuSceneIndex = 0; // CAMBIO A INT

    public void ResumeGame()
    {
        if (playerController != null) playerController.TogglePause();
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        if (TransitionManager.Instance != null)
            TransitionManager.Instance.LoadSceneWithFade(mainMenuSceneIndex);
        else
            SceneManager.LoadScene(mainMenuSceneIndex);
    }
}