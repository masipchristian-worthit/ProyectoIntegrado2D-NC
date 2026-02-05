using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Node", menuName = "Dialogue/Node")]
public class DialogueNode : ScriptableObject
{
    [Header("Secuencia Lineal")]
    public DialogueSegment[] dialogueSequence;

    [Header("Decisión Final")]
    public DialogueResponse[] responses;
}

[System.Serializable]
public class DialogueSegment
{
    public string speakerName;
    [TextArea(3, 10)]
    public string text;
    public ShaderSettings visualEffect;
}

[System.Serializable]
public class DialogueResponse
{
    public string responseText;
    public DialogueNode nextNode;
}

[System.Serializable]
public class ShaderSettings
{
    public bool applyChanges = false;

    [Header("Ritmo de la Transición")]
    [Tooltip("0 = Instantáneo, 0.2 = Abrupto/Rápido, 2.0+ = Lento/Relajado")]
    [Range(0f, 5f)]
    public float transitionDuration = 0.5f;

    [Tooltip("Si se asigna un preset, se ignorarán los valores manuales.")]
    public MaterialPresetSO preset;

    [Header("Ajustes Manuales")]
    public Color lightColor = new Color(0.93f, 0.86f, 0.82f);
    public Color darkColor = new Color(0.2f, 0.3f, 0.18f);
    public float noiseSpeed = 1.0f;
    public float noiseScale = 0.05f;
    public float ditherThreshold = 0.5f;
    public float ditherStrength = 0.1f;
    public float softness = 0.01f;
    public float textureBlend = 0.2f;

    public ShaderSettings GetFinalSettings()
    {
        if (preset != null) return preset.settings;
        return this;
    }
}