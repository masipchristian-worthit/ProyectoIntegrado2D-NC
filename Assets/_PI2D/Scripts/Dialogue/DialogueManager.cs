using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject mainCanvasObject;
    [SerializeField] private RectTransform dialoguePanelRect;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private GameObject optionButtonPrefab;

    [Header("Global Material Control")]
    [SerializeField] private Material globalSharedMaterial;
    public Material GlobalMaterial => globalSharedMaterial;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float panelAnimationSpeed = 0.5f;
    [SerializeField] private float startInputCooldown = 0.5f;
    [SerializeField] private float nextSentenceCooldown = 0.5f;
    [SerializeField] private float buttonAppearanceCooldown = 1.0f;
    [SerializeField] private int audioFrequency = 2;

    private Vector2 originalPanelPosition;

    // Memoria para el Shader (Persistencia)
    private Color currentLightColor;
    private Color currentDarkColor;
    private float currentNoiseSpeed, currentNoiseScale;
    private float currentDitherThreshold, currentDitherStrength;
    private float currentSoftness, currentTextureBlend;

    private bool isTyping = false;
    private bool canAdvanceText = false;
    private DialogueNode currentNode;
    private int currentSegmentIndex = 0;

    public bool IsDialogueActive
    {
        get
        {
            // Si el canvas ha sido destruido o es nulo, devolvemos false (no hay diálogo)
            // en lugar de lanzar un error que congele el juego.
            return mainCanvasObject != null && mainCanvasObject.activeInHierarchy;
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            SaveCurrentMaterialStateToMemory();
        }
        else Destroy(gameObject);

        if (mainCanvasObject != null) mainCanvasObject.SetActive(false);
        if (dialoguePanelRect != null)
        {
            originalPanelPosition = dialoguePanelRect.anchoredPosition;
            dialoguePanelRect.anchoredPosition = new Vector2(originalPanelPosition.x, -Screen.height);
        }
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ApplyMemoryToMaterial();

    // --- START DIALOGUE ---
    public void StartDialogue(DialogueNode rootNode)
    {
        if (rootNode == null) return;
        if (NarrativeManager.Instance != null && NarrativeManager.Instance.IsDialogueBlocked(rootNode.name)) return;

        if (dialogueText != null) dialogueText.text = string.Empty;
        ClearOptions();

        Time.timeScale = 0f;
        canAdvanceText = false;

        if (InputManager.Instance != null) InputManager.Instance.SwitchTo(InputManager.InputMapType.UI);
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

    private void DisplayNode(DialogueNode node)
    {
        currentNode = node;
        currentSegmentIndex = 0;
        ClearOptions();

        // --- CONTROL DE MÚSICA ---
        if (AudioManager.Instance != null)
        {
            if (node.stopMusic) AudioManager.Instance.PauseMusic();
            if (node.resumeMusic) AudioManager.Instance.ResumeMusic();
        }

        if (node.dialogueSequence == null || node.dialogueSequence.Length == 0)
        {
            FinishCurrentNode();
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

        if (segment.visualEffect.applyChanges && segment.visualEffect.preset != null)
            ApplyShaderSettings(segment.visualEffect);

        char[] letters = segment.text.ToCharArray();
        for (int i = 0; i < letters.Length; i++)
        {
            if (dialogueText != null) dialogueText.text += letters[i];
            if (currentNode.typingSound != null && AudioManager.Instance != null && i % audioFrequency == 0)
            {
                AudioManager.Instance.sfxSource.pitch = Random.Range(0.9f, 1.1f);
                AudioManager.Instance.sfxSource.PlayOneShot(currentNode.typingSound);
            }
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
        if (AudioManager.Instance != null) AudioManager.Instance.sfxSource.pitch = 1f;
        isTyping = false;
    }

    public void DisplayNextSentence()
    {
        if (currentNode == null || !canAdvanceText) return;

        if (isTyping)
        {
            StopAllCoroutines();
            if (dialogueText != null) dialogueText.text = currentNode.dialogueSequence[currentSegmentIndex].text;
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
            FinishCurrentNode();
        }
    }

    private void FinishCurrentNode()
    {
        // 1. GESTIÓN DE BLOQUEO
        // Solo bloqueamos el diálogo para el futuro, pero NO ejecutamos eventos (carga de escena) todavía.
        if (NarrativeManager.Instance != null && currentNode.lockAfterCompletion)
        {
            NarrativeManager.Instance.BlockDialogue(currentNode.name);
        }

        // 2. CASO: HAY SIGUIENTE NODO (Encadenado)
        if (currentNode.nextNode != null)
        {
            // Como NO vamos a pasar por EndDialogue, aquí SÍ debemos registrar el evento manualmente
            // para guardar flags o dar items antes de pasar a la siguiente frase.
            if (NarrativeManager.Instance != null)
                NarrativeManager.Instance.CheckForNarrativeEvents(currentNode);

            DisplayNode(currentNode.nextNode);
            return;
        }

        // 3. CASO: HAY RESPUESTAS (Opciones)
        if (currentNode.responses != null && currentNode.responses.Length > 0)
        {
            // Mostramos opciones y esperamos input del jugador.
            GenerateOptions(currentNode);
            return;
        }

        // 4. CASO: FIN DEL DIÁLOGO (Aquí estaba el error duplicado)
        // Ya NO llamamos a CheckForNarrativeEvents aquí arriba.
        // Dejamos que EndDialogue() se encargue de llamarlo tras la animación y el delay.
        EndDialogue();
    }

    public void EndDialogue()
    {
        DialogueNode finishedNode = currentNode;
        ClearOptions();
        currentNode = null;

        dialoguePanelRect.DOAnchorPos(new Vector2(originalPanelPosition.x, -Screen.height), panelAnimationSpeed)
            .SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
            {
                if (mainCanvasObject != null) mainCanvasObject.SetActive(false);
                Time.timeScale = 1f;

                if (finishedNode != null && finishedNode.changeSceneOnEnd)
                    StartCoroutine(WaitAndTriggerEvents(finishedNode, 1.5f));
                else
                {
                    if (NarrativeManager.Instance != null) NarrativeManager.Instance.CheckForNarrativeEvents(finishedNode);
                    if (InputManager.Instance != null) InputManager.Instance.SwitchTo(InputManager.InputMapType.Gameplay);
                }
            });
    }

    private IEnumerator WaitAndTriggerEvents(DialogueNode node, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (NarrativeManager.Instance != null) NarrativeManager.Instance.CheckForNarrativeEvents(node);
    }

    // --- MEMORIA Y SHADER ---
    private void SaveCurrentMaterialStateToMemory()
    {
        if (globalSharedMaterial == null) return;
        currentLightColor = globalSharedMaterial.GetColor("_LightColor");
        currentDarkColor = globalSharedMaterial.GetColor("_DarkColor");
        currentNoiseSpeed = globalSharedMaterial.GetFloat("_NoiseSpeed");
        currentNoiseScale = globalSharedMaterial.GetFloat("_NoiseScale");
        currentDitherThreshold = globalSharedMaterial.GetFloat("_DitherThreshold");
        currentDitherStrength = globalSharedMaterial.GetFloat("_DitherStrength");
        currentSoftness = globalSharedMaterial.GetFloat("_Softness");
        currentTextureBlend = globalSharedMaterial.GetFloat("_TextureBlend");
    }

    private void ApplyMemoryToMaterial()
    {
        if (globalSharedMaterial == null) return;
        globalSharedMaterial.SetColor("_LightColor", currentLightColor);
        globalSharedMaterial.SetColor("_DarkColor", currentDarkColor);
        globalSharedMaterial.SetFloat("_NoiseSpeed", currentNoiseSpeed);
        globalSharedMaterial.SetFloat("_NoiseScale", currentNoiseScale);
        globalSharedMaterial.SetFloat("_DitherThreshold", currentDitherThreshold);
        globalSharedMaterial.SetFloat("_DitherStrength", currentDitherStrength);
        globalSharedMaterial.SetFloat("_Softness", currentSoftness);
        globalSharedMaterial.SetFloat("_TextureBlend", currentTextureBlend);
    }

    void ApplyShaderSettings(ShaderSettings settings)
    {
        if (globalSharedMaterial == null || settings.preset == null) return;
        float d = settings.transitionDuration;
        MaterialPresetSO p = settings.preset;

        globalSharedMaterial.DOColor(p.lightColor, "_LightColor", d).SetUpdate(true);
        globalSharedMaterial.DOColor(p.darkColor, "_DarkColor", d).SetUpdate(true);
        globalSharedMaterial.DOFloat(p.noiseSpeed, "_NoiseSpeed", d).SetUpdate(true);
        globalSharedMaterial.DOFloat(p.noiseScale, "_NoiseScale", d).SetUpdate(true);
        globalSharedMaterial.DOFloat(p.ditherThreshold, "_DitherThreshold", d).SetUpdate(true);
        globalSharedMaterial.DOFloat(p.ditherStrength, "_DitherStrength", d).SetUpdate(true);
        globalSharedMaterial.DOFloat(p.softness, "_Softness", d).SetUpdate(true);
        globalSharedMaterial.DOFloat(p.textureBlend, "_TextureBlend", d).SetUpdate(true);

        currentLightColor = p.lightColor; currentDarkColor = p.darkColor;
        currentNoiseSpeed = p.noiseSpeed; currentNoiseScale = p.noiseScale;
        currentDitherThreshold = p.ditherThreshold; currentDitherStrength = p.ditherStrength;
        currentSoftness = p.softness; currentTextureBlend = p.textureBlend;
    }

    // UTILS
    private IEnumerator EnableInputAfterCooldown(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        canAdvanceText = true;
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
        foreach (DialogueResponse response in node.responses)
            CreateButton(response.responseText, () => OnOptionSelected(response.nextNode));
        StartCoroutine(ActivateButtonsAfterDelay());
    }
    void CreateButton(string text, UnityEngine.Events.UnityAction action)
    {
        if (optionButtonPrefab == null) return;

        GameObject btnObj = Instantiate(optionButtonPrefab, optionsContainer);

        // --- FUERZA LA POSICIÓN Y ESCALA ---
        btnObj.transform.localScale = Vector3.one;
        btnObj.transform.localPosition = Vector3.zero; // Resetea posición local
        // -----------------------------------

        btnObj.GetComponentInChildren<TextMeshProUGUI>().text = text;
        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(() => { btn.interactable = false; action.Invoke(); });
        btn.interactable = false;
    }
    private IEnumerator ActivateButtonsAfterDelay()
    {
        yield return new WaitForSecondsRealtime(buttonAppearanceCooldown);
        Button[] buttons = optionsContainer.GetComponentsInChildren<Button>();
        foreach (Button btn in buttons) btn.interactable = true;
        if (buttons.Length > 0) { Canvas.ForceUpdateCanvases(); EventSystem.current.SetSelectedGameObject(buttons[0].gameObject); }
    }
    void OnOptionSelected(DialogueNode nextNode) => DisplayNode(nextNode != null ? nextNode : null);
    void ClearOptions() { foreach (Transform child in optionsContainer) Destroy(child.gameObject); }
}