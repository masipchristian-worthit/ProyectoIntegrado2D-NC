using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Node", menuName = "Dialogue/Node")]
public class DialogueNode : ScriptableObject
{
    [Header("Audio")]
    public AudioClip typingSound;

    [Header("Secuencia")]
    public DialogueSegment[] dialogueSequence;

    [Header("Decisión Final")]
    public DialogueResponse[] responses;

    [Header("Eventos")]
    public GameObject prefabToSpawn;
    public string spawnPointTag;
    public string eventID;

    [Header("Transición de Escena")]
    public bool changeSceneOnEnd = false;
    // CAMBIO: Usamos int para el ID de la Build
    public int targetSceneIndex;
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
    [Tooltip("0 = Instantáneo, 0.2 = Rápido, 2.0+ = Lento")]
    [Range(0f, 5f)]
    public float transitionDuration = 0.5f;

    [Header("Referencia Obligatoria")]
    [Tooltip("Arrastra aquí el MaterialPresetSO con los colores y valores.")]
    public MaterialPresetSO preset;
}