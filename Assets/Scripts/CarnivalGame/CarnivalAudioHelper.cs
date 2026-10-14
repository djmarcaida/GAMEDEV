using UnityEngine;

namespace CarnivalGame
{
    public static class CarnivalAudioHelper
    {
        private const int SampleRate = 44100;

        public static AudioClip CreateThrowClip()
        {
            float duration = 0.28f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;
                float freq = Mathf.Lerp(180f, 340f, Mathf.Sin(progress * Mathf.PI));
                float noise = (Random.value * 2f - 1f) * 0.35f;
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.65f;
                float envelope = Mathf.Sin(progress * Mathf.PI);

                data[i] = (tone + noise) * envelope * 0.7f;
            }

            AudioClip clip = AudioClip.Create("ThrowWhoosh", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateTargetHitClip()
        {
            float duration = 0.22f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;
                float freq1 = Mathf.Lerp(520f, 160f, progress);
                float freq2 = Mathf.Lerp(310f, 90f, progress);
                float noise = (Random.value * 2f - 1f) * Mathf.Exp(-progress * 22f);
                float tone = (Mathf.Sin(2f * Mathf.PI * freq1 * t) + 0.5f * Mathf.Sin(2f * Mathf.PI * freq2 * t)) * 0.6f;
                float envelope = Mathf.Exp(-progress * 14f);

                data[i] = (tone + noise) * envelope * 0.9f;
            }

            AudioClip clip = AudioClip.Create("TargetHit", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateShelfHitClip()
        {
            float duration = 0.18f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;
                float freq = Mathf.Lerp(220f, 60f, progress);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t);
                float noise = (Random.value * 2f - 1f) * 0.3f;
                float envelope = Mathf.Exp(-progress * 20f);

                data[i] = (tone * 0.7f + noise) * envelope * 0.8f;
            }

            AudioClip clip = AudioClip.Create("ShelfHit", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateRoundStartClip()
        {
            float duration = 0.45f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            float[] notes = { 440f, 554.37f, 659.25f };
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                int noteIndex = Mathf.Clamp((int)(t / (duration / 3f)), 0, 2);
                float freq = notes[noteIndex];
                float noteT = t - (noteIndex * (duration / 3f));
                float env = Mathf.Exp(-noteT * 9f);

                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.65f;
            }

            AudioClip clip = AudioClip.Create("RoundStart", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateWinClip()
        {
            float duration = 0.85f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            float[] melody = { 523.25f, 659.25f, 783.99f, 1046.5f };
            float noteDuration = duration / 4f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                int noteIndex = Mathf.Clamp((int)(t / noteDuration), 0, 3);
                float freq = melody[noteIndex];
                float localT = t - (noteIndex * noteDuration);
                float env = Mathf.Exp(-localT * 6f);
                float harmonic = 0.3f * Mathf.Sin(4f * Mathf.PI * freq * t);

                data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) + harmonic) * env * 0.7f;
            }

            AudioClip clip = AudioClip.Create("WinFanfare", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateLoseClip()
        {
            float duration = 0.5f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;
                float freq = Mathf.Lerp(196f, 130f, progress);
                float square = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t)) * 0.4f;
                float env = 1f - progress;

                data[i] = square * env * 0.55f;
            }

            AudioClip clip = AudioClip.Create("LoseBuzzer", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateButtonClickClip()
        {
            float duration = 0.08f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = t / duration;
                float freq = Mathf.Lerp(880f, 1200f, progress);
                float env = Mathf.Exp(-progress * 30f);

                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.6f;
            }

            AudioClip clip = AudioClip.Create("ButtonClick", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
