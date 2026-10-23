using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cached PBR materials for the facility, built from <see cref="ProceduralTextures"/>.
/// Box meshes from <see cref="StealthVisuals.CreateBox"/> have UVs in metres, so each material's
/// tiling is 1 / (world size covered by one texture repeat).
/// </summary>
public static class FacilityMaterials
{
    private static ProceduralTextures.TextureSet floorSet, plasterSet, paintedSet, brushedSet, woodSet, cardboardSet, ceilingSet, concreteSet, plasticSet, stripeSet;
    private static readonly Dictionary<string, Material> cache = new();

    private static ProceduralTextures.TextureSet Painted => paintedSet ??= ProceduralTextures.PaintedMetal();
    private static ProceduralTextures.TextureSet PlasticSet => plasticSet ??= ProceduralTextures.Plastic();

    public static Material Floor => Get("Floor", () => Textured("Floor", floorSet ??= ProceduralTextures.FloorTiles(), Color.white, 2f));
    public static Material Wall => Get("Wall", () => Textured("Wall", plasterSet ??= ProceduralTextures.PaintedPlaster(), new Color(0.66f, 0.7f, 0.72f), 2f));
    public static Material Ceiling => Get("Ceiling", () => Textured("Ceiling", ceilingSet ??= ProceduralTextures.CeilingTiles(), Color.white, 1.2f));
    public static Material Concrete => Get("Concrete", () => Textured("Concrete", concreteSet ??= ProceduralTextures.RoughConcrete(), Color.white, 2f));
    public static Material BrushedMetal => Get("Brushed", () => Textured("Brushed", brushedSet ??= ProceduralTextures.BrushedMetal(), Color.white, 1f));
    public static Material Wood => Get("Wood", () => Textured("Wood", woodSet ??= ProceduralTextures.Wood(), Color.white, 1f));
    public static Material CrateWood => Get("CrateWood", () => Textured("CrateWood", woodSet ??= ProceduralTextures.Wood(), new Color(0.95f, 0.85f, 0.7f), 0.6f));
    public static Material Cardboard => Get("Cardboard", () => Textured("Cardboard", cardboardSet ??= ProceduralTextures.Cardboard(), Color.white, 0.5f));
    public static Material HazardStripes => Get("Stripes", () => Textured("Stripes", stripeSet ??= ProceduralTextures.HazardStripes(), Color.white, 1f));

    public static Material PaintedMetal(Color tint) => Get("Painted" + tint, () => Textured("PaintedMetal", Painted, tint, 1f));
    public static Material Plastic(Color tint) => Get("Plastic" + tint, () => Textured("Plastic", PlasticSet, tint, 0.5f));

    public static Material Glass(Color tint) => Get("Glass" + tint, () =>
    {
        var mat = StealthVisuals.CreateTransparent("Glass", tint, true);
        mat.SetFloat("_Smoothness", 0.95f);
        mat.SetFloat("_Metallic", 0f);
        return mat;
    });

    /// <summary>Self-lit surface (screens, LEDs, light diffusers, signs).</summary>
    public static Material Emissive(Color color, float intensity) => Get($"Emissive{color}{intensity}", () => CreateEmissive(color, intensity));

    public static Material Screen => Get("Screen", () =>
    {
        var mat = StealthVisuals.CreateLit("Screen", Color.black);
        mat.SetFloat("_Smoothness", 0.9f);
        mat.SetTexture("_EmissionMap", ProceduralTextures.ScreenUI());
        StealthVisuals.SetEmission(mat, Color.white * 1.4f);
        return mat;
    });

    /// <summary>A fresh (uncached) emissive material, for lights whose brightness is animated.</summary>
    public static Material CreateEmissive(Color color, float intensity)
    {
        var mat = StealthVisuals.CreateLit("Emissive", color * 0.2f);
        StealthVisuals.SetEmission(mat, color * intensity);
        return mat;
    }

    private static Material Get(string key, System.Func<Material> create)
    {
        if (!cache.TryGetValue(key, out var mat) || mat == null)
            cache[key] = mat = create();
        return mat;
    }

    private static Material Textured(string name, ProceduralTextures.TextureSet set, Color tint, float worldSize)
    {
        var mat = StealthVisuals.CreateLit(name, tint);
        mat.SetTexture("_BaseMap", set.Albedo);
        mat.mainTextureScale = Vector2.one / worldSize;

        mat.SetTexture("_BumpMap", set.Normal);
        mat.SetFloat("_BumpScale", 1f);
        mat.EnableKeyword("_NORMALMAP");

        mat.SetTexture("_MetallicGlossMap", set.Mask);
        mat.SetFloat("_Metallic", 1f);
        mat.SetFloat("_Smoothness", 1f);
        mat.EnableKeyword("_METALLICSPECGLOSSMAP");
        return mat;
    }
}
