using UnityEngine;

[CreateAssetMenu(fileName = "New Material Preset", menuName = "Dialogue/Material Preset")]
public class MaterialPresetSO : ScriptableObject
{
    [Header("Identificador")]
    public string presetName;

    [Header("Configuración de Shader (Datos Crudos)")]
    public Color lightColor = new Color(0.93f, 0.86f, 0.82f);
    public Color darkColor = new Color(0.2f, 0.3f, 0.18f);
    public float noiseSpeed = 1.0f;
    public float noiseScale = 0.05f;
    public float ditherThreshold = 0.5f;
    public float ditherStrength = 0.1f;
    public float softness = 0.01f;
    public float textureBlend = 0.2f;
}