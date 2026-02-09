using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("Main Containers")]
    [SerializeField] private GameObject mainCanvasObject;
    [SerializeField] private RectTransform dialoguePanelRect;

    [Header("Global Material Control")]
    [SerializeField] private Material globalSharedMaterial;
    public Material GlobalMaterial => globalSharedMaterial;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private GameObject optionButtonPrefab;

    [Header("Tiempos y Ritmos")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float panelAnimationSpeed = 0.5f;
    [SerializeField] private float startInputCooldown = 0.5f;
    [SerializeField] private float nextSentenceCooldown = 0.5f;
    [SerializeField] private float buttonAppearanceCooldown = 1.0f;

    [Header("Audio Ajustes")]
    [Tooltip("Frecuencia del sonido de voz (1 = cada letra, 2 = cada dos letras...).")]
    [SerializeField] private int audioFrequency = 2;

    private Vector2 originalPanelPosition;

    // BACKUP DE MATERIAL
    private Color backupLightColor;
    private Color backupDarkColor;
    private float backupNoiseSpeed;
    private float backupNoiseScale;
    private float backupDitherThreshold;
    private float backupDitherStrength;
    private float backupSoftness;
    private float backupTextureBlend;

    private bool isTyping = false;
    private bool canAdvanceText = false;
    private DialogueNode currentNode;
    private int currentSegmentIndex = 0;

    public bool IsDialogueActive => mainCanvasObject.activeInHierarchy;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (globalSharedMaterial != null)
        {
            backupLightColor = globalSharedMaterial.GetColor("_LightColor");
            backupDarkColor = globalSharedMaterial.GetColor("_DarkColor");
            backupNoiseSpeed = globalSharedMaterial.GetFloat("_NoiseSpeed");
            backupNoiseScale = globalSharedMaterial.GetFloat("_NoiseScale");
            backupDitherThreshold = globalSharedMaterial.GetFloat("_DitherThreshold");
            backupDitherStrength = globalSharedMaterial.GetFloat("_DitherStrength");
            backupSoftness = globalSharedMaterial.GetFloat("_Softness");
            backupTextureBlend = globalSharedMaterial.GetFloat("_TextureBlend");
        }

        if (mainCanvasObject != null) mainCanvasObject.SetActive(false);
        if (dialoguePanelRect != null)
        {
            originalPanelPosition = dialoguePanelRect.anchoredPosition;
            dialoguePanelRect.anchoredPosition = new Vector2(originalPanelPosition.x, -Screen.height);
        }
    }

    private void OnDestroy()
    {
        RestoreMaterialDefaults();
    }

    public void RestoreMaterialDefaults()
    {
        if (globalSharedMaterial == null) return;
        globalSharedMaterial.SetColor("_LightColor", backupLightColor);
        globalSharedMaterial.SetColor("_DarkColor", backupDarkColor);
        globalSharedMaterial.SetFloat("_NoiseSpeed", backupNoiseSpeed);
        globalSharedMaterial.SetFloat("_NoiseScale", backupNoiseScale);
        globalSharedMaterial.SetFloat("_DitherThreshold", backupDitherThreshold);
        globalSharedMaterial.SetFloat("_DitherStrength", backupDitherStrength);
        globalSharedMaterial.SetFloat("_Softness", backupSoftness);
        globalSharedMaterial.SetFloat("_TextureBlend", backupTextureBlend);
    }

    public void StartDialogue(DialogueNode rootNode)
    {
        if (rootNode == null) return;

        if (dialogueText != null) dialogueText.text = string.Empty;
        ClearOptions();

        Time.timeScale = 0f;
        canAdvanceText = false;

        if (InputManager.Instance != null)
            InputManager.Instance.SwitchTo(InputManager.InputMapType.UI);

        if (mainCanvasObject != null) mainCanvasObject.SetActive(true);
        if (dialoguePanelRect != null) dialoguePanelRect.gameObject.SetActive(true);

        dialoguePanelRect.DOAnchorPos(originalPanelPosition, panelAnimationSpeed)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                DisplayNode(rootNode);
                StartCoroutine(EnableInputAfterCooldown(startInputCooldown));
            });
    }

    private IEnumerator EnableInputAfterCooldown(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        canAdvanceText = true;
    }

    private void DisplayNode(DialogueNode node)
    {
        currentNode = node;
        currentSegmentIndex = 0;
        ClearOptions();

        if (node == null || node.dialogueSequence == null || node.dialogueSequence.Length == 0)
        {
            EndDialogue();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(TypeSegment(node.dialogueSequence[currentSegmentIndex]));
    }

    IEnumerator TypeSegment(DialogueSegment segment)
    {
        isTyping = true;
        canAdvanceText = true;

        if (dialogueText != null) dialogueText.text = "";
        if (nameText != null) nameText.text = segment.speakerName;

        // --- CORRECCIÓN SHADER: Usamos tu lógica original ---
        if (segment.visualEffect.applyChanges && globalSharedMaterial != null && segment.visualEffect.preset != null)
        {
            ApplyShaderSettings(segment.visualEffect);
        }
        // ---------------------------------------------------

        char[] letters = segment.text.ToCharArray();
        for (int i = 0; i < letters.Length; i++)
        {
            if (dialogueText != null) dialogueText.text += letters[i];

            // --- SONIDO DE TIPEO (Estilo Undertale) ---
            if (currentNode.typingSound != null && AudioManager.Instance != null)
            {
                if (i % audioFrequency == 0)
                {
                    AudioManager.Instance.sfxSource.pitch = Random.Range(0.9f, 1.1f);
                    AudioManager.Instance.sfxSource.PlayOneShot(currentNode.typingSound);
                }
            }
            // ------------------------------------------

            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        if (AudioManager.Instance != null) AudioManager.Instance.sfxSource.pitch = 1f;

        isTyping = false;
        // La lógica DisplayNextSentence maneja el avance desde aquí
    }

    // --- CORRECCIÓN FUNCIÓN SHADER ---
    // Volvemos a leer desde 'settings.preset' que es lo que existe en tu proyecto
    void ApplyShaderSettings(ShaderSettings settings)
    {
        if (globalSharedMaterial == null || settings.preset == null) return;

        float duration = settings.transitionDuration;
        MaterialPresetSO p = settings.preset; // Referencia corta

        globalSharedMaterial.DOColor(p.lightColor, "_LightColor", duration).SetUpdate(true);
        globalSharedMaterial.DOColor(p.darkColor, "_DarkColor", duration).SetUpdate(true);
        globalSharedMaterial.DOFloat(p.noiseSpeed, "_NoiseSpeed", duration).SetUpdate(true);
        globalSharedMaterial.DOFloat(p.noiseScale, "_NoiseScale", duration).SetUpdate(true);
        globalSharedMaterial.DOFloat(p.ditherThreshold, "_DitherThreshold", duration).SetUpdate(true);
        globalSharedMaterial.DOFloat(p.ditherStrength, "_DitherStrength", duration).SetUpdate(true);
        globalSharedMaterial.DOFloat(p.softness, "_Softness", duration).SetUpdate(true);
        globalSharedMaterial.DOFloat(p.textureBlend, "_TextureBlend", duration).SetUpdate(true);
    }
    // --------------------------------

    public void DisplayNextSentence()
    {
        if (currentNode == null || !canAdvanceText) return;

        if (isTyping)
        {
            StopAllCoroutines();
            if (dialogueText != null && currentNode.dialogueSequence.Length > currentSegmentIndex)
            {
                dialogueText.text = currentNode.dialogueSequence[currentSegmentIndex].text;
            }
            isTyping = false;

            if (AudioManager.Instance != null) AudioManager.Instance.sfxSource.pitch = 1f;

            canAdvanceText = false;
            StartCoroutine(WaitAfterSentenceFinished());
            return;
        }

        if (currentSegmentIndex < currentNode.dialogueSequence.Length - 1)
        {
            currentSegmentIndex++;
            StartCoroutine(TypeSegment(currentNode.dialogueSequence[currentSegmentIndex]));
        }
        else
        {
            if (optionsContainer != null && optionsContainer.childCount == 0)
            {
                GenerateOptions(currentNode);
            }
        }
    }

    private IEnumerator WaitAfterSentenceFinished()
    {
        yield return new WaitForSecondsRealtime(nextSentenceCooldown);
        canAdvanceText = true;
    }

    void GenerateOptions(DialogueNode node)
    {
        canAdvanceText = false;
        if (dialogueText != null) dialogueText.text = "";
        ClearOptions();

        if (node.responses != null && node.responses.Length > 0)
        {
            foreach (DialogueResponse response in node.responses)
            {
                DialogueNode next = response.nextNode;
                CreateButton(response.responseText, () => OnOptionSelected(next), false);
            }
        }
        else
        {
            CreateButton("Cerrar", EndDialogue, false);
        }

        StartCoroutine(ActivateButtonsAfterDelay());
    }

    private IEnumerator ActivateButtonsAfterDelay()
    {
        yield return new WaitForSecondsRealtime(buttonAppearanceCooldown);
        Button[] buttons = optionsContainer.GetComponentsInChildren<Button>();
        foreach (Button btn in buttons) btn.interactable = true;

        if (buttons.Length > 0)
        {
            Canvas.ForceUpdateCanvases();
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
        }
    }

    void CreateButton(string text, UnityEngine.Events.UnityAction action, bool startInteractable)
    {
        if (optionButtonPrefab == null) return;
        GameObject btnObj = Instantiate(optionButtonPrefab, optionsContainer);
        btnObj.transform.localScale = Vector3.one;
        Vector3 localPos = btnObj.transform.localPosition;
        btnObj.transform.localPosition = new Vector3(localPos.x, localPos.y, 0);

        TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText) btnText.text = text;

        Button btn = btnObj.GetComponent<Button>();
        DialogueButtonAnim anim = btnObj.GetComponent<DialogueButtonAnim>();

        if (btn)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                btn.interactable = false;
                if (anim != null) anim.PlayClickAnimation(() => action.Invoke());
                else action.Invoke();
            });
            btn.interactable = startInteractable;
        }
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
        DialogueNode finishedNode = currentNode;
        ClearOptions();
        currentNode = null;

        dialoguePanelRect.DOAnchorPos(new Vector2(originalPanelPosition.x, -Screen.height), panelAnimationSpeed)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (dialogueText != null) dialogueText.text = string.Empty;
                if (mainCanvasObject != null) mainCanvasObject.SetActive(false);
                Time.timeScale = 1f;

                if (NarrativeManager.Instance != null && finishedNode != null)
                {
                    NarrativeManager.Instance.CheckForNarrativeEvents(finishedNode);
                }

                if (InputManager.Instance != null)
                    InputManager.Instance.SwitchTo(InputManager.InputMapType.Gameplay);
            });
    }
}