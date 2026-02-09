using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    [Header("Referencias")]
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private CanvasGroup fadeOverlay;

    [Header("Configuración")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private int sortOrder = 30000;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupCanvas();
            if (fadeOverlay != null) { fadeOverlay.alpha = 1f; fadeOverlay.blocksRaycasts = true; }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void Start() => FadeIn();
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupCanvas();
        FadeIn();
    }

    private void SetupCanvas()
    {
        if (rootCanvas != null)
        {
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.sortingOrder = sortOrder;
        }
    }

    private void FadeIn()
    {
        if (fadeOverlay == null) return;
        Time.timeScale = 0f;
        if (InputManager.Instance != null) InputManager.Instance.SwitchTo(InputManager.InputMapType.UI);

        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.DOKill();
        fadeOverlay.alpha = 1f;
        fadeOverlay.blocksRaycasts = true;

        fadeOverlay.DOFade(0f, fadeDuration).SetUpdate(true).SetEase(Ease.Linear).OnComplete(() =>
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
            Time.timeScale = 1f;
            if (InputManager.Instance != null) InputManager.Instance.SwitchTo(InputManager.InputMapType.Gameplay);
        });
    }

    // --- CAMBIO PRINCIPAL: ACEPTAMOS INT ---
    public void LoadSceneWithFade(int sceneIndex)
    {
        StartCoroutine(TransitionSequence(sceneIndex));
    }

    private IEnumerator TransitionSequence(int sceneIndex)
    {
        Time.timeScale = 0f;
        if (InputManager.Instance != null) InputManager.Instance.SwitchTo(InputManager.InputMapType.UI);

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.DOKill();
            fadeOverlay.blocksRaycasts = true;
            yield return fadeOverlay.DOFade(1f, fadeDuration).SetUpdate(true).WaitForCompletion();
        }

        Debug.Log($"[TransitionManager] Cargando escena ID: {sceneIndex}");
        SceneManager.LoadScene(sceneIndex);
    }
}