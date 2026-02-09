using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int firstLevelSceneIndex = 1; // CAMBIO A INT

    // Asigna esto en el evento "OnClickDelayed" del botón
    public void StartGame()
    {
        if (TransitionManager.Instance != null)
            TransitionManager.Instance.LoadSceneWithFade(firstLevelSceneIndex);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(firstLevelSceneIndex);
    }

    public void QuitGame() => Application.Quit();
}