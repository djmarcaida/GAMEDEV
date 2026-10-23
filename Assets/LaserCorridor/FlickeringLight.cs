using UnityEngine;

/// <summary>
/// Animates a light and its emissive fixture together: faulty fluorescent flicker, or a slow emergency pulse.
/// </summary>
public class FlickeringLight : MonoBehaviour
{
    public enum Mode
    {
        Faulty,
        Pulse
    }

    private Light target;
    private Material emissive;
    private Color emissionColor;
    private float baseIntensity;
    private Mode mode;

    private float stateTimer;
    private bool lit = true;
    private float level = 1f;
    private float phase;

    public void Setup(Light light, Material fixtureMaterial, Color emission, Mode flickerMode)
    {
        target = light;
        emissive = fixtureMaterial;
        emissionColor = emission;
        baseIntensity = light.intensity;
        mode = flickerMode;
        stateTimer = Random.Range(0f, 2f);
        phase = Random.Range(0f, 100f);
    }

    private void Update()
    {
        if (target == null) return;

        if (mode == Mode.Pulse)
        {
            level = 0.35f + 0.65f * (0.5f + 0.5f * Mathf.Sin(Time.time * 2.2f + phase));
        }
        else
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                lit = !lit;
                // Mostly on with short dropouts, plus the occasional long blackout.
                stateTimer = lit ? Random.Range(0.05f, 2.5f) : Random.value < 0.15f ? Random.Range(0.6f, 1.8f) : Random.Range(0.03f, 0.15f);
            }
            float buzz = 0.9f + 0.1f * Mathf.PerlinNoise(Time.time * 18f, phase);
            level = Mathf.MoveTowards(level, lit ? buzz : 0.02f, Time.deltaTime * 25f);
        }

        target.intensity = baseIntensity * level;
        target.enabled = level > 0.03f;
        if (emissive != null)
            emissive.SetColor("_EmissionColor", emissionColor * level);
    }
}
