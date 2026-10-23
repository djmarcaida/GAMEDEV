using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Synthesises realistic-sounding effects at runtime (no audio assets needed): a mechanical flashlight switch,
/// a quiet sensor tick, a facility klaxon, a pickup rustle and an electrical discharge.
/// </summary>
public class StealthAudio : MonoBehaviour
{
    private const int SampleRate = 44100;

    public static StealthAudio Instance { get; private set; }

    private AudioSource audioSource;
    private readonly Dictionary<string, AudioClip> clipCache = new();
    private readonly System.Random rng = new System.Random(1234);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void PlayFlashlightToggle(bool isOn) =>
        Play(isOn ? "SwitchOn" : "SwitchOff", () => CreateSwitchClick(isOn ? 1f : 0.85f), 0.7f);

    public void PlayDetectionPing(float suspicion01) =>
        Play("SensorTick", CreateSensorTick, 0.12f + suspicion01 * 0.18f);

    public void PlayAlarmSiren() => Play("Klaxon", CreateKlaxon, 0.55f);

    public void PlaySecretCollected() => Play("Pickup", CreatePickup, 0.8f);

    public void PlayEmpDischarge() => Play("Discharge", CreateDischarge, 0.9f);

    private void Play(string key, System.Func<AudioClip> create, float volume)
    {
        if (!clipCache.TryGetValue(key, out var clip))
            clipCache[key] = clip = create();
        audioSource.PlayOneShot(clip, volume);
    }

    // ---------------------------------------------------------------- Synthesis

    private float Noise() => (float)(rng.NextDouble() * 2.0 - 1.0);

    private static AudioClip ToClip(string name, float[] samples)
    {
        // Normalise to avoid clipping.
        float peak = 0.0001f;
        foreach (var s in samples) peak = Mathf.Max(peak, Mathf.Abs(s));
        for (int i = 0; i < samples.Length; i++) samples[i] *= 0.9f / peak;

        var clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>Plastic tail-cap switch: a sharp latch transient followed by a softer release click.</summary>
    private AudioClip CreateSwitchClick(float pitch)
    {
        var s = new float[(int)(SampleRate * 0.09f)];
        AddClick(s, 0f, 1f, 3200f * pitch);
        AddClick(s, 0.032f, 0.45f, 2400f * pitch);
        return ToClip("FlashlightSwitch", s);
    }

    private void AddClick(float[] s, float start, float gain, float resonance)
    {
        int offset = (int)(start * SampleRate);
        float lastNoise = 0f;
        for (int i = 0; offset + i < s.Length; i++)
        {
            float t = (float)i / SampleRate;
            float n = Noise();
            float highPassed = n - lastNoise; // crude high-pass gives the hard "tick"
            lastNoise = n;
            float body = Mathf.Sin(2f * Mathf.PI * resonance * t) * Mathf.Exp(-t * 600f);
            s[offset + i] += (highPassed * 0.5f * Mathf.Exp(-t * 2500f) + body * 0.6f) * gain;
        }
    }

    /// <summary>Quiet electronic sensor tick, like a scanner acquiring a target.</summary>
    private AudioClip CreateSensorTick()
    {
        var s = new float[(int)(SampleRate * 0.05f)];
        for (int i = 0; i < s.Length; i++)
        {
            float t = (float)i / SampleRate;
            float env = Mathf.Min(1f, t * 2000f) * Mathf.Exp(-t * 90f);
            s[i] = (Mathf.Sin(2f * Mathf.PI * 2100f * t) * 0.7f + Mathf.Sin(2f * Mathf.PI * 4200f * t) * 0.15f) * env;
        }
        return ToClip("SensorTick", s);
    }

    /// <summary>Two-tone facility klaxon: a band-limited buzzing horn with slight vibrato and room tail.</summary>
    private AudioClip CreateKlaxon()
    {
        const float duration = 1.6f;
        var s = new float[(int)(SampleRate * duration)];
        float phase = 0f, lowPass = 0f;

        for (int i = 0; i < s.Length; i++)
        {
            float t = (float)i / SampleRate;
            float freq = (t % 0.8f < 0.4f ? 440f : 370f) * (1f + 0.004f * Mathf.Sin(2f * Mathf.PI * 6f * t));
            phase += 2f * Mathf.PI * freq / SampleRate;

            // Sawtooth from its first harmonics: rich like a horn, without aliasing.
            float saw = 0f;
            for (int h = 1; h <= 12; h++)
                saw += Mathf.Sin(phase * h) / h;

            lowPass += 0.25f * (saw - lowPass);
            float env = Mathf.Min(1f, t * 40f) * Mathf.Min(1f, (duration - t) * 8f);
            s[i] = lowPass * env;
        }
        return ToClip("Klaxon", s);
    }

    /// <summary>Picking something up off a surface: a short scrape, a plastic tap and a fabric rustle.</summary>
    private AudioClip CreatePickup()
    {
        const float duration = 0.35f;
        var s = new float[(int)(SampleRate * duration)];
        float band = 0f, band2 = 0f;

        for (int i = 0; i < s.Length; i++)
        {
            float t = (float)i / SampleRate;
            float n = Noise();
            band += 0.35f * (n - band);   // low-pass
            band2 += 0.08f * (band - band2);
            float rustle = (band - band2) * (Mathf.Exp(-Mathf.Pow((t - 0.05f) / 0.03f, 2f)) + 0.6f * Mathf.Exp(-Mathf.Pow((t - 0.2f) / 0.06f, 2f)));
            float tap = t > 0.09f ? Mathf.Sin(2f * Mathf.PI * 1300f * (t - 0.09f)) * Mathf.Exp(-(t - 0.09f) * 220f) * 0.5f : 0f;
            s[i] = rustle * 1.6f + tap;
        }
        return ToClip("Pickup", s);
    }

    /// <summary>Electrical discharge: a bang, sparking crackle and a mains buzz dying away.</summary>
    private AudioClip CreateDischarge()
    {
        const float duration = 1.4f;
        var s = new float[(int)(SampleRate * duration)];
        float lowPass = 0f, spark = 0f;

        for (int i = 0; i < s.Length; i++)
        {
            float t = (float)i / SampleRate;
            float n = Noise();
            lowPass += 0.08f * (n - lowPass);
            float bang = lowPass * 3f * Mathf.Exp(-t * 18f);

            // Crackle: random impulses that thin out over time.
            if (rng.NextDouble() < 0.004 * Mathf.Exp(-t * 2.5f)) spark = 1f;
            spark *= 0.985f;
            float crackle = spark * n;

            float hum = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 100f * t)) * 0.15f + Mathf.Sin(2f * Mathf.PI * 200f * t) * 0.1f;
            s[i] = bang + crackle * 0.8f + hum * Mathf.Exp(-t * 2.2f) * Mathf.Min(1f, t * 30f);
        }
        return ToClip("Discharge", s);
    }
}
