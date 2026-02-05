using UnityEngine;

[CreateAssetMenu(fileName = "New Material Preset", menuName = "Dialogue/Material Preset")]
public class MaterialPresetSO : ScriptableObject
{
    [Header("Identificador")]
    public string presetName;

    [Header("Configuración de Shader")]
    public ShaderSettings settings;
    public void ApplyTo(ref ShaderSettings target)
    {
        target.lightColor = settings.lightColor;
        target.darkColor = settings.darkColor;
        target.noiseSpeed = settings.noiseSpeed;
        target.noiseScale = settings.noiseScale;
        target.ditherThreshold = settings.ditherThreshold;
        target.ditherStrength = settings.ditherStrength;
        target.softness = settings.softness;
        target.textureBlend = settings.textureBlend;
    }
}
