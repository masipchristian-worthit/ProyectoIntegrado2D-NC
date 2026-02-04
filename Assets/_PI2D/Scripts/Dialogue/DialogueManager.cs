using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI nameText; // Asegúrate de asignar esto en el inspector si lo usas, o el script ignorará el nombre.
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private GameObject optionButtonPrefab;

    [Header("Shader Control")]
    [SerializeField] private Material despeloteMaterial;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f;

    // NOTA TÉCNICA: Se ha eliminado la referencia a PlayerInput para evitar conflictos de autoridad.

    // ESTADO INTERNO
    private bool isTyping = false;
    private DialogueNode currentNode;
    private int currentSegmentIndex = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueNode rootNode)
    {
        Time.timeScale = 0f; // Pausamos el tiempo físico

        // INTERVENCIÓN: Delegamos el cambio de mapa al InputManager central
        if (InputManager.Instance != null)
            InputManager.Instance.SwitchTo(InputManager.InputMapType.UI);

        dialoguePanel.SetActive(true);

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

        // Si tienes un campo para el nombre en la UI, lo actualizamos aquí
        if (nameText != null)
        {
            nameText.text = segment.speakerName;
        }

        // 1. APLICAR CAMBIOS AL SHADER
        if (segment.visualEffect.applyChanges && despeloteMaterial != null)
        {
            ApplyShaderSettings(segment.visualEffect);
        }

        // 2. ESCRIBIR TEXTO
        foreach (char letter in segment.text.ToCharArray())
        {
            dialogueText.text += letter;
            // Usamos WaitForSecondsRealtime porque Time.timeScale es 0
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
    }

    public void DisplayNextSentence()
    {
        // Si el texto se está escribiendo, lo completamos de golpe
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentNode.dialogueSequence[currentSegmentIndex].text;
            isTyping = false;

            // Si era la última frase, mostramos opciones inmediatamente
            if (currentSegmentIndex == currentNode.dialogueSequence.Length - 1)
            {
                GenerateOptions(currentNode);
            }
            return;
        }

        // Si hay más frases en la secuencia actual, avanzamos
        if (currentSegmentIndex < currentNode.dialogueSequence.Length - 1)
        {
            currentSegmentIndex++;
            StartCoroutine(TypeSegment(currentNode.dialogueSequence[currentSegmentIndex]));
        }
        else
        {
            // Si ya no hay frases y no se han generado opciones, las generamos
            if (optionsContainer.childCount == 0)
            {
                GenerateOptions(currentNode);
            }
        }
    }

    void ApplyShaderSettings(ShaderSettings settings)
    {
        if (despeloteMaterial == null) return;

        despeloteMaterial.SetColor("_LightColor", settings.lightColor);
        despeloteMaterial.SetColor("_DarkColor", settings.darkColor);
        despeloteMaterial.SetFloat("_NoiseSpeed", settings.noiseSpeed);
        despeloteMaterial.SetFloat("_NoiseScale", settings.noiseScale);
        despeloteMaterial.SetFloat("_DitherThreshold", settings.ditherThreshold);
        despeloteMaterial.SetFloat("_DitherStrength", settings.ditherStrength);
        despeloteMaterial.SetFloat("_Softness", settings.softness);
        despeloteMaterial.SetFloat("_TextureBlend", settings.textureBlend);
    }

    void GenerateOptions(DialogueNode node)
    {
        // Limpiamos opciones previas por seguridad
        ClearOptions();

        if (node.responses != null && node.responses.Length > 0)
        {
            foreach (DialogueResponse response in node.responses)
            {
                // Usamos una variable local para capturar el cierre en el loop lambda
                DialogueNode next = response.nextNode;
                CreateButton(response.responseText, () => OnOptionSelected(next));
            }
        }
        else
        {
            // Opción por defecto para cerrar si no hay ramas
            CreateButton("Cerrar", EndDialogue);
        }

        // Opcional: Seleccionar automáticamente el primer botón para navegación con mando
        if (optionsContainer.childCount > 0)
        {
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
        if (nextNode != null)
        {
            DisplayNode(nextNode);
        }
        else
        {
            EndDialogue();
        }
    }

    void ClearOptions()
    {
        foreach (Transform child in optionsContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        ClearOptions();
        Time.timeScale = 1f; // Restauramos el tiempo

        // SOLUCIÓN CRÍTICA: Forzamos el estado de Gameplay explícitamente.
        // Esto corrige el bug de quedarse atascado en UI al iniciar el juego.
        if (InputManager.Instance != null)
            InputManager.Instance.SwitchTo(InputManager.InputMapType.Gameplay);
        else
            Debug.LogError("InputManager Instance no encontrada. El jugador no podrá moverse.");
    }
}