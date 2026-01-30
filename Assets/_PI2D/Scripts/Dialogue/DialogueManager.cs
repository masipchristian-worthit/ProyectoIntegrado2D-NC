using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private GameObject optionButtonPrefab;

    [Header("Shader Control")]
    [SerializeField] private Material despeloteMaterial; 

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private PlayerInput playerInput;

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
        Time.timeScale = 0f;
        playerInput.SwitchCurrentActionMap("UI");
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

        // 1. APLICAR CAMBIOS AL SHADER
        if (segment.visualEffect.applyChanges && despeloteMaterial != null)
        {
            ApplyShaderSettings(segment.visualEffect);
        }

        // 2. ESCRIBIR TEXTO
        foreach (char letter in segment.text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
    }

    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentNode.dialogueSequence[currentSegmentIndex].text;
            isTyping = false;
            
            if (currentSegmentIndex == currentNode.dialogueSequence.Length - 1)
            {
                GenerateOptions(currentNode);
            }
            return;
        }

        if (currentSegmentIndex < currentNode.dialogueSequence.Length - 1)
        {
            currentSegmentIndex++;
            StartCoroutine(TypeSegment(currentNode.dialogueSequence[currentSegmentIndex]));
        }
        else
        {
            if (optionsContainer.childCount > 0) return;
            GenerateOptions(currentNode);
        }
    }

    void ApplyShaderSettings(ShaderSettings settings)
    {
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
        if (node.responses != null && node.responses.Length > 0)
        {
            foreach (DialogueResponse response in node.responses)
            {
                CreateButton(response.responseText, () => OnOptionSelected(response.nextNode));
            }
        }
        else
        {
            CreateButton("Cerrar", EndDialogue);
        }
    }

    void CreateButton(string text, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = Instantiate(optionButtonPrefab, optionsContainer);
        TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if(btnText) btnText.text = text;
        
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
        dialoguePanel.SetActive(false);
        ClearOptions();
        Time.timeScale = 1f;
        playerInput.SwitchCurrentActionMap("Gameplay");
    }
}