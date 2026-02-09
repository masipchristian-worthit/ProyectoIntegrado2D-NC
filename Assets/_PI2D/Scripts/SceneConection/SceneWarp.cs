using UnityEngine;

public class SceneWarp : MonoBehaviour
{
    [Header("Destino Normal (ID Build Settings)")]
    [SerializeField] private int normalSceneIndex; // CAMBIO A INT

    [Header("Destino Alternativo (ID Build Settings)")]
    [SerializeField] private int alternativeSceneIndex; // CAMBIO A INT

    private bool useAlternative = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerInteract")) ChangeScene();
    }

    private void ChangeScene()
    {
        int target = useAlternative ? alternativeSceneIndex : normalSceneIndex;

        if (TransitionManager.Instance != null)
            TransitionManager.Instance.LoadSceneWithFade(target);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(target);
    }

    public void ActivateAlternativeScene() => useAlternative = true;
}