using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Shared helpers for the runtime-built stealth props (materials and collider-free primitives).
/// </summary>
public static class StealthVisuals
{
    private static Shader litShader;
    private static Shader unlitShader;

    private static Shader LitShader => litShader != null ? litShader : litShader = FindShader("Universal Render Pipeline/Lit", "Standard");
    private static Shader UnlitShader => unlitShader != null ? unlitShader : unlitShader = FindShader("Universal Render Pipeline/Unlit", "Unlit/Color");

    private static Shader FindShader(string preferred, string fallback)
    {
        var shader = Shader.Find(preferred);
        return shader != null ? shader : Shader.Find(fallback);
    }

    public static Material CreateLit(string name, Color color)
    {
        return new Material(LitShader) { name = name, color = color };
    }

    public static Material CreateUnlit(string name, Color color)
    {
        return new Material(UnlitShader) { name = name, color = color };
    }

    /// <summary>
    /// URP materials ignore colour alpha unless the surface type is switched to Transparent.
    /// </summary>
    public static Material CreateTransparent(string name, Color color, bool lit = false)
    {
        var mat = new Material(lit ? LitShader : UnlitShader) { name = name, color = color };
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
        mat.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0f);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)RenderQueue.Transparent;
        return mat;
    }

    public static void SetEmission(Material mat, Color emission)
    {
        if (mat == null || !mat.HasProperty("_EmissionColor")) return;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emission);
    }

    /// <summary>
    /// Creates a primitive with its collider removed, so props never block movement, raycasts or the flashlight.
    /// </summary>
    public static Renderer CreatePrimitive(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat, bool castShadows = true)
    {
        var obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        Object.DestroyImmediate(obj.GetComponent<Collider>());
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPos;
        obj.transform.localScale = localScale;

        var renderer = obj.GetComponent<Renderer>();
        renderer.sharedMaterial = mat;
        renderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
        return renderer;
    }

    private static readonly System.Collections.Generic.Dictionary<Vector3, Mesh> boxMeshes = new();

    /// <summary>
    /// Creates a box of the given size (unscaled transform) whose UVs are in metres, so tiled
    /// materials keep a constant texel density on any size of box.
    /// </summary>
    public static GameObject CreateBox(string name, Transform parent, Vector3 localPos, Vector3 size, Material mat, bool collider = false, float yaw = 0f)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPos;
        obj.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        obj.AddComponent<MeshFilter>().sharedMesh = GetBoxMesh(size);
        obj.AddComponent<MeshRenderer>().sharedMaterial = mat;
        if (collider)
            obj.AddComponent<BoxCollider>();
        return obj;
    }

    public static Mesh GetBoxMesh(Vector3 size)
    {
        if (boxMeshes.TryGetValue(size, out var cached) && cached != null)
            return cached;

        var verts = new System.Collections.Generic.List<Vector3>(24);
        var uvs = new System.Collections.Generic.List<Vector2>(24);
        var tris = new System.Collections.Generic.List<int>(36);
        Vector3 h = size * 0.5f;

        // normal, u axis, v axis (v is "up" on side faces so textures stay upright)
        AddFace(Vector3.right, Vector3.forward, Vector3.up);
        AddFace(Vector3.left, Vector3.back, Vector3.up);
        AddFace(Vector3.forward, Vector3.left, Vector3.up);
        AddFace(Vector3.back, Vector3.right, Vector3.up);
        AddFace(Vector3.up, Vector3.right, Vector3.forward);
        AddFace(Vector3.down, Vector3.right, Vector3.back);

        void AddFace(Vector3 n, Vector3 uAxis, Vector3 vAxis)
        {
            Vector3 c = Vector3.Scale(n, h);
            Vector3 du = Vector3.Scale(uAxis, h), dv = Vector3.Scale(vAxis, h);
            int start = verts.Count;
            Vector3[] corners = { c - du - dv, c - du + dv, c + du + dv, c + du - dv };
            foreach (var p in corners)
            {
                verts.Add(p);
                uvs.Add(new Vector2(Vector3.Dot(p, uAxis), Vector3.Dot(p, vAxis)));
            }

            // Unity treats Cross(b - a, c - a) as the front-face normal; flip winding if needed.
            bool flip = Vector3.Dot(Vector3.Cross(corners[1] - corners[0], corners[2] - corners[0]), n) < 0f;
            if (flip) tris.AddRange(new[] { start, start + 2, start + 1, start, start + 3, start + 2 });
            else tris.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
        }

        var mesh = new Mesh { name = $"Box_{size.x:0.##}x{size.y:0.##}x{size.z:0.##}" };
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        boxMeshes[size] = mesh;
        return mesh;
    }

    public static Light CreateLight(string name, Transform parent, Vector3 localPos, LightType type, float range, float intensity)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPos;
        var light = obj.AddComponent<Light>();
        light.type = type;
        light.range = range;
        light.intensity = intensity;
        return light;
    }
}
