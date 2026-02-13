using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Node", menuName = "Dialogue/Node")]
public class DialogueNode : ScriptableObject
{
    [Header("Audio")]
    public AudioClip typingSound;

    [Header("Secuencia")]
    public DialogueSegment[] dialogueSequence;

    [Header("Encadenamiento (Sin Opciones)")]
    [Tooltip("Arrastra aquí el siguiente DialogueNode para que salte automáticamente al terminar.")]
    public DialogueNode nextNode;

    [Tooltip("Si marcas esto, la música se pausará al empezar este diálogo.")]
    public bool stopMusic = false;
    [Tooltip("Si marcas esto, la música volverá a sonar al empezar este diálogo.")]
    public bool resumeMusic = false;

    // --- NUEVO 2: BLOQUEO ---
    [Header("Comportamiento al Finalizar")]
    [Tooltip("Si es TRUE, el jugador no podrá volver a iniciar este diálogo (útil para eventos únicos).")]
    public bool lockAfterCompletion = false;

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
    public GameObject objectToActivate;
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