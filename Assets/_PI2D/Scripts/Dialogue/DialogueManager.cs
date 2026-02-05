using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening; // Importante: Requiere DOTween

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    [SerializeField] private RectTransform dialoguePanelRect; // Referencia al RectTransform para moverlo
    [SerializeField] private Image dialogueBackgroundImage;   // Solo para el material (Shader)
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private GameObject optionButtonPrefab;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float panelAnimationSpeed = 0.5f;

    // Posición original para saber dónde volver
    private Vector2 originalPanelPosition;
    private float offScreenYPosition = -1080f; // Ajusta según tu resolución, o usa Screen.height

    private bool isTyping = false;
    private DialogueNode currentNode;
    private int currentSegmentIndex = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Guardamos la posición de diseño (centro)
        if (dialoguePanelRect != null)
        {
            originalPanelPosition = dialoguePanelRect.anchoredPosition;
            // Lo ocultamos inicialmente moviéndolo abajo
            dialoguePanelRect.anchoredPosition = new Vector2(originalPanelPosition.x, -Screen.height);
            dialoguePanelRect.gameObject.SetActive(false);
        }
    }

    public void StartDialogue(DialogueNode rootNode)
    {
        Time.timeScale = 0f;

        if (InputManager.Instance != null)
            InputManager.Instance.SwitchTo(InputManager.InputMapType.UI);

        dialoguePanelRect.gameObject.SetActive(true);

        // Animación DOTween de entrada (Desde abajo con efecto rebote "OutBack")
        dialoguePanelRect.DOAnchorPos(originalPanelPosition, panelAnimationSpeed)
            .SetEase(Ease.OutBack)
            .SetUpdate(true); // Ignora Time.timeScale = 0

        DisplayNode(rootNode);
    }

    private void DisplayNode(DialogueNode node)
    {
        currentNode = node;
        currentSegmentIndex = 0;

        if (node == null || node.dialogueSequence == null || node.dialogueSequence.Length == 0)
        {
            EndDialogue();
            return;
        }

        ClearOptions();
        StopAllCoroutines();
        StartCoroutine(TypeSegment(node.dialogueSequence[currentSegmentIndex]));
    }

    IEnumerator TypeSegment(DialogueSegment segment)
    {
        isTyping = true;
        dialogueText.text = "";

        if (nameText != null) nameText.text = segment.speakerName;

        // Aplicar Shader con animación si es necesario
        if (segment.visualEffect.applyChanges && dialogueBackgroundImage != null)
        {
            ApplyShaderSettings(segment.visualEffect.GetFinalSettings());
        }

        foreach (char letter in segment.text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
    }

    // --- INTEGRACIÓN DOTWEEN PARA SHADER ---
    void ApplyShaderSettings(ShaderSettings settings)
    {
        if (dialogueBackgroundImage.material == null) return;

        Material mat = dialogueBackgroundImage.material;
        float duration = settings.transitionDuration; // Sacado del SO

        // Animamos los colores y valores usando DOTween
        // Usamos SetUpdate(true) porque el juego está en pausa (TimeScale 0)
        mat.DOColor(settings.lightColor, "_LightColor", duration).SetUpdate(true);
        mat.DOColor(settings.darkColor, "_DarkColor", duration).SetUpdate(true);

        mat.DOFloat(settings.noiseSpeed, "_NoiseSpeed", duration).SetUpdate(true);
        mat.DOFloat(settings.noiseScale, "_NoiseScale", duration).SetUpdate(true);
        mat.DOFloat(settings.ditherThreshold, "_DitherThreshold", duration).SetUpdate(true);
        mat.DOFloat(settings.ditherStrength, "_DitherStrength", duration).SetUpdate(true);
        mat.DOFloat(settings.softness, "_Softness", duration).SetUpdate(true);
        mat.DOFloat(settings.textureBlend, "_TextureBlend", duration).SetUpdate(true);
    }

    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentNode.dialogueSequence[currentSegmentIndex].text;
            isTyping = false;

            if (currentSegmentIndex == currentNode.dialogueSequence.Length - 1)
                GenerateOptions(currentNode);
            return;
        }

        if (currentSegmentIndex < currentNode.dialogueSequence.Length - 1)
        {
            currentSegmentIndex++;
            StartCoroutine(TypeSegment(currentNode.dialogueSequence[currentSegmentIndex]));
        }
        else
        {
            if (optionsContainer.childCount == 0) GenerateOptions(currentNode);
        }
    }

    void GenerateOptions(DialogueNode node)
    {
        ClearOptions();

        if (node.responses != null && node.responses.Length > 0)
        {
            foreach (DialogueResponse response in node.responses)
            {
                DialogueNode next = response.nextNode;
                CreateButton(response.responseText, () => OnOptionSelected(next));
            }
        }
        else
        {
            CreateButton("Cerrar", EndDialogue);
        }

        if (optionsContainer.childCount > 0)
        {
            // Seleccionar el primer botón para navegación con teclado/mando
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(optionsContainer.GetChild(0).gameObject);
        }
    }

    void CreateButton(string text, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = Instantiate(optionButtonPrefab, optionsContainer);
        TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText) btnText.text = text;

        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    void OnOptionSelected(DialogueNode nextNode)
    {
        if (nextNode != null) DisplayNode(nextNode);
        else EndDialogue();
    }

    void ClearOptions()
    {
        foreach (Transform child in optionsContainer) Destroy(child.gameObject);
    }

    public void EndDialogue()
    {
        ClearOptions();

        // Animación de Salida (Hacia abajo)
        dialoguePanelRect.DOAnchorPos(new Vector2(originalPanelPosition.x, -Screen.height), panelAnimationSpeed)
            .SetEase(Ease.InBack) // Efecto de anticipación antes de bajar
            .SetUpdate(true)
            .OnComplete(() =>
            {
                dialoguePanelRect.gameObject.SetActive(false);
                Time.timeScale = 1f;

                if (InputManager.Instance != null)
                    InputManager.Instance.SwitchTo(InputManager.InputMapType.Gameplay);
            });
    }
}