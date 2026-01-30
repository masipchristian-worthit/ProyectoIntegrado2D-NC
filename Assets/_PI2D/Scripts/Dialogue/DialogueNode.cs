using UnityEngine;

// Definimos que esto es un ARCHIVO que puedes crear con click derecho
[CreateAssetMenu(fileName = "New Dialogue Node", menuName = "Dialogue/Node")]
public class DialogueNode : ScriptableObject
{
    [Header("Secuencia Lineal")]
    [Tooltip("Añade aquí todas las frases seguidas que quieras antes de dar a elegir.")]
    public DialogueSegment[] dialogueSequence; 

    [Header("Decisión Final")]
    [Tooltip("Botones que saldrán al terminar la secuencia anterior.")]
    public DialogueResponse[] responses; 
}

// --- CLASES AUXILIARES (Serializables) ---

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
    public DialogueNode nextNode; // Ahora esto es una referencia a otro archivo
}

[System.Serializable]
public class ShaderSettings
{
    public bool applyChanges = false; 

    [Header("Colores")]
    public Color lightColor = new Color(0.93f, 0.86f, 0.82f);
    public Color darkColor = new Color(0.2f, 0.3f, 0.18f);

    [Header("Valores Numéricos")]
    public float noiseSpeed = 1.0f;
    public float noiseScale = 0.05f;
    public float ditherThreshold = 0.5f;
    public float ditherStrength = 0.1f;
    public float softness = 0.01f;
    public float textureBlend = 0.2f;
}