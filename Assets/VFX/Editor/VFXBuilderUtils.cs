using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GamedevVFX.Editor
{
    internal static class VFXBuilderUtils
    {
        public const string BrackeysParticles = "Assets/VFX/Textures/Brackeys/Particles/";
        public const string BrackeysFlipbooks = "Assets/VFX/Textures/Brackeys/Flipbooks/";
        public const string KenneyDir = "Assets/VFX/Textures/Kenney/";
        public const string MaterialsDir = "Assets/VFX/Materials/";
        public const string PrefabsDir = "Assets/VFX/Prefabs/";
        public const string SettingsDir = "Assets/VFX/Settings/";
        public const string ScenePath = "Assets/Scenes/VFX_Showcase.unity";

        private static Mesh _cachedQuadMesh;

        public static void EnsureFolders()
        {
            Directory.CreateDirectory(MaterialsDir);
            Directory.CreateDirectory(PrefabsDir);
            Directory.CreateDirectory(SettingsDir);
            Directory.CreateDirectory("Assets/Scenes");
        }

        public static void ConfigureTextures()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/VFX/Textures" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                // Additive black-background textures have no meaningful alpha channel
                bool isOpaqueAdditive = path.EndsWith("fire_01_8x8.tga", StringComparison.OrdinalIgnoreCase)
                                     || path.EndsWith("kenney_gradient_radial.png", StringComparison.OrdinalIgnoreCase)
                                     || path.EndsWith("kenney_comet_trail_beam.png", StringComparison.OrdinalIgnoreCase);

                bool changed = false;
                if (importer.textureType != TextureImporterType.Default) { importer.textureType = TextureImporterType.Default; changed = true; }
                if (importer.wrapMode != TextureWrapMode.Clamp) { importer.wrapMode = TextureWrapMode.Clamp; changed = true; }
                if (importer.mipmapEnabled) { importer.mipmapEnabled = false; changed = true; }
                if (!isOpaqueAdditive && !importer.alphaIsTransparency) { importer.alphaIsTransparency = true; changed = true; }
                if (importer.filterMode != FilterMode.Bilinear) { importer.filterMode = FilterMode.Bilinear; changed = true; }

                if (changed) importer.SaveAndReimport();
            }
        }

        public static Material CreateParticleMaterial(
            string fileName,
            string texturePath,
            bool additive,
            float hdrStops = 0f,
            bool softParticles = false,
            float softFar = 0.5f)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null)
                throw new Exception("Missing shader: Universal Render Pipeline/Particles/Unlit");

            string matPath = MaterialsDir + fileName;
            Material mat = LoadOrCreateMaterial(matPath, shader);

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (tex == null)
                throw new Exception($"Missing texture: {texturePath}");

            // Shuriken vertex colors are 8-bit LDR, so HDR emission intensity must live in _BaseColor
            float linearMult = hdrStops > 0.001f ? Mathf.Pow(2f, hdrStops) : 1f;
            mat.SetTexture("_BaseMap", tex);
            mat.SetColor("_BaseColor", new Color(linearMult, linearMult, linearMult, 1f));

            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", additive ? 2f : 0f);
            mat.SetFloat("_ColorMode", 0f);
            mat.SetFloat("_Cull", 0f);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_AlphaClip", 0f);
            mat.SetFloat("_BlendOp", 0f);

            if (additive)
            {
                mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)BlendMode.One);
                mat.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
                mat.SetFloat("_DstBlendAlpha", (float)BlendMode.One);
            }
            else
            {
                mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
                mat.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            }

            mat.SetFloat("_SoftParticlesEnabled", softParticles ? 1f : 0f);
            mat.SetFloat("_SoftParticlesNearFadeDistance", 0f);
            mat.SetFloat("_SoftParticlesFarFadeDistance", softFar);
            mat.SetVector("_SoftParticleFadeParams", new Vector4(0f, 1f / Mathf.Max(0.001f, softFar), 0f, 0f));

            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = (int)RenderQueue.Transparent;

            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.DisableKeyword("_ALPHAMODULATE_ON");
            if (softParticles)
                mat.EnableKeyword("_SOFTPARTICLES_ON");
            else
                mat.DisableKeyword("_SOFTPARTICLES_ON");

            ValidateURPMaterial(mat);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static Material CreateLitMaterial(string fileName, Color color, float metallic, float smoothness)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                throw new Exception("Missing shader: Universal Render Pipeline/Lit");

            Material mat = LoadOrCreateMaterial(MaterialsDir + fileName, shader);
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static GameObject CreateRoot(string name, float duration, bool loop, bool prewarm = false)
        {
            var root = new GameObject(name);
            var ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = duration;
            main.loop = loop;
            main.prewarm = prewarm;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.None;

            var em = ps.emission;
            em.enabled = false;
            DisableShape(ps);
            root.GetComponent<ParticleSystemRenderer>().enabled = false;
            return root;
        }

        public static ParticleSystem CreateEmitter(GameObject parent, string name, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPos;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = true;
            return ps;
        }

        public static void DisableShape(ParticleSystem ps)
        {
            var shape = ps.shape;
            shape.enabled = false;
        }

        public static void SetBursts(ParticleSystem ps, float rateOverTime, params ParticleSystem.Burst[] bursts)
        {
            var em = ps.emission;
            em.rateOverTime = rateOverTime;
            if (bursts != null && bursts.Length > 0)
                em.SetBursts(bursts);
        }

        public static void SetColorOverLifetime(ParticleSystem ps, (float t, Color c)[] colors, (float t, float a)[] alphas)
        {
            var col = ps.colorOverLifetime;
            col.enabled = true;
            col.color = MakeGradient(colors, alphas);
        }

        public static void SetSizeOverLifetime(ParticleSystem ps, params (float t, float v)[] keys)
        {
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = MakeCurve(keys);
        }

        public static void SetRotationZ(ParticleSystem ps, float degPerSec)
        {
            var rol = ps.rotationOverLifetime;
            rol.enabled = true;
            rol.z = degPerSec * Mathf.Deg2Rad;
        }

        public static void SetRotationZ(ParticleSystem ps, float minDegPerSec, float maxDegPerSec)
        {
            var rol = ps.rotationOverLifetime;
            rol.enabled = true;
            rol.z = new ParticleSystem.MinMaxCurve(minDegPerSec * Mathf.Deg2Rad, maxDegPerSec * Mathf.Deg2Rad);
        }

        public static void SetFlipbook(ParticleSystem ps, int tilesX, int tilesY, int maxStartFrame)
        {
            var tsa = ps.textureSheetAnimation;
            tsa.enabled = true;
            tsa.mode = ParticleSystemAnimationMode.Grid;
            tsa.numTilesX = tilesX;
            tsa.numTilesY = tilesY;
            tsa.animation = ParticleSystemAnimationType.WholeSheet;
            tsa.timeMode = ParticleSystemAnimationTimeMode.Lifetime;
            tsa.frameOverTime = MakeCurve((0f, 0f), (1f, 1f));
            tsa.startFrame = new ParticleSystem.MinMaxCurve(0f, (float)maxStartFrame / (tilesX * tilesY));
            tsa.cycleCount = 1;
        }

        public static void ConfigureBillboard(ParticleSystem ps, Material mat, ParticleSystemRenderMode mode, float sortingFudge)
        {
            var rend = ps.GetComponent<ParticleSystemRenderer>();
            rend.renderMode = mode;
            rend.sharedMaterial = mat;
            rend.sortingFudge = sortingFudge;
        }

        public static void ConfigureQuadMesh(ParticleSystem ps, Material mat, float sortingFudge)
        {
            var rend = ps.GetComponent<ParticleSystemRenderer>();
            rend.renderMode = ParticleSystemRenderMode.Mesh;
            rend.mesh = GetQuadMesh();
            rend.alignment = ParticleSystemRenderSpace.Local;
            rend.sharedMaterial = mat;
            rend.sortingFudge = sortingFudge;
        }

        public static void ConfigureStretched(ParticleSystem ps, Material mat, float velocityScale, float lengthScale, float sortingFudge)
        {
            var rend = ps.GetComponent<ParticleSystemRenderer>();
            rend.renderMode = ParticleSystemRenderMode.Stretch;
            rend.cameraVelocityScale = 0f;
            rend.velocityScale = velocityScale;
            rend.lengthScale = lengthScale;
            rend.sharedMaterial = mat;
            rend.sortingFudge = sortingFudge;
        }

        public static GameObject SavePrefab(GameObject root, string fileName)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabsDir + fileName);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        public static Mesh GetQuadMesh()
        {
            if (_cachedQuadMesh != null) return _cachedQuadMesh;
            _cachedQuadMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
            if (_cachedQuadMesh == null)
            {
                GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                _cachedQuadMesh = temp.GetComponent<MeshFilter>().sharedMesh;
                UnityEngine.Object.DestroyImmediate(temp);
            }
            return _cachedQuadMesh;
        }

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }

        public static ParticleSystem.MinMaxGradient MakeRandomPalette(params Color[] colors)
        {
            var g = new Gradient { mode = GradientMode.Fixed };
            var cks = new GradientColorKey[colors.Length];
            for (int i = 0; i < colors.Length; i++)
                cks[i] = new GradientColorKey(colors[i], (i + 1f) / colors.Length);

            var aks = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };
            g.SetKeys(cks, aks);
            return new ParticleSystem.MinMaxGradient(g) { mode = ParticleSystemGradientMode.RandomColor };
        }

        public static ParticleSystem.MinMaxGradient MakeGradient((float t, Color c)[] colorKeys, (float t, float a)[] alphaKeys)
        {
            var g = new Gradient();
            var cks = new GradientColorKey[colorKeys.Length];
            for (int i = 0; i < colorKeys.Length; i++)
                cks[i] = new GradientColorKey(colorKeys[i].c, colorKeys[i].t);

            var aks = new GradientAlphaKey[alphaKeys.Length];
            for (int i = 0; i < alphaKeys.Length; i++)
                aks[i] = new GradientAlphaKey(alphaKeys[i].a, alphaKeys[i].t);

            g.SetKeys(cks, aks);
            return new ParticleSystem.MinMaxGradient(g);
        }

        public static ParticleSystem.MinMaxCurve MakeCurve(params (float t, float v)[] keys)
        {
            var kfs = new Keyframe[keys.Length];
            for (int i = 0; i < keys.Length; i++)
                kfs[i] = new Keyframe(keys[i].t, keys[i].v);

            var curve = new AnimationCurve(kfs);
            for (int i = 0; i < kfs.Length; i++)
                AnimationUtility.SetKeyBroken(curve, i, false);

            return new ParticleSystem.MinMaxCurve(1f, curve);
        }

        private static Material LoadOrCreateMaterial(string path, Shader shader)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }
            return mat;
        }

        private static void ValidateURPMaterial(Material mat)
        {
            try
            {
                foreach (Type t in TypeCache.GetTypesDerivedFrom<ShaderGUI>())
                {
                    if (t.FullName != "UnityEditor.Rendering.Universal.ShaderGUI.ParticlesUnlitShader") continue;
                    object gui = Activator.CreateInstance(t);
                    MethodInfo validate = t.GetMethod("ValidateMaterial", BindingFlags.Public | BindingFlags.Instance);
                    validate?.Invoke(gui, new object[] { mat });
                    break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"URP material validation failed for {mat.name}: {ex.Message}");
            }
        }
    }
}
