using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Función para usar en el Inspector de los botones
    public void LoadScene(int sceneIndex)
    {
        if (TransitionManager.Instance != null)
            TransitionManager.Instance.LoadSceneWithFade(sceneIndex);
        else
            SceneManager.LoadScene(sceneIndex);
    }
}