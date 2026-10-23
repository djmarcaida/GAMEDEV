using UnityEngine;

/// <summary>
/// Generates tileable PBR texture sets (albedo, normal, metallic/smoothness) at runtime,
/// so the facility can look realistic without any imported art assets.
/// </summary>
public static class ProceduralTextures
{
    public sealed class TextureSet
    {
        public Texture2D Albedo;
        public Texture2D Normal;
        public Texture2D Mask; // R = metallic, A = smoothness (URP Lit _MetallicGlossMap layout)
    }

    public delegate void Sampler(float u, float v, out Color albedo, out float height, out float smoothness, out float metallic);

    // ---------------------------------------------------------------- Noise

    private static float Hash(int x, int y, int seed)
    {
        unchecked
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + seed * 1442695041);
            h = (h ^ (h >> 13)) * 1274126177u;
            return (h ^ (h >> 16)) / (float)uint.MaxValue;
        }
    }

    private static int Wrap(int i, int period)
    {
        int m = i % period;
        return m < 0 ? m + period : m;
    }

    /// <summary>Value noise that tiles over u, v in [0, 1) with the given lattice periods.</summary>
    public static float Noise(float u, float v, int periodX, int periodY, int seed)
    {
        float x = u * periodX, y = v * periodY;
        int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
        float fx = x - x0, fy = y - y0;
        fx = fx * fx * (3f - 2f * fx);
        fy = fy * fy * (3f - 2f * fy);

        int xa = Wrap(x0, periodX), xb = Wrap(x0 + 1, periodX);
        int ya = Wrap(y0, periodY), yb = Wrap(y0 + 1, periodY);

        float a = Mathf.Lerp(Hash(xa, ya, seed), Hash(xb, ya, seed), fx);
        float b = Mathf.Lerp(Hash(xa, yb, seed), Hash(xb, yb, seed), fx);
        return Mathf.Lerp(a, b, fy);
    }

    /// <summary>Tileable fractal noise in [0, 1].</summary>
    public static float Fbm(float u, float v, int period, int octaves, int seed, int periodY = -1)
    {
        int px = period, py = periodY > 0 ? periodY : period;
        float sum = 0f, amp = 0.5f, norm = 0f;
        for (int i = 0; i < octaves; i++)
        {
            sum += amp * Noise(u, v, px, py, seed + i * 31);
            norm += amp;
            amp *= 0.5f;
            px *= 2;
            py *= 2;
        }
        return sum / norm;
    }

    private static float SmoothStep(float a, float b, float x)
    {
        float t = Mathf.Clamp01((x - a) / (b - a));
        return t * t * (3f - 2f * t);
    }

    private static float Frac(float x) => x - Mathf.Floor(x);

    // ---------------------------------------------------------------- Builder

    public static TextureSet Generate(string name, int size, float normalStrength, Sampler sampler)
    {
        int count = size * size;
        var albedo = new Color32[count];
        var mask = new Color32[count];
        var heights = new float[count];

        for (int y = 0; y < size; y++)
        {
            float v = (y + 0.5f) / size;
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size;
                sampler(u, v, out Color c, out float h, out float s, out float m);
                int i = y * size + x;
                albedo[i] = c;
                heights[i] = h;
                mask[i] = new Color32((byte)(Mathf.Clamp01(m) * 255f), 0, 0, (byte)(Mathf.Clamp01(s) * 255f));
            }
        }

        // Normal map from the height field (wrapping, so it tiles). Stored as RGB with A = 1, which URP's
        // UnpackNormalmapRGorAG decodes correctly on every platform.
        var normals = new Color32[count];
        float strength = normalStrength * size / 256f;
        for (int y = 0; y < size; y++)
        {
            int yu = (y + 1) % size, yd = (y - 1 + size) % size;
            for (int x = 0; x < size; x++)
            {
                int xr = (x + 1) % size, xl = (x - 1 + size) % size;
                float dx = (heights[y * size + xr] - heights[y * size + xl]) * strength;
                float dy = (heights[yu * size + x] - heights[yd * size + x]) * strength;
                Vector3 n = new Vector3(-dx, -dy, 1f).normalized;
                normals[y * size + x] = new Color32((byte)((n.x * 0.5f + 0.5f) * 255f), (byte)((n.y * 0.5f + 0.5f) * 255f), (byte)((n.z * 0.5f + 0.5f) * 255f), 255);
            }
        }

        return new TextureSet
        {
            Albedo = CreateTexture(name + "_Albedo", size, albedo, false),
            Normal = CreateTexture(name + "_Normal", size, normals, true),
            Mask = CreateTexture(name + "_Mask", size, mask, true),
        };
    }

    private static Texture2D CreateTexture(string name, int size, Color32[] pixels, bool linear, TextureWrapMode wrap = TextureWrapMode.Repeat)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, true, linear)
        {
            name = name,
            wrapMode = wrap,
            filterMode = FilterMode.Trilinear,
            anisoLevel = 8
        };
        tex.SetPixels32(pixels);
        tex.Apply(true, true);
        return tex;
    }

    // ---------------------------------------------------------------- Surfaces

    /// <summary>Polished concrete floor tiles. One texture = 2m x 2m (four 1m tiles).</summary>
    public static TextureSet FloorTiles() => Generate("FloorTiles", 512, 6f, (float u, float v, out Color c, out float h, out float s, out float m) =>
    {
        float tu = u * 2f, tv = v * 2f;
        int ix = Mathf.FloorToInt(tu), iy = Mathf.FloorToInt(tv);
        float fx = tu - ix, fy = tv - iy;
        float edge = Mathf.Min(Mathf.Min(fx, 1f - fx), Mathf.Min(fy, 1f - fy));
        float seam = SmoothStep(0.003f, 0.008f, edge);

        float broad = Fbm(u, v, 6, 5, 1);
        float fine = Fbm(u, v, 64, 3, 2);
        float stain = Fbm(u, v, 3, 4, 3);
        float tint = (Hash(ix, iy, 7) - 0.5f) * 0.06f;

        float g = 0.34f + (broad - 0.5f) * 0.12f + (fine - 0.5f) * 0.06f + tint;
        g *= Mathf.Lerp(1f, 0.7f, SmoothStep(0.58f, 0.8f, stain));
        g *= Mathf.Lerp(0.35f, 1f, seam);
        c = new Color(g * 1.02f, g, g * 0.95f);

        h = seam * (0.9f + fine * 0.1f);
        s = Mathf.Lerp(0.2f, 0.7f, Fbm(u, v, 2, 3, 4)) * Mathf.Lerp(0.3f, 1f, seam) * Mathf.Lerp(1f, 0.7f, fine);
        m = 0f;
    });

    /// <summary>Painted plaster wall (white, tint with material colour). One texture = 2m.</summary>
    public static TextureSet PaintedPlaster() => Generate("Plaster", 512, 1.5f, (float u, float v, out Color c, out float h, out float s, out float m) =>
    {
        float broad = Fbm(u, v, 8, 4, 11);
        float fine = Fbm(u, v, 128, 2, 12);
        float g = 0.82f + (broad - 0.5f) * 0.08f + (fine - 0.5f) * 0.05f;
        float grime = SmoothStep(0.62f, 0.85f, Fbm(u, v, 4, 4, 13));
        g *= Mathf.Lerp(1f, 0.8f, grime);
        c = new Color(g, g, g);
        h = fine * 0.6f + broad * 0.4f;
        s = 0.28f - grime * 0.12f;
        m = 0f;
    });

    /// <summary>Painted sheet metal with dents and bare-metal scratches. One texture = 1m.</summary>
    public static TextureSet PaintedMetal() => Generate("PaintedMetal", 256, 2f, (float u, float v, out Color c, out float h, out float s, out float m) =>
    {
        float dents = Fbm(u, v, 4, 3, 21);
        float fine = Fbm(u, v, 64, 2, 22);
        float scratch = 1f - Mathf.Abs(Noise(u, v, 3, 48, 23) * 2f - 1f);
        bool isScratch = scratch > 0.985f && Fbm(u, v, 6, 2, 24) > 0.55f;

        float g = 0.85f + (fine - 0.5f) * 0.05f + (dents - 0.5f) * 0.04f;
        c = isScratch ? new Color(1.1f, 1.1f, 1.1f) : new Color(g, g, g);
        h = dents * 0.5f + fine * 0.1f - (isScratch ? 0.15f : 0f);
        s = isScratch ? 0.65f : 0.42f + (fine - 0.5f) * 0.1f;
        m = isScratch ? 1f : 0f;
    });

    /// <summary>Brushed steel / aluminium. One texture = 1m, brushing runs along U.</summary>
    public static TextureSet BrushedMetal() => Generate("BrushedMetal", 256, 1f, (float u, float v, out Color c, out float h, out float s, out float m) =>
    {
        float streak = Noise(u, v, 2, 256, 31) * 0.6f + Noise(u, v, 4, 128, 32) * 0.4f;
        float smudge = Fbm(u, v, 3, 3, 33);
        float g = 0.58f + (streak - 0.5f) * 0.14f;
        c = new Color(g, g * 1.01f, g * 1.03f);
        h = streak * 0.3f;
        s = 0.55f + (streak - 0.5f) * 0.2f - SmoothStep(0.6f, 0.8f, smudge) * 0.2f;
        m = 1f;
    });

    /// <summary>Varnished wood grain. One texture = 1m, grain runs along U.</summary>
    public static TextureSet Wood() => Generate("Wood", 256, 2f, (float u, float v, out Color c, out float h, out float s, out float m) =>
    {
        float warp = Fbm(u, v, 4, 3, 41);
        float ring = 0.5f + 0.5f * Mathf.Sin((v * 10f + warp * 3f) * Mathf.PI * 2f);
        float grain = Noise(u, v, 4, 96, 42);
        float t = Mathf.Pow(ring, 0.7f) * 0.7f + grain * 0.3f;
        c = Color.Lerp(new Color(0.30f, 0.18f, 0.10f), new Color(0.60f, 0.42f, 0.25f), t);
        h = ring * 0.3f + grain * 0.4f;
        s = 0.42f + grain * 0.1f;
        m = 0f;
    });

    /// <summary>Corrugated cardboard. One texture = 1m.</summary>
    public static TextureSet Cardboard() => Generate("Cardboard", 256, 1.5f, (float u, float v, out Color c, out float h, out float s, out float m) =>
    {
        float n = Fbm(u, v, 8, 4, 51);
        float flute = 0.5f + 0.5f * Mathf.Sin(v * Mathf.PI * 2f * 60f);
        float g = 0.85f + (n - 0.5f) * 0.25f;
        c = new Color(0.58f * g, 0.44f * g, 0.28f * g);
        h = flute * 0.15f + n * 0.3f;
        s = 0.12f;
        m = 0f;
    });

    /// <summary>Acoustic ceiling tiles in a T-bar grid. One texture = 1.2m (four 0.6m tiles).</summary>
    public static TextureSet CeilingTiles() => Generate("CeilingTiles", 512, 4f, (float u, float v, out Color c, out float h, out float s, out float m) =>
    {
        float tu = u * 2f, tv = v * 2f;
        float fx = Frac(tu), fy = Frac(tv);
        float edge = Mathf.Min(Mathf.Min(fx, 1f - fx), Mathf.Min(fy, 1f - fy));
        bool bar = edge < 0.02f;

        if (bar)
        {
            c = new Color(0.8f, 0.8f, 0.78f);
            h = 1f;
            s = 0.5f;
            m = 0.2f;
            return;
        }

        float pits = Fbm(u, v, 128, 2, 61);
        float stain = Fbm(u, v, 3, 4, 62);
        float g = 0.72f - SmoothStep(0.55f, 0.75f, pits) * 0.25f;
        c = new Color(g, g * 0.99f, g * 0.96f);
        // Brown water-stain blotches with darker rims.
        float stainMask = SmoothStep(0.64f, 0.7f, stain);
        float rim = SmoothStep(0.64f, 0.66f, stain) * (1f - SmoothStep(0.66f, 0.7f, stain));
        c = Color.Lerp(c, new Color(0.55f, 0.45f, 0.3f), stainMask * 0.5f + rim * 0.4f);
        h = 0.6f - pits * 0.2f;
        s = 0.05f;
        m = 0f;
    });

    /// <summary>Rough cast concrete with pores. One texture = 2m.</summary>
    public static TextureSet RoughConcrete() => Generate("Concrete", 512, 4f, (float u, float v, out Color c, out float h, out float s, out float m) =>
    {
        float broad = Fbm(u, v, 6, 5, 71);
        float fine = Fbm(u, v, 32, 3, 72);
        float pore = Fbm(u, v, 96, 2, 73);
        bool isPore = pore < 0.25f;

        float g = 0.45f + (broad - 0.5f) * 0.2f + (fine - 0.5f) * 0.1f;
        if (isPore) g *= 0.55f;
        c = new Color(g * 1.03f, g, g * 0.95f);
        h = broad * 0.4f + fine * 0.4f - (isPore ? 0.3f : 0f);
        s = 0.12f + (broad - 0.5f) * 0.05f;
        m = 0f;
    });

    /// <summary>Fine-grained matte plastic / rubber / fabric. Tint with material colour. One texture = 1m.</summary>
    public static TextureSet Plastic() => Generate("Plastic", 256, 1f, (float u, float v, out Color c, out float h, out float s, out float m) =>
    {
        float fine = Fbm(u, v, 64, 3, 81);
        float g = 0.9f + (fine - 0.5f) * 0.1f;
        c = new Color(g, g, g);
        h = fine;
        s = 0.3f + (fine - 0.5f) * 0.1f;
        m = 0f;
    });

    /// <summary>Worn yellow/black diagonal hazard stripes. One texture = 1m.</summary>
    public static TextureSet HazardStripes() => Generate("HazardStripes", 256, 2f, (float u, float v, out Color c, out float h, out float s, out float m) =>
    {
        bool yellow = Frac((u + v) * 4f) < 0.5f;
        float wear = Fbm(u, v, 16, 4, 91);
        bool worn = wear > 0.66f;
        c = worn ? new Color(0.35f, 0.34f, 0.32f) : yellow ? new Color(0.85f, 0.62f, 0.05f) : new Color(0.04f, 0.04f, 0.04f);
        h = worn ? 0f : 0.5f;
        s = worn ? 0.15f : 0.35f;
        m = 0f;
    });

    // ---------------------------------------------------------------- Special textures

    /// <summary>Fake terminal UI used as a monitor's emission map.</summary>
    public static Texture2D ScreenUI()
    {
        const int size = 128;
        var px = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            int row = y / 6;
            float rowLength = 0.2f + Hash(row, 0, 5) * 0.7f;
            bool textRow = y % 6 < 3 && row > 1 && row < 19;
            for (int x = 0; x < size; x++)
            {
                float u = x / (float)size;
                bool header = y > size - 14;
                bool glyph = textRow && u > 0.08f && u < rowLength && Hash(x / 3, row, 9) > 0.25f;
                float scan = 0.85f + 0.15f * Mathf.Sin(y * Mathf.PI);
                Color c = header ? new Color(0.15f, 0.5f, 0.8f) : glyph ? new Color(0.4f, 0.85f, 1f) : new Color(0.02f, 0.06f, 0.12f);
                px[y * size + x] = c * scan;
            }
        }
        return CreateTexture("ScreenUI", size, px, false, TextureWrapMode.Clamp);
    }

    /// <summary>Flashlight cookie: hot centre, a bright reflector ring and a soft falloff, like a real torch.</summary>
    public static Texture2D FlashlightCookie()
    {
        const int size = 256;
        var px = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size * 2f - 1f, dy = (y + 0.5f) / size * 2f - 1f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float core = 1f - SmoothStep(0f, 0.35f, r);
                float ring = Mathf.Exp(-Mathf.Pow((r - 0.42f) / 0.05f, 2f)) * 0.35f;
                float spill = (1f - SmoothStep(0.3f, 0.98f, r)) * 0.45f;
                float imperfection = 0.92f + 0.08f * Mathf.Sin(r * 90f);
                float value = Mathf.Clamp01((core * 0.6f + ring + spill) * imperfection);
                byte b = (byte)(value * 255f);
                px[y * size + x] = new Color32(b, b, b, b);
            }
        }
        return CreateTexture("FlashlightCookie", size, px, true, TextureWrapMode.Clamp);
    }
}
