using System;
using System.IO;
using System.Reflection;
using GamedevVFX;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace GamedevVFX.Editor
{
    public static class BuildActivityVFX
    {
        private const string TexBrackeysParticles = "Assets/VFX/Textures/Brackeys/Particles/";
        private const string TexBrackeysFlipbooks = "Assets/VFX/Textures/Brackeys/Flipbooks/";
        private const string TexBrackeysPredrawn  = "Assets/VFX/Textures/Brackeys/Predrawn/";
        private const string TexKenney            = "Assets/VFX/Textures/Kenney/";
        private const string MatDir               = "Assets/VFX/Materials/";
        private const string PrefabDir            = "Assets/VFX/Prefabs/";
        private const string SettingsDir          = "Assets/VFX/Settings/";
        private const string ScenePath            = "Assets/Scenes/VFX_Showcase.unity";

        private static Mesh _cachedQuadMesh;

        [MenuItem("VFX Activity/Build All VFX Prefabs & Showcase Scene")]
        public static void BuildAll()
        {
            EnsureFolders();
            ConfigureTextures();

            // -----------------------------------------------------------------
            // 1. URP Particle Unlit Materials (Neutral Tonemapping + HDR Bloom)
            // -----------------------------------------------------------------
            // VFX 1: Extravagant Magical Girl Transformation + Sky Starfall
            var matMGCore      = CreateParticleMaterial(MatDir + "MAT_MG_CoreGlow_Add.mat",       TexBrackeysParticles + "circle_05_a.png",        true,  0.75f, true,  1.6f);
            var matMGPillar    = CreateParticleMaterial(MatDir + "MAT_MG_LightPillar_Add.mat",    TexBrackeysParticles + "trace_06_a.png",         true,  0.90f, true,  1.8f);
            var matMGSigil     = CreateParticleMaterial(MatDir + "MAT_MG_FloorSigil_Add.mat",     TexBrackeysParticles + "magic_01_a.png",         true,  1.10f, true,  0.6f);
            var matMGRing      = CreateParticleMaterial(MatDir + "MAT_MG_EnergyRing_Add.mat",     TexBrackeysParticles + "circle_02_a.png",        true,  1.15f, false, 0.5f);
            var matMGRibbon    = CreateParticleMaterial(MatDir + "MAT_MG_Ribbon_Add.mat",         TexBrackeysParticles + "twirl_02_a.png",         true,  1.05f, false, 0.5f);
            var matMGSparkle   = CreateParticleMaterial(MatDir + "MAT_MG_Sparkle_Add.mat",        TexBrackeysParticles + "star_08_a.png",          true,  1.25f, false, 0.5f);
            var matMGStar5Pt   = CreateParticleMaterial(MatDir + "MAT_MG_SkyStar5Pt_Add.mat",     TexBrackeysParticles + "symbol_02_a.png",        true,  1.15f, false, 0.5f);
            var matMGCometHead = CreateParticleMaterial(MatDir + "MAT_MG_CometHead_Add.mat",      TexBrackeysParticles + "star_09_a.png",          true,  1.35f, false, 0.5f);
            var matMGCometTail = CreateParticleMaterial(MatDir + "MAT_MG_CometTrail_Add.mat",     TexKenney + "kenney_comet_trail_beam.png",       true,  1.20f, false, 0.5f);
            var matMGSubStar   = CreateParticleMaterial(MatDir + "MAT_MG_GlitterPop_Add.mat",     TexBrackeysParticles + "star_07_a.png",          true,  1.35f, false, 0.5f);
            var matMGShockwave = CreateParticleMaterial(MatDir + "MAT_MG_Shockwave_Add.mat",      TexBrackeysParticles + "effect_02_a.png",        true,  1.10f, true,  1.8f);
            var matMGFlare     = CreateParticleMaterial(MatDir + "MAT_MG_Flare_Add.mat",          TexBrackeysParticles + "flare_01_a.png",         true,  1.35f, true,  1.2f);

            // VFX 2: Avatar-Style Lightning Bending (Azula / Ozai Electric Blue-White Plasma)
            var matLtCharge    = CreateParticleMaterial(MatDir + "MAT_Lt_ChargeOrb_Add.mat",      TexBrackeysParticles + "twirl_03_a.png",         true,  1.10f, false, 0.5f);
            var matLtCoreGlow  = CreateParticleMaterial(MatDir + "MAT_Lt_CoreGlow_Add.mat",       TexBrackeysParticles + "circle_05_a.png",        true,  0.90f, true,  1.5f);
            var matLtBoltBeam  = CreateParticleMaterial(MatDir + "MAT_Lt_BoltBeam_Add.mat",       TexBrackeysParticles + "spark_06_a.png",         true,  1.30f, true,  1.2f);
            var matLtWebArc    = CreateParticleMaterial(MatDir + "MAT_Lt_WebArc_Add.mat",         TexBrackeysParticles + "spark_02_a.png",         true,  1.15f, true,  1.5f);
            var matLtElecRing  = CreateParticleMaterial(MatDir + "MAT_Lt_ElectricRing_Add.mat",   TexBrackeysParticles + "circle_02_a.png",        true,  1.15f, false, 0.5f);
            var matLtSpear     = CreateParticleMaterial(MatDir + "MAT_Lt_PlasmaSpear_Add.mat",    TexBrackeysParticles + "trace_03_a.png",         true,  1.35f, false, 0.5f);
            var matLtSparks    = CreateParticleMaterial(MatDir + "MAT_Lt_Sparks_Add.mat",         TexBrackeysParticles + "star_07_a.png",          true,  1.35f, false, 0.5f);
            var matLtZapSub    = CreateParticleMaterial(MatDir + "MAT_Lt_GroundZap_Add.mat",      TexBrackeysParticles + "scorch_02_a.png",        true,  1.25f, true,  0.8f);
            var matLtOzoneMist = CreateParticleMaterial(MatDir + "MAT_Lt_OzoneMist_Alpha.mat",    TexBrackeysFlipbooks + "wispy_smoke_01_8x8.tga", false, 0.0f,  true,  2.2f);

            // VFX 3: Colossal Final Boss "Auramaxxing"
            var matBossSmoke   = CreateParticleMaterial(MatDir + "MAT_Boss_AbyssSmoke_Alpha.mat", TexBrackeysFlipbooks + "cloud_01_8x8.tga",       false, 0.0f,  true,  2.2f);
            var matBossSpikes  = CreateParticleMaterial(MatDir + "MAT_Boss_EnergySpike_Add.mat",  TexBrackeysParticles + "trace_06_a.png",         true,  1.40f, true,  1.0f);
            var matBossBolt    = CreateParticleMaterial(MatDir + "MAT_Boss_Lightning_Add.mat",    TexBrackeysParticles + "spark_02_a.png",         true,  1.60f, true,  1.6f);
            var matBossVertBlt = CreateParticleMaterial(MatDir + "MAT_Boss_VertBolt_Add.mat",     TexBrackeysParticles + "spark_06_a.png",         true,  1.65f, true,  1.6f);
            var matBossGeo     = CreateParticleMaterial(MatDir + "MAT_Boss_GeoAccent_Add.mat",    TexKenney + "kenney_decagon_pattern_0024.png",   true,  1.30f, false, 0.5f);
            var matBossRing    = CreateParticleMaterial(MatDir + "MAT_Boss_DreadRing_Add.mat",    TexBrackeysParticles + "circle_03_a.png",        true,  1.55f, true,  0.6f);
            var matBossSigil   = CreateParticleMaterial(MatDir + "MAT_Boss_FloorSigil_Add.mat",   TexBrackeysParticles + "magic_02_a.png",         true,  1.25f, true,  0.6f);

            AssetDatabase.SaveAssets();

            // 2. Build 3 Colossal VFX Prefabs
            GameObject prefab1 = BuildMagicalGirlPrefab(
                matMGCore, matMGPillar, matMGSigil, matMGRing, matMGRibbon,
                matMGSparkle, matMGStar5Pt, matMGCometHead, matMGCometTail,
                matMGSubStar, matMGShockwave, matMGFlare);

            GameObject prefab2 = BuildLightningBendingPrefab(
                matLtCharge, matLtCoreGlow, matLtBoltBeam, matLtWebArc,
                matLtElecRing, matLtSpear, matLtSparks, matLtZapSub, matLtOzoneMist);

            GameObject prefab3 = BuildFinalBossAuramaxxingPrefab(
                matBossSmoke, matBossSpikes, matBossBolt, matBossVertBlt,
                matBossGeo, matBossRing, matBossSigil);

            // 3. Build Grand Showcase Scene
            BuildShowcaseScene(prefab1, prefab2, prefab3);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[BuildActivityVFX] Successfully built all colossal URP materials, 3 VFX prefabs (Starfall Magical Girl, Lightning Bending, Colossal Boss Auramaxxing), and VFX_Showcase.unity!");
        }

        public static void BuildAndCaptureFromCLI()
        {
            BuildAll();
            string previewDir = Environment.GetEnvironmentVariable("VFX_PREVIEW_DIR");
            if (!string.IsNullOrEmpty(previewDir))
                CapturePreviews(previewDir);
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(MatDir);
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(SettingsDir);
            Directory.CreateDirectory("Assets/Scenes");
        }

        private static void ConfigureTextures()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/VFX/Textures" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

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

        private static Material CreateParticleMaterial(
            string matPath,
            string texPath,
            bool additive,
            float hdrExposureStops,
            bool softParticles,
            float softFar)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null)
                throw new Exception("Could not find shader 'Universal Render Pipeline/Particles/Unlit'.");

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex == null)
                throw new Exception($"Missing texture at path: {texPath}");

            float linearMult = hdrExposureStops > 0.001f ? Mathf.Pow(2f, hdrExposureStops) : 1f;
            Color hdrWhite = new Color(linearMult, linearMult, linearMult, 1f);

            mat.SetTexture("_BaseMap", tex);
            mat.SetColor("_BaseColor", hdrWhite);

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
            if (softParticles) mat.EnableKeyword("_SOFTPARTICLES_ON");
            else mat.DisableKeyword("_SOFTPARTICLES_ON");

            InvokeURPValidateMaterial(mat);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void InvokeURPValidateMaterial(Material mat)
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
                Debug.LogWarning($"[BuildActivityVFX] URP ValidateMaterial fallback note: {ex.Message}");
            }
        }

        // =========================================================================
        // VFX 1: EXTRAVAGANT CELESTIAL MAGICAL GIRL TRANSFORMATION + SKY STARFALL
        // =========================================================================
        private static GameObject BuildMagicalGirlPrefab(
            Material matCore,
            Material matPillar,
            Material matSigil,
            Material matRing,
            Material matRibbon,
            Material matSparkle,
            Material matStar5Pt,
            Material matCometHead,
            Material matCometTail,
            Material matSubStar,
            Material matShockwave,
            Material matFlare)
        {
            GameObject root = CreateMasterController("VFX_MagicalGirl_Henshin", 3.4f, false, false);

            // 1. PS_CoreAuraGlow (Massive luminous aura wrapping hero, fades quickly so character reads cleanly)
            var psCore = CreateChildPS(root, "PS_CoreAuraGlow", new Vector3(0f, 1.8f, 0f));
            {
                var main = psCore.main;
                main.duration = 1.2f;
                main.loop = false;
                main.startLifetime = 1.0f;
                main.startSpeed = 0f;
                main.startSize = 7.0f;
                main.startColor = Color.white;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.maxParticles = 4;

                var em = psCore.emission;
                em.rateOverTime = 0f;
                em.SetBursts(new[]
                {
                    new ParticleSystem.Burst(0.00f, 1),
                    new ParticleSystem.Burst(0.10f, 1)
                });

                var shape = psCore.shape;
                shape.enabled = false;

                var col = psCore.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#FF148A")), (0.50f, Hex("#B829FF")), (1.00f, Hex("#00E5FF")) },
                    new[] { (0.00f, 0.00f), (0.14f, 0.65f), (0.55f, 0.30f), (1.00f, 0.00f) });

                var sol = psCore.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.35f), (0.28f, 1.05f), (1.00f, 1.45f));

                var rend = psCore.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Billboard;
                rend.sharedMaterial = matCore;
                rend.sortingFudge = 2.5f;
            }

            // 2. PS_CelestialFloorSigil (Colossal 10.5m rotating celestial pentagram circle on the arena floor)
            var psSigil = CreateChildPS(root, "PS_CelestialFloorSigil", new Vector3(0f, 0.08f, 0f));
            {
                var main = psSigil.main;
                main.duration = 2.6f;
                main.loop = false;
                main.startLifetime = 2.6f;
                main.startSpeed = 0f;
                main.startSize = 10.5f;
                main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.maxParticles = 3;

                var em = psSigil.emission;
                em.rateOverTime = 0f;
                em.SetBursts(new[] { new ParticleSystem.Burst(0.00f, 1) });
                var shape = psSigil.shape;
                shape.enabled = false;

                var col = psSigil.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#FF148A")), (0.45f, Hex("#B429FF")), (0.80f, Hex("#00EAFF")), (1.00f, Hex("#FFE047")) },
                    new[] { (0.00f, 0.00f), (0.10f, 0.90f), (0.78f, 0.72f), (1.00f, 0.00f) });

                var sol = psSigil.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.25f), (0.14f, 1.00f), (0.85f, 1.08f), (1.00f, 1.25f));

                var rol = psSigil.rotationOverLifetime;
                rol.enabled = true;
                rol.z = 38f * Mathf.Deg2Rad;

                var rend = psSigil.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
                rend.sharedMaterial = matSigil;
                rend.sortingFudge = 1.5f;
            }

            // 3. PS_CelestialLightPillar (Towering sky-to-earth transformation beam)
            var psPillar = CreateChildPS(root, "PS_CelestialLightPillar", new Vector3(0f, 6.2f, 0f));
            {
                var main = psPillar.main;
                main.duration = 1.1f;
                main.loop = false;
                main.startDelay = 0.04f;
                main.startLifetime = 0.85f;
                main.startSpeed = 0f;
                main.startSize3D = true;
                main.startSizeX = 4.5f;
                main.startSizeY = 14.5f;
                main.startSizeZ = 1.0f;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.maxParticles = 3;

                var em = psPillar.emission;
                em.rateOverTime = 0f;
                em.SetBursts(new[] { new ParticleSystem.Burst(0.0f, 2) });
                var shape = psPillar.shape;
                shape.enabled = false;

                var col = psPillar.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#FFE047")), (0.35f, Hex("#FF148A")), (0.75f, Hex("#00EAFF")), (1.00f, Hex("#00EAFF")) },
                    new[] { (0.00f, 0.00f), (0.14f, 0.65f), (0.55f, 0.35f), (1.00f, 0.00f) });

                var sol = psPillar.sizeOverLifetime;
                sol.enabled = true;
                sol.separateAxes = true;
                sol.x = MakeCurve((0.00f, 0.25f), (0.20f, 1.00f), (1.00f, 1.55f));
                sol.y = MakeCurve((0.00f, 1.00f), (1.00f, 1.05f));
                sol.z = MakeCurve((0.00f, 1.00f), (1.00f, 1.00f));

                var rend = psPillar.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.VerticalBillboard;
                rend.sharedMaterial = matPillar;
                rend.sortingFudge = 2.0f;
            }

            // 4. PS_EnergyRings (Colossal 6.5–10m tilted & horizontal jewel-toned rings)
            var psRings = CreateChildPS(root, "PS_EnergyRings", new Vector3(0f, 2.0f, 0f));
            {
                var main = psRings.main;
                main.duration = 1.4f;
                main.loop = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.75f, 1.05f);
                main.startSpeed = 0f;
                main.startSize = new ParticleSystem.MinMaxCurve(6.2f, 9.8f);
                main.startRotation3D = true;
                main.startRotationX = new ParticleSystem.MinMaxCurve(58f * Mathf.Deg2Rad, 122f * Mathf.Deg2Rad);
                main.startRotationY = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.startRotationZ = new ParticleSystem.MinMaxCurve(-35f * Mathf.Deg2Rad, 35f * Mathf.Deg2Rad);
                main.startColor = MakeRandomPalette(Hex("#FF148A"), Hex("#00EAFF"), Hex("#FFD83D"));
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.maxParticles = 18;

                var em = psRings.emission;
                em.rateOverTime = 0f;
                em.SetBursts(new[]
                {
                    new ParticleSystem.Burst(0.04f, 3),
                    new ParticleSystem.Burst(0.18f, 3),
                    new ParticleSystem.Burst(0.36f, 3),
                    new ParticleSystem.Burst(0.58f, 2)
                });

                var shape = psRings.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = new Vector3(0.15f, 2.6f, 0.15f);

                var vel = psRings.velocityOverLifetime;
                vel.enabled = true;
                vel.space = ParticleSystemSimulationSpace.Local;
                vel.x = 0f;
                vel.y = 1.4f;
                vel.z = 0f;

                var col = psRings.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Color.white), (1.00f, Color.white) },
                    new[] { (0.00f, 0.00f), (0.12f, 1.00f), (0.72f, 0.85f), (1.00f, 0.00f) });

                var sol = psRings.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.14f), (0.28f, 0.88f), (1.00f, 1.32f));

                var rol = psRings.rotationOverLifetime;
                rol.enabled = true;
                rol.separateAxes = true;
                rol.x = new ParticleSystem.MinMaxCurve(0f, 0f);
                rol.y = new ParticleSystem.MinMaxCurve(0f, 0f);
                rol.z = new ParticleSystem.MinMaxCurve(-170f * Mathf.Deg2Rad, 170f * Mathf.Deg2Rad);

                var rend = psRings.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Mesh;
                rend.mesh = GetBuiltinQuadMesh();
                rend.alignment = ParticleSystemRenderSpace.Local;
                rend.sharedMaterial = matRing;
                rend.sortingFudge = 0.0f;
            }

            // 5. PS_LuminousRibbons (Wide sweeping ribbons spiraling 7m up around character)
            var psRibbons = CreateChildPS(root, "PS_LuminousRibbons", new Vector3(0f, 0.25f, 0f));
            {
                var main = psRibbons.main;
                main.duration = 1.15f;
                main.loop = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.95f, 1.35f);
                main.startSpeed = 0f;
                main.startSize = new ParticleSystem.MinMaxCurve(4.6f, 6.8f);
                main.startRotation3D = true;
                main.startRotationX = new ParticleSystem.MinMaxCurve(90f * Mathf.Deg2Rad, 90f * Mathf.Deg2Rad);
                main.startRotationY = new ParticleSystem.MinMaxCurve(0f, 0f);
                main.startRotationZ = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.maxParticles = 24;

                var em = psRibbons.emission;
                em.rateOverTime = 20f;

                var shape = psRibbons.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.25f;
                shape.rotation = new Vector3(-90f, 0f, 0f);

                var vel = psRibbons.velocityOverLifetime;
                vel.enabled = true;
                vel.space = ParticleSystemSimulationSpace.Local;
                vel.x = 0f;
                vel.y = 5.4f;
                vel.z = 0f;

                var col = psRibbons.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#FF148A")), (0.48f, Hex("#A82BFF")), (1.00f, Hex("#00EAFF")) },
                    new[] { (0.00f, 0.00f), (0.18f, 0.90f), (0.74f, 0.65f), (1.00f, 0.00f) });

                var sol = psRibbons.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.55f), (0.42f, 1.08f), (1.00f, 0.35f));

                var rol = psRibbons.rotationOverLifetime;
                rol.enabled = true;
                rol.separateAxes = true;
                rol.x = 0f;
                rol.y = 0f;
                rol.z = 330f * Mathf.Deg2Rad;

                var rend = psRibbons.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Mesh;
                rend.mesh = GetBuiltinQuadMesh();
                rend.alignment = ParticleSystemRenderSpace.Local;
                rend.sharedMaterial = matRibbon;
                rend.sortingFudge = -0.5f;
            }

            // 6. PS_StarlightSpiralMotes (Upward spiraling anime sparkles around the body)
            var psSparkles = CreateChildPS(root, "PS_StarlightSpiralMotes", new Vector3(0f, 0.25f, 0f));
            var psSubGlitter = CreateChildPS(psSparkles.gameObject, "PS_Sub_GlitterPop", Vector3.zero);
            ConfigureGlitterSubEmitter(psSubGlitter, matSubStar, 0.35f, 0.65f);
            {
                var main = psSparkles.main;
                main.duration = 1.6f;
                main.loop = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(1.0f, 1.65f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(2.2f, 4.5f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.45f, 1.15f);
                main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.startColor = MakeRandomPalette(Hex("#FF148A"), Hex("#00EAFF"), Hex("#FFE047"));
                main.gravityModifier = -0.22f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 140;

                var em = psSparkles.emission;
                em.rateOverTime = 65f;
                em.SetBursts(new[]
                {
                    new ParticleSystem.Burst(0.00f, 24),
                    new ParticleSystem.Burst(0.12f, 32)
                });

                var shape = psSparkles.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 16f;
                shape.radius = 1.9f;
                shape.radiusThickness = 0.55f;
                shape.rotation = new Vector3(-90f, 0f, 0f);

                var vel = psSparkles.velocityOverLifetime;
                vel.enabled = true;
                vel.space = ParticleSystemSimulationSpace.Local;
                vel.x = 0f;
                vel.y = 4.2f;
                vel.z = 0f;
                vel.orbitalX = 0f;
                vel.orbitalY = 4.2f;
                vel.orbitalZ = 0f;
                vel.radial = 1.1f;

                var col = psSparkles.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Color.white), (1.00f, Color.white) },
                    new[] { (0.00f, 0.00f), (0.12f, 1.00f), (0.80f, 0.92f), (1.00f, 0.00f) });

                var sol = psSparkles.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.00f), (0.14f, 1.18f), (0.30f, 0.92f), (1.00f, 0.00f));

                var rol = psSparkles.rotationOverLifetime;
                rol.enabled = true;
                rol.z = new ParticleSystem.MinMaxCurve(-180f * Mathf.Deg2Rad, 180f * Mathf.Deg2Rad);

                var sub = psSparkles.subEmitters;
                sub.enabled = true;
                sub.AddSubEmitter(psSubGlitter, ParticleSystemSubEmitterType.Death, ParticleSystemSubEmitterProperties.InheritColor, 0.40f);

                var rend = psSparkles.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Billboard;
                rend.sharedMaterial = matSparkle;
                rend.sortingFudge = -1.5f;
            }

            // 7. PS_SkyStarfallShower (EXTRAVAGANT 5-point stars raining down from the sky around the character!)
            var psSkyStars = CreateChildPS(root, "PS_SkyStarfallShower", new Vector3(0f, 10.5f, 0f));
            var psSubStarImpact = CreateChildPS(psSkyStars.gameObject, "PS_Sub_StarBounceSparkle", Vector3.zero);
            ConfigureGlitterSubEmitter(psSubStarImpact, matSubStar, 0.45f, 0.90f);
            {
                var main = psSkyStars.main;
                main.duration = 2.0f;
                main.loop = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(1.45f, 2.20f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(4.5f, 8.5f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.48f, 1.12f);
                main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.startColor = MakeRandomPalette(Hex("#FFE047"), Hex("#FF148A"), Hex("#00EAFF"), Hex("#FFF6B3"));
                main.gravityModifier = 0.55f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 220;

                var em = psSkyStars.emission;
                em.rateOverTime = 85f;
                em.SetBursts(new[]
                {
                    new ParticleSystem.Burst(0.00f, 45),
                    new ParticleSystem.Burst(0.25f, 35),
                    new ParticleSystem.Burst(0.65f, 30)
                });

                var shape = psSkyStars.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 5f;
                shape.radius = 6.8f;
                shape.radiusThickness = 0.90f;
                shape.rotation = new Vector3(90f, 0f, 0f); // Emit straight down from sky canopy!

                var vel = psSkyStars.velocityOverLifetime;
                vel.enabled = true;
                vel.space = ParticleSystemSimulationSpace.Local;
                vel.x = 0f;
                vel.y = 0f;
                vel.z = 0f;
                vel.orbitalX = 0f;
                vel.orbitalY = 0.85f; // Gentle magical swirling descent
                vel.orbitalZ = 0f;

                var noise = psSkyStars.noise;
                noise.enabled = true;
                noise.strength = 1.15f;
                noise.frequency = 0.7f;
                noise.scrollSpeed = 0.8f;

                var col = psSkyStars.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Color.white), (1.00f, Color.white) },
                    new[] { (0.00f, 0.00f), (0.10f, 1.00f), (0.82f, 0.95f), (1.00f, 0.00f) });

                var sol = psSkyStars.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.20f), (0.12f, 1.15f), (0.82f, 0.90f), (1.00f, 0.00f));

                var rol = psSkyStars.rotationOverLifetime;
                rol.enabled = true;
                rol.z = new ParticleSystem.MinMaxCurve(-240f * Mathf.Deg2Rad, 240f * Mathf.Deg2Rad);

                var coll = psSkyStars.collision;
                coll.enabled = true;
                coll.type = ParticleSystemCollisionType.World;
                coll.mode = ParticleSystemCollisionMode.Collision3D;
                coll.bounce = 0.42f;
                coll.dampen = 0.25f;
                coll.lifetimeLoss = 0.35f;
                coll.radiusScale = 0.5f;

                var sub = psSkyStars.subEmitters;
                sub.enabled = true;
                sub.AddSubEmitter(psSubStarImpact, ParticleSystemSubEmitterType.Collision, ParticleSystemSubEmitterProperties.InheritColor, 0.75f);

                var rend = psSkyStars.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Billboard;
                rend.sharedMaterial = matStar5Pt;
                rend.sortingFudge = -2.0f;
            }

            // 8. PS_FallingShootingStars (Extravagant shooting-star comets streaking down from sky with Shuriken Trails!)
            var psComets = CreateChildPS(root, "PS_FallingShootingStars", new Vector3(1.8f, 12.2f, 0f));
            {
                var main = psComets.main;
                main.duration = 1.9f;
                main.loop = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.85f, 1.35f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(12.0f, 18.0f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.85f, 1.55f);
                main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.startColor = MakeRandomPalette(Hex("#FFE047"), Hex("#00EAFF"), Hex("#FF148A"));
                main.gravityModifier = 0.35f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 45;

                var em = psComets.emission;
                em.rateOverTime = 20f;
                em.SetBursts(new[]
                {
                    new ParticleSystem.Burst(0.02f, 12),
                    new ParticleSystem.Burst(0.35f, 10),
                    new ParticleSystem.Burst(0.80f, 8)
                });

                var shape = psComets.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = new Vector3(13.0f, 1.5f, 10.0f);
                shape.rotation = new Vector3(68f, -24f, 0f); // Sweeping diagonal meteor shower streaks!

                var col = psComets.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Color.white), (1.00f, Color.white) },
                    new[] { (0.00f, 0.00f), (0.10f, 1.00f), (0.85f, 0.90f), (1.00f, 0.00f) });

                var sol = psComets.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.35f), (0.15f, 1.15f), (0.85f, 0.85f), (1.00f, 0.00f));

                var rol = psComets.rotationOverLifetime;
                rol.enabled = true;
                rol.z = new ParticleSystem.MinMaxCurve(-300f * Mathf.Deg2Rad, 300f * Mathf.Deg2Rad);

                var trails = psComets.trails;
                trails.enabled = true;
                trails.mode = ParticleSystemTrailMode.PerParticle;
                trails.ratio = 1.0f;
                trails.lifetime = new ParticleSystem.MinMaxCurve(0.28f);
                trails.minVertexDistance = 0.15f;
                trails.worldSpace = true;
                trails.dieWithParticles = false;
                trails.sizeAffectsWidth = true;
                trails.inheritParticleColor = true;
                trails.widthOverTrail = MakeCurve((0.00f, 0.24f), (1.00f, 0.00f));
                trails.colorOverTrail = MakeGradient(
                    new[] { (0.00f, Color.white), (1.00f, Color.white) },
                    new[] { (0.00f, 0.95f), (1.00f, 0.00f) });

                var coll = psComets.collision;
                coll.enabled = true;
                coll.type = ParticleSystemCollisionType.World;
                coll.mode = ParticleSystemCollisionMode.Collision3D;
                coll.bounce = 0.20f;
                coll.lifetimeLoss = 0.85f;

                var rend = psComets.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Billboard;
                rend.sharedMaterial = matCometHead;
                rend.trailMaterial = matCometTail;
                rend.sortingFudge = -2.2f;
            }

            // 9. PS_OuterFlashShockwave + PS_AnamorphicSnapFlare (Massive 10.5m starburst + 12m lens flare)
            var psShock = CreateChildPS(root, "PS_OuterFlashShockwave", new Vector3(0f, 2.4f, 0f));
            {
                var main = psShock.main;
                main.duration = 0.60f;
                main.loop = false;
                main.startDelay = 0.08f;
                main.startLifetime = 0.42f;
                main.startSpeed = 0f;
                main.startSize = 10.5f;
                main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.maxParticles = 2;

                var em = psShock.emission;
                em.rateOverTime = 0f;
                em.SetBursts(new[] { new ParticleSystem.Burst(0.0f, 1) });
                var shape = psShock.shape;
                shape.enabled = false;

                var col = psShock.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#FFE047")), (0.30f, Hex("#FF148A")), (0.75f, Hex("#B429FF")), (1.00f, Hex("#00EAFF")) },
                    new[] { (0.00f, 0.95f), (0.30f, 0.85f), (0.75f, 0.35f), (1.00f, 0.00f) });

                var sol = psShock.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.14f), (0.25f, 0.94f), (1.00f, 1.32f));

                var rol = psShock.rotationOverLifetime;
                rol.enabled = true;
                rol.z = 85f * Mathf.Deg2Rad;

                var rend = psShock.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Billboard;
                rend.sharedMaterial = matShockwave;
                rend.sortingFudge = -2.5f;
            }

            var psFlare = CreateChildPS(root, "PS_AnamorphicSnapFlare", new Vector3(0f, 2.2f, 0f));
            {
                var main = psFlare.main;
                main.duration = 0.45f;
                main.loop = false;
                main.startDelay = 0.08f;
                main.startLifetime = 0.30f;
                main.startSpeed = 0f;
                main.startSize3D = true;
                main.startSizeX = 12.0f;
                main.startSizeY = 2.8f;
                main.startSizeZ = 1.0f;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.maxParticles = 2;

                var em = psFlare.emission;
                em.rateOverTime = 0f;
                em.SetBursts(new[] { new ParticleSystem.Burst(0.0f, 1) });
                var shape = psFlare.shape;
                shape.enabled = false;

                var col = psFlare.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#FFE66D")), (0.40f, Hex("#FF148A")), (1.00f, Hex("#00EAFF")) },
                    new[] { (0.00f, 1.00f), (0.40f, 0.88f), (1.00f, 0.00f) });

                var sol = psFlare.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.20f), (0.20f, 1.00f), (1.00f, 0.00f));

                var rend = psFlare.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Billboard;
                rend.sharedMaterial = matFlare;
                rend.sortingFudge = -3.0f;
            }

            return SaveAndDestroyPrefab(root, PrefabDir + "VFX_MagicalGirl_Henshin.prefab");
        }

        private static void ConfigureGlitterSubEmitter(ParticleSystem subPs, Material mat, float minSize, float maxSize)
        {
            var sMain = subPs.main;
            sMain.duration = 0.40f;
            sMain.loop = false;
            sMain.playOnAwake = false;
            sMain.startLifetime = new ParticleSystem.MinMaxCurve(0.28f, 0.45f);
            sMain.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.2f);
            sMain.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
            sMain.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            sMain.simulationSpace = ParticleSystemSimulationSpace.World;
            sMain.maxParticles = 140;

            var sEm = subPs.emission;
            sEm.rateOverTime = 0f;
            sEm.SetBursts(new[] { new ParticleSystem.Burst(0.0f, 3) });

            var sShape = subPs.shape;
            sShape.enabled = true;
            sShape.shapeType = ParticleSystemShapeType.Hemisphere;
            sShape.radius = 0.22f;
            sShape.rotation = new Vector3(-90f, 0f, 0f);

            var sCol = subPs.colorOverLifetime;
            sCol.enabled = true;
            sCol.color = MakeGradient(
                new[] { (0.00f, Color.white), (1.00f, Color.white) },
                new[] { (0.00f, 1.00f), (1.00f, 0.00f) });

            var sSol = subPs.sizeOverLifetime;
            sSol.enabled = true;
            sSol.size = MakeCurve((0.00f, 0.20f), (0.25f, 1.00f), (1.00f, 0.00f));

            var sRend = subPs.GetComponent<ParticleSystemRenderer>();
            sRend.renderMode = ParticleSystemRenderMode.Billboard;
            sRend.sharedMaterial = mat;
            sRend.sortingFudge = -2.5f;
        }

        // =========================================================================
        // VFX 2: MASSIVE AVATAR-STYLE LIGHTNING BENDING STRIKE
        // =========================================================================
        private static GameObject BuildLightningBendingPrefab(
            Material matCharge,
            Material matCoreGlow,
            Material matBoltBeam,
            Material matWebArc,
            Material matElecRing,
            Material matSpear,
            Material matSparks,
            Material matZapSub,
            Material matOzoneMist)
        {
            GameObject root = CreateMasterController("VFX_ElementalBending_LightningStrike", 2.0f, false, false);

            // 1. PS_FingertipChargeOrb (Spinning electric yin-yang charge vortex right in front of fingertips)
            var psCharge = CreateChildPS(root, "PS_FingertipChargeOrb", new Vector3(0f, 0f, 0.55f));
            {
                var main = psCharge.main;
                main.duration = 0.55f;
                main.loop = false;
                main.startLifetime = 0.42f;
                main.startSpeed = 0f;
                main.startSize = 3.8f;
                main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.maxParticles = 6;

                var em = psCharge.emission;
                em.rateOverTime = 0f;
                em.SetBursts(new[]
                {
                    new ParticleSystem.Burst(0.00f, 2),
                    new ParticleSystem.Burst(0.10f, 2)
                });

                var shape = psCharge.shape;
                shape.enabled = false;

                var col = psCharge.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Color.white), (0.32f, Hex("#00F0FF")), (0.75f, Hex("#1A6BFF")), (1.00f, Hex("#0A24A8")) },
                    new[] { (0.00f, 1.00f), (0.35f, 0.90f), (1.00f, 0.00f) });

                var sol = psCharge.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.25f), (0.30f, 1.05f), (1.00f, 1.35f));

                var rol = psCharge.rotationOverLifetime;
                rol.enabled = true;
                rol.z = 540f * Mathf.Deg2Rad;

                var rend = psCharge.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Mesh;
                rend.mesh = GetBuiltinQuadMesh();
                rend.alignment = ParticleSystemRenderSpace.Local;
                rend.sharedMaterial = matCharge;
                rend.sortingFudge = -1.5f;
            }

            // Core plasma glow orb at fingertips
            var psCoreOrb = CreateChildPS(root, "PS_PlasmaMuzzleGlow", new Vector3(0f, 0f, 0.55f));
            {
                var main = psCoreOrb.main;
                main.duration = 0.65f;
                main.loop = false;
                main.startLifetime = 0.55f;
                main.startSpeed = 0f;
                main.startSize = 4.2f;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.maxParticles = 3;

                var em = psCoreOrb.emission;
                em.rateOverTime = 0f;
                em.SetBursts(new[] { new ParticleSystem.Burst(0.00f, 1), new ParticleSystem.Burst(0.10f, 1) });
                var shape = psCoreOrb.shape;
                shape.enabled = false;

                var col = psCoreOrb.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#E6FFFF")), (0.35f, Hex("#00EAFF")), (1.00f, Hex("#145CFF")) },
                    new[] { (0.00f, 0.85f), (0.30f, 0.65f), (1.00f, 0.00f) });

                var sol = psCoreOrb.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.30f), (0.20f, 1.05f), (1.00f, 0.45f));

                var rend = psCoreOrb.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Billboard;
                rend.sharedMaterial = matCoreGlow;
                rend.sortingFudge = 0.5f;
            }

            // 2. PS_LightningBoltStream (Crisp individual jagged lightning forks shooting 15m strictly ahead of fingertips!)
            var psBeam = CreateChildPS(root, "PS_LightningBoltStream", new Vector3(0f, 0f, 2.4f));
            {
                var main = psBeam.main;
                main.duration = 0.65f;
                main.loop = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.28f, 0.46f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(26.0f, 38.0f);
                main.startSize = new ParticleSystem.MinMaxCurve(1.4f, 2.5f);
                main.flipRotation = 0.5f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 45;

                var em = psBeam.emission;
                em.rateOverTime = 30f;
                em.SetBursts(new[]
                {
                    new ParticleSystem.Burst(0.02f, 5),
                    new ParticleSystem.Burst(0.16f, 6),
                    new ParticleSystem.Burst(0.32f, 5)
                });

                var shape = psBeam.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 6f;
                shape.radius = 0.30f;
                shape.rotation = Vector3.zero;

                var noise = psBeam.noise;
                noise.enabled = true;
                noise.strength = 3.8f;
                noise.frequency = 2.6f;
                noise.scrollSpeed = 4.5f;

                var col = psBeam.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#FFFFFF")), (0.25f, Hex("#00EAFF")), (0.68f, Hex("#1464FF")), (1.00f, Hex("#0B26A8")) },
                    new[] { (0.00f, 1.00f), (0.65f, 0.92f), (1.00f, 0.00f) });

                var sol = psBeam.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.65f), (0.25f, 1.15f), (1.00f, 0.40f));

                var rend = psBeam.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Stretch;
                rend.cameraVelocityScale = 0f;
                rend.velocityScale = 0.06f;
                rend.lengthScale = 1.6f;
                rend.sharedMaterial = matBoltBeam;
                rend.sortingFudge = -2.5f;
            }

            // 3. PS_BranchingPlasmaArcs (Jagged branching electric webs cracking along the 14m discharge!)
            var psArcs = CreateChildPS(root, "PS_BranchingPlasmaArcs", new Vector3(0f, 0f, 2.0f));
            {
                var main = psArcs.main;
                main.duration = 0.70f;
                main.loop = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.24f, 0.42f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(22.0f, 34.0f);
                main.startSize = new ParticleSystem.MinMaxCurve(2.8f, 4.8f);
                main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.flipRotation = 0.5f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 32;

                var em = psArcs.emission;
                em.rateOverTime = 24f;
                em.SetBursts(new[]
                {
                    new ParticleSystem.Burst(0.03f, 4),
                    new ParticleSystem.Burst(0.18f, 5),
                    new ParticleSystem.Burst(0.35f, 4)
                });

                var shape = psArcs.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 9.5f;
                shape.radius = 0.45f;
                shape.rotation = Vector3.zero;

                var noise = psArcs.noise;
                noise.enabled = true;
                noise.strength = 3.6f;
                noise.frequency = 1.6f;
                noise.scrollSpeed = 3.5f;

                var col = psArcs.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#D8FFFF")), (0.30f, Hex("#00D4FF")), (0.72f, Hex("#145CFF")), (1.00f, Hex("#0B2088")) },
                    new[] { (0.00f, 0.92f), (0.55f, 0.80f), (1.00f, 0.00f) });

                var sol = psArcs.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.55f), (0.28f, 1.12f), (1.00f, 0.85f));

                var rend = psArcs.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Billboard;
                rend.sharedMaterial = matWebArc;
                rend.sortingFudge = -2.0f;
            }

            // 4. PS_ElectricCompressionRings (Cone Angle 0 -> fires DEAD STRAIGHT down +Z around the lightning beam!)
            var psRings = CreateChildPS(root, "PS_ElectricCompressionRings", new Vector3(0f, 0f, 0.9f));
            {
                var main = psRings.main;
                main.duration = 0.60f;
                main.loop = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.58f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(20.0f, 28.0f);
                main.startSize = new ParticleSystem.MinMaxCurve(3.6f, 5.8f);
                main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 14;

                var em = psRings.emission;
                em.rateOverTime = 0f;
                em.SetBursts(new[]
                {
                    new ParticleSystem.Burst(0.02f, 1),
                    new ParticleSystem.Burst(0.11f, 1),
                    new ParticleSystem.Burst(0.20f, 1),
                    new ParticleSystem.Burst(0.30f, 1),
                    new ParticleSystem.Burst(0.42f, 1)
                });

                var shape = psRings.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 0f;
                shape.radius = 0.05f;
                shape.rotation = Vector3.zero;

                var col = psRings.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#E6FFFF")), (0.45f, Hex("#00EAFF")), (1.00f, Hex("#1A6BFF")) },
                    new[] { (0.00f, 0.00f), (0.12f, 0.92f), (0.72f, 0.70f), (1.00f, 0.00f) });

                var sol = psRings.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.25f), (0.35f, 0.95f), (1.00f, 1.38f));

                var rend = psRings.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Mesh;
                rend.mesh = GetBuiltinQuadMesh();
                rend.alignment = ParticleSystemRenderSpace.Local;
                rend.sharedMaterial = matElecRing;
                rend.sortingFudge = -1.2f;
            }

            // 5. PS_PlasmaSpearStreaks (Razor-sharp helical lightning lances starting cleanly ahead of fingertips!)
            var psSpears = CreateChildPS(root, "PS_PlasmaSpearStreaks", new Vector3(0f, 0f, 2.2f));
            {
                var main = psSpears.main;
                main.duration = 0.65f;
                main.loop = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.28f, 0.46f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(28.0f, 42.0f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.45f, 0.90f);
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 60;

                var em = psSpears.emission;
                em.rateOverTime = 52f;
                em.SetBursts(new[] { new ParticleSystem.Burst(0.02f, 10) });

                var shape = psSpears.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 7.5f;
                shape.radius = 0.35f;
                shape.rotation = Vector3.zero;

                var vel = psSpears.velocityOverLifetime;
                vel.enabled = true;
                vel.space = ParticleSystemSimulationSpace.Local;
                vel.x = 0f;
                vel.y = 0f;
                vel.z = 0f;
                vel.orbitalX = 0f;
                vel.orbitalY = 0f;
                vel.orbitalZ = 3.8f;

                var col = psSpears.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Color.white), (0.35f, Hex("#00F0FF")), (0.80f, Hex("#1F6BFF")), (1.00f, Hex("#0C2080")) },
                    new[] { (0.00f, 0.00f), (0.12f, 1.00f), (0.70f, 0.85f), (1.00f, 0.00f) });

                var sol = psSpears.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.40f), (0.25f, 1.05f), (1.00f, 0.10f));

                var rend = psSpears.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Stretch;
                rend.cameraVelocityScale = 0f;
                rend.velocityScale = 0.06f;
                rend.lengthScale = 2.0f;
                rend.sharedMaterial = matSpear;
                rend.sortingFudge = -2.8f;
            }

            // 6. PS_GroundRicochetSparks + Sub-Emitter PS_Sub_GroundZap (Gravity + Floor Bounce + Electric Impact Flash!)
            var psSparks = CreateChildPS(root, "PS_GroundRicochetSparks", new Vector3(0f, -0.15f, 0.8f));
            var psSubZap = CreateChildPS(psSparks.gameObject, "PS_Sub_GroundZap", Vector3.zero);
            {
                var sMain = psSubZap.main;
                sMain.duration = 0.35f;
                sMain.loop = false;
                sMain.playOnAwake = false;
                sMain.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.30f);
                sMain.startSpeed = 0f;
                sMain.startSize = new ParticleSystem.MinMaxCurve(1.4f, 2.6f);
                sMain.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                sMain.simulationSpace = ParticleSystemSimulationSpace.World;
                sMain.maxParticles = 60;

                var sEm = psSubZap.emission;
                sEm.rateOverTime = 0f;
                sEm.SetBursts(new[] { new ParticleSystem.Burst(0.0f, 1) });
                var sShape = psSubZap.shape;
                sShape.enabled = false;

                var sCol = psSubZap.colorOverLifetime;
                sCol.enabled = true;
                sCol.color = MakeGradient(
                    new[] { (0.00f, Color.white), (0.35f, Hex("#00EAFF")), (1.00f, Hex("#1A5CFF")) },
                    new[] { (0.00f, 1.00f), (0.50f, 0.85f), (1.00f, 0.00f) });

                var sSol = psSubZap.sizeOverLifetime;
                sSol.enabled = true;
                sSol.size = MakeCurve((0.00f, 0.35f), (0.25f, 1.05f), (1.00f, 1.35f));

                var sRend = psSubZap.GetComponent<ParticleSystemRenderer>();
                sRend.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
                sRend.sharedMaterial = matZapSub;
                sRend.sortingFudge = -1.5f;
            }
            {
                var main = psSparks.main;
                main.duration = 0.65f;
                main.loop = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.75f, 1.35f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(14.0f, 28.0f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.32f, 0.72f);
                main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.startColor = MakeRandomPalette(Color.white, Hex("#00F0FF"), Hex("#4DA6FF"));
                main.gravityModifier = 2.0f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 110;

                var em = psSparks.emission;
                em.rateOverTime = 65f;
                em.SetBursts(new[]
                {
                    new ParticleSystem.Burst(0.03f, 32),
                    new ParticleSystem.Burst(0.18f, 24)
                });

                var shape = psSparks.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 22f;
                shape.radius = 0.55f;
                shape.rotation = new Vector3(-6f, 0f, 0f);

                var col = psSparks.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Color.white), (0.60f, Hex("#00EAFF")), (1.00f, Hex("#145CFF")) },
                    new[] { (0.00f, 1.00f), (0.75f, 0.90f), (1.00f, 0.00f) });

                var sol = psSparks.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.60f), (0.15f, 1.00f), (1.00f, 0.00f));

                var coll = psSparks.collision;
                coll.enabled = true;
                coll.type = ParticleSystemCollisionType.World;
                coll.mode = ParticleSystemCollisionMode.Collision3D;
                coll.bounce = 0.52f;
                coll.dampen = 0.30f;
                coll.lifetimeLoss = 0.20f;
                coll.radiusScale = 0.5f;

                var sub = psSparks.subEmitters;
                sub.enabled = true;
                sub.AddSubEmitter(psSubZap, ParticleSystemSubEmitterType.Collision, ParticleSystemSubEmitterProperties.InheritNothing, 0.55f);

                var rend = psSparks.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Stretch;
                rend.cameraVelocityScale = 0f;
                rend.velocityScale = 0.08f;
                rend.lengthScale = 2.0f;
                rend.sharedMaterial = matSparks;
                rend.sortingFudge = -2.2f;
            }

            // 7. PS_IonizedOzoneMist (Huge swirling electric-blue vapor clouds trailing behind the lightning surge)
            var psMist = CreateChildPS(root, "PS_IonizedOzoneMist", new Vector3(0f, 0f, 0.8f));
            {
                var main = psMist.main;
                main.duration = 0.70f;
                main.loop = false;
                main.startDelay = 0.04f;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.85f, 1.45f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(14.0f, 24.0f);
                main.startSize = new ParticleSystem.MinMaxCurve(2.6f, 4.8f);
                main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 55;

                var em = psMist.emission;
                em.rateOverTime = 60f;
                em.SetBursts(new[] { new ParticleSystem.Burst(0.05f, 14) });

                var shape = psMist.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 20f;
                shape.radius = 0.65f;
                shape.rotation = Vector3.zero;

                var vel = psMist.velocityOverLifetime;
                vel.enabled = true;
                vel.space = ParticleSystemSimulationSpace.Local;
                vel.x = 0f;
                vel.y = 0.85f;
                vel.z = 0f;
                vel.orbitalX = 0f;
                vel.orbitalY = 0f;
                vel.orbitalZ = 2.6f;

                var limitVel = psMist.limitVelocityOverLifetime;
                limitVel.enabled = true;
                limitVel.drag = 1.6f;
                limitVel.multiplyDragByParticleSize = false;
                limitVel.multiplyDragByParticleVelocity = false; // Linear drag so ozone mist travels 8-10m down the lightning wake!

                var noise = psMist.noise;
                noise.enabled = true;
                noise.strength = 1.1f;
                noise.frequency = 0.65f;
                noise.scrollSpeed = 0.65f;

                var col = psMist.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#99F3FF")), (0.40f, Hex("#2063B8")), (1.00f, Hex("#0B1B3B")) },
                    new[] { (0.00f, 0.00f), (0.18f, 0.56f), (0.65f, 0.36f), (1.00f, 0.00f) });

                var sol = psMist.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.45f), (1.00f, 1.85f));

                var rol = psMist.rotationOverLifetime;
                rol.enabled = true;
                rol.z = new ParticleSystem.MinMaxCurve(-80f * Mathf.Deg2Rad, 80f * Mathf.Deg2Rad);

                var tsa = psMist.textureSheetAnimation;
                tsa.enabled = true;
                tsa.mode = ParticleSystemAnimationMode.Grid;
                tsa.numTilesX = 8;
                tsa.numTilesY = 8;
                tsa.animation = ParticleSystemAnimationType.WholeSheet;
                tsa.timeMode = ParticleSystemAnimationTimeMode.Lifetime;
                tsa.frameOverTime = MakeCurve((0f, 0f), (1f, 1f));
                tsa.startFrame = new ParticleSystem.MinMaxCurve(0f, 16f / 64f);
                tsa.cycleCount = 1;

                var rend = psMist.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Billboard;
                rend.sharedMaterial = matOzoneMist;
                rend.sortingFudge = 2.0f;
            }

            return SaveAndDestroyPrefab(root, PrefabDir + "VFX_ElementalBending_LightningStrike.prefab");
        }

        // =========================================================================
        // VFX 3: COLOSSAL FINAL BOSS "AURAMAXXING"
        // =========================================================================
        private static GameObject BuildFinalBossAuramaxxingPrefab(
            Material matSmoke,
            Material matSpikes,
            Material matBolt,
            Material matVertBolt,
            Material matGeo,
            Material matRing,
            Material matSigil)
        {
            GameObject root = CreateMasterController("VFX_FinalBoss_Auramaxxing", 2.0f, true, true);

            // 1. PS_AbyssalDarkSmoke (Colossal 10m+ towering obsidian-violet dark storm)
            var psSmoke = CreateChildPS(root, "PS_AbyssalDarkSmoke", new Vector3(0f, 0.5f, 0f));
            {
                var main = psSmoke.main;
                main.duration = 2.0f;
                main.loop = true;
                main.prewarm = true;
                main.startLifetime = new ParticleSystem.MinMaxCurve(2.0f, 3.0f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(1.4f, 2.8f);
                main.startSize = new ParticleSystem.MinMaxCurve(3.8f, 6.4f);
                main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 85;

                var em = psSmoke.emission;
                em.rateOverTime = 30f;

                var shape = psSmoke.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 11f;
                shape.radius = 2.4f;
                shape.radiusThickness = 0.75f;
                shape.rotation = new Vector3(-90f, 0f, 0f);

                var vel = psSmoke.velocityOverLifetime;
                vel.enabled = true;
                vel.space = ParticleSystemSimulationSpace.Local;
                vel.x = 0f;
                vel.y = 3.5f;
                vel.z = 0f;

                var col = psSmoke.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#1A062B")), (0.45f, Hex("#4A0080")), (0.85f, Hex("#1F0533")), (1.00f, Hex("#0D0216")) },
                    new[] { (0.00f, 0.00f), (0.20f, 0.88f), (0.65f, 0.76f), (1.00f, 0.00f) });

                var sol = psSmoke.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.65f), (0.50f, 1.18f), (1.00f, 1.72f));

                var rol = psSmoke.rotationOverLifetime;
                rol.enabled = true;
                rol.z = new ParticleSystem.MinMaxCurve(-35f * Mathf.Deg2Rad, 35f * Mathf.Deg2Rad);

                var noise = psSmoke.noise;
                noise.enabled = true;
                noise.strength = 2.1f;
                noise.frequency = 0.45f;
                noise.scrollSpeed = 0.45f;
                noise.octaveCount = 2;
                noise.quality = ParticleSystemNoiseQuality.Medium;

                var tsa = psSmoke.textureSheetAnimation;
                tsa.enabled = true;
                tsa.mode = ParticleSystemAnimationMode.Grid;
                tsa.numTilesX = 8;
                tsa.numTilesY = 8;
                tsa.animation = ParticleSystemAnimationType.WholeSheet;
                tsa.timeMode = ParticleSystemAnimationTimeMode.Lifetime;
                tsa.frameOverTime = MakeCurve((0f, 0f), (1f, 1f));
                tsa.startFrame = new ParticleSystem.MinMaxCurve(0f, 20f / 64f);
                tsa.cycleCount = 1;

                var rend = psSmoke.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Billboard;
                rend.sharedMaterial = matSmoke;
                rend.sortingFudge = 2.5f;
            }

            // 2. PS_RisingEnergySpikes (Colossal 11m upward crimson-violet power-pressure streaks)
            var psSpikes = CreateChildPS(root, "PS_RisingEnergySpikes", new Vector3(0f, 0.25f, 0f));
            {
                var main = psSpikes.main;
                main.duration = 2.0f;
                main.loop = true;
                main.prewarm = true;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.82f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(10.0f, 18.0f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.42f, 0.88f);
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.maxParticles = 135;

                var em = psSpikes.emission;
                em.rateOverTime = 88f;

                var shape = psSpikes.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 6f;
                shape.radius = 3.1f;
                shape.radiusThickness = 0.50f;
                shape.rotation = new Vector3(-90f, 0f, 0f);

                var vel = psSpikes.velocityOverLifetime;
                vel.enabled = true;
                vel.space = ParticleSystemSimulationSpace.Local;
                vel.x = 0f;
                vel.y = 4.2f;
                vel.z = 0f;
                vel.radial = -0.85f;

                var noise = psSpikes.noise;
                noise.enabled = true;
                noise.strength = 0.95f;
                noise.frequency = 1.4f;
                noise.scrollSpeed = 1.4f;

                var col = psSpikes.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#FFD6E0")), (0.25f, Hex("#FF003C")), (0.65f, Hex("#B5179E")), (1.00f, Hex("#4A0080")) },
                    new[] { (0.00f, 0.00f), (0.15f, 1.00f), (0.72f, 0.76f), (1.00f, 0.00f) });

                var sol = psSpikes.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.30f), (0.20f, 1.00f), (1.00f, 0.00f));

                var rend = psSpikes.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Stretch;
                rend.cameraVelocityScale = 0f;
                rend.velocityScale = 0.16f;
                rend.lengthScale = 3.2f;
                rend.sharedMaterial = matSpikes;
                rend.sortingFudge = 0.0f;
            }

            // 3. PS_HighVoltageCrackles (Giant crimson-violet plasma crackles erupting around boss)
            var psBolts = CreateChildPS(root, "PS_HighVoltageCrackles", new Vector3(0f, 2.8f, 0f));
            {
                var main = psBolts.main;
                main.duration = 2.0f;
                main.loop = true;
                main.prewarm = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.26f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(1.0f, 3.2f);
                main.startSize = new ParticleSystem.MinMaxCurve(3.4f, 6.2f);
                main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.flipRotation = 0.5f;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.maxParticles = 32;

                var em = psBolts.emission;
                em.rateOverTime = 0f;
                var b0 = new ParticleSystem.Burst(0.00f, 3, 5, 1, 0.01f) { probability = 0.96f };
                var b1 = new ParticleSystem.Burst(0.54f, 3, 5, 1, 0.01f) { probability = 0.92f };
                var b2 = new ParticleSystem.Burst(1.08f, 2, 4, 1, 0.01f) { probability = 0.88f };
                var b3 = new ParticleSystem.Burst(1.58f, 3, 5, 1, 0.01f) { probability = 0.95f };
                em.SetBursts(new[] { b0, b1, b2, b3 });

                var shape = psBolts.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 2.8f;
                shape.radiusThickness = 0.65f;

                var col = psBolts.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#FFE6EE")), (0.30f, Hex("#FF003C")), (0.72f, Hex("#B5179E")), (1.00f, Hex("#4A0080")) },
                    new[] { (0.00f, 1.00f), (0.55f, 1.00f), (1.00f, 0.00f) });

                var sol = psBolts.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.72f), (0.20f, 1.18f), (1.00f, 0.96f));

                var rend = psBolts.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Billboard;
                rend.sharedMaterial = matBolt;
                rend.sortingFudge = -2.0f;
            }

            // 3b. PS_VerticalDreadBolts (Towering sky-to-floor crimson lightning pillars crashing around the boss)
            var psVertBolts = CreateChildPS(root, "PS_VerticalDreadBolts", new Vector3(0f, 5.2f, 0f));
            {
                var main = psVertBolts.main;
                main.duration = 2.0f;
                main.loop = true;
                main.prewarm = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.14f, 0.24f);
                main.startSpeed = 0f;
                main.startSize3D = true;
                main.startSizeX = new ParticleSystem.MinMaxCurve(2.2f, 3.6f);
                main.startSizeY = new ParticleSystem.MinMaxCurve(9.5f, 12.5f);
                main.startSizeZ = new ParticleSystem.MinMaxCurve(1.0f, 1.0f);
                main.flipRotation = 0.5f;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.maxParticles = 12;

                var em = psVertBolts.emission;
                em.rateOverTime = 0f;
                em.SetBursts(new[]
                {
                    new ParticleSystem.Burst(0.02f, 2),
                    new ParticleSystem.Burst(0.75f, 2),
                    new ParticleSystem.Burst(1.42f, 2)
                });

                var shape = psVertBolts.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 3.2f;
                shape.radiusThickness = 0.45f;
                shape.rotation = new Vector3(-90f, 0f, 0f);

                var col = psVertBolts.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#FFE0EC")), (0.35f, Hex("#FF0044")), (1.00f, Hex("#8E00B8")) },
                    new[] { (0.00f, 1.00f), (0.60f, 0.90f), (1.00f, 0.00f) });

                var rend = psVertBolts.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.VerticalBillboard;
                rend.sharedMaterial = matVertBolt;
                rend.sortingFudge = -2.2f;
            }

            // 4. PS_GeometricRuneAccents (Colossal 5.8–9.0m Kenney geometric decagon fractures)
            var psGeo = CreateChildPS(root, "PS_GeometricRuneAccents", new Vector3(0f, 2.7f, 0f));
            {
                var main = psGeo.main;
                main.duration = 2.0f;
                main.loop = true;
                main.prewarm = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.28f, 0.46f);
                main.startSpeed = 0f;
                main.startSize = new ParticleSystem.MinMaxCurve(5.6f, 8.8f);
                main.startRotation3D = true;
                main.startRotationX = new ParticleSystem.MinMaxCurve(0f, 180f * Mathf.Deg2Rad);
                main.startRotationY = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.startRotationZ = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.maxParticles = 12;

                var em = psGeo.emission;
                em.rateOverTime = 0f;
                var g0 = new ParticleSystem.Burst(0.02f, 2, 2, 1, 0.01f) { probability = 0.92f };
                var g1 = new ParticleSystem.Burst(0.62f, 2, 2, 1, 0.01f) { probability = 0.85f };
                var g2 = new ParticleSystem.Burst(1.44f, 2, 2, 1, 0.01f) { probability = 0.90f };
                em.SetBursts(new[] { g0, g1, g2 });

                var shape = psGeo.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.65f;

                var col = psGeo.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#FFE0EA")), (0.30f, Hex("#FF0044")), (0.80f, Hex("#B5179E")), (1.00f, Hex("#4A0080")) },
                    new[] { (0.00f, 0.00f), (0.15f, 0.92f), (0.62f, 0.62f), (1.00f, 0.00f) });

                var sol = psGeo.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.35f), (0.25f, 1.05f), (1.00f, 1.38f));

                var rol = psGeo.rotationOverLifetime;
                rol.enabled = true;
                rol.separateAxes = true;
                rol.x = new ParticleSystem.MinMaxCurve(0f, 0f);
                rol.y = new ParticleSystem.MinMaxCurve(0f, 0f);
                rol.z = new ParticleSystem.MinMaxCurve(-45f * Mathf.Deg2Rad, 45f * Mathf.Deg2Rad);

                var rend = psGeo.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.Mesh;
                rend.mesh = GetBuiltinQuadMesh();
                rend.alignment = ParticleSystemRenderSpace.Local;
                rend.sharedMaterial = matGeo;
                rend.sortingFudge = -1.0f;
            }

            // 5. PS_GroundDreadRing (Arena-spanning 13.5m pulsing floor pressure wave)
            var psRing = CreateChildPS(root, "PS_GroundDreadRing", new Vector3(0f, 0.09f, 0f));
            {
                var main = psRing.main;
                main.duration = 2.0f;
                main.loop = true;
                main.prewarm = true;
                main.startLifetime = 1.45f;
                main.startSpeed = 0f;
                main.startSize = 13.5f;
                main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.maxParticles = 6;

                var em = psRing.emission;
                em.rateOverTime = 0f;
                em.SetBursts(new[]
                {
                    new ParticleSystem.Burst(0.00f, 1),
                    new ParticleSystem.Burst(1.00f, 1)
                });
                var shape = psRing.shape;
                shape.enabled = false;

                var col = psRing.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#4A0080")), (0.25f, Hex("#FF0044")), (0.65f, Hex("#B5179E")), (1.00f, Hex("#1A062B")) },
                    new[] { (0.00f, 0.00f), (0.18f, 0.90f), (0.65f, 0.58f), (1.00f, 0.00f) });

                var sol = psRing.sizeOverLifetime;
                sol.enabled = true;
                sol.size = MakeCurve((0.00f, 0.50f), (1.00f, 1.38f));

                var rol = psRing.rotationOverLifetime;
                rol.enabled = true;
                rol.z = 22f * Mathf.Deg2Rad;

                var rend = psRing.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
                rend.sharedMaterial = matRing;
                rend.sortingFudge = -0.5f;
            }

            // 6. PS_GroundDreadSigil (Colossal 9.5m counter-rotating arcane floor sigil)
            var psSigil = CreateChildPS(root, "PS_GroundDreadSigil", new Vector3(0f, 0.07f, 0f));
            {
                var main = psSigil.main;
                main.duration = 4.0f;
                main.loop = true;
                main.prewarm = true;
                main.startLifetime = 4.0f;
                main.startSpeed = 0f;
                main.startSize = 9.5f;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.maxParticles = 2;

                var em = psSigil.emission;
                em.rateOverTime = 0f;
                em.SetBursts(new[] { new ParticleSystem.Burst(0.0f, 1) });
                var shape = psSigil.shape;
                shape.enabled = false;

                var col = psSigil.colorOverLifetime;
                col.enabled = true;
                col.color = MakeGradient(
                    new[] { (0.00f, Hex("#4A0080")), (0.50f, Hex("#FF0055")), (1.00f, Hex("#4A0080")) },
                    new[] { (0.00f, 0.00f), (0.15f, 0.58f), (0.85f, 0.58f), (1.00f, 0.00f) });

                var rol = psSigil.rotationOverLifetime;
                rol.enabled = true;
                rol.z = -18f * Mathf.Deg2Rad;

                var rend = psSigil.GetComponent<ParticleSystemRenderer>();
                rend.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
                rend.sharedMaterial = matSigil;
                rend.sortingFudge = 0.5f;
            }

            return SaveAndDestroyPrefab(root, PrefabDir + "VFX_FinalBoss_Auramaxxing.prefab");
        }

        // =========================================================================
        // GRAND SHOWCASE SCENE (WIDE ARENA FOR COLOSSAL VFX + CINEMATIC CAMERAS)
        // =========================================================================
        private static void BuildShowcaseScene(GameObject prefab1, GameObject prefab2, GameObject prefab3)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Hex("#1C2238");
            RenderSettings.ambientEquatorColor = Hex("#121524");
            RenderSettings.ambientGroundColor = Hex("#08090F");
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = Hex("#090B14");
            RenderSettings.fogDensity = 0.009f;

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            Material matFloor    = CreateLitMaterial(MatDir + "MAT_Stage_Floor.mat",        litShader, Hex("#161925"), 0.25f, 0.35f);
            Material matPedestal = CreateLitMaterial(MatDir + "MAT_Stage_Pedestal.mat",     litShader, Hex("#23283A"), 0.35f, 0.45f);
            Material matHero     = CreateLitMaterial(MatDir + "MAT_Ref_Silhouette.mat",     litShader, Hex("#3A4056"), 0.20f, 0.45f);
            Material matBoss     = CreateLitMaterial(MatDir + "MAT_Ref_BossSilhouette.mat", litShader, Hex("#1E182B"), 0.45f, 0.55f);

            // Huge Arena Floor with BoxCollider (for Sky Starfall bounce & Lightning Spark ricochets!)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "ArenaFloor";
            floor.transform.position = new Vector3(0f, -0.25f, 4f);
            floor.transform.localScale = new Vector3(110f, 0.5f, 90f);
            floor.GetComponent<Renderer>().sharedMaterial = matFloor;

            GameObject keyLightObj = new GameObject("Directional Light (Key)");
            Light keyLight = keyLightObj.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.color = Hex("#A9C4FF");
            keyLight.intensity = 0.65f;
            keyLight.shadows = LightShadows.Soft;
            keyLightObj.transform.rotation = Quaternion.Euler(42f, -32f, 0f);

            GameObject rimLightObj = new GameObject("Directional Light (Rim)");
            Light rimLight = rimLightObj.AddComponent<Light>();
            rimLight.type = LightType.Directional;
            rimLight.color = Hex("#FF88CC");
            rimLight.intensity = 0.30f;
            rimLight.shadows = LightShadows.None;
            rimLightObj.transform.rotation = Quaternion.Euler(22f, 155f, 0f);

            VolumeProfile profile = CreateShowcaseVolumeProfile(SettingsDir + "VFXShowcaseVolumeProfile.asset");
            GameObject volObj = new GameObject("Global Volume (URP Bloom)");
            Volume vol = volObj.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 10f;
            vol.sharedProfile = profile;

            // --- Stage 1 (Left, X = -20): Extravagant Magical Girl + Sky Starfall ---
            Vector3 pos1 = new Vector3(-20f, 0f, 0f);
            CreatePedestalAndMannequin("Stage_1_MagicalGirl", pos1, 2.0f, 9.0f, matPedestal, matHero, false);
            GameObject inst1 = (GameObject)PrefabUtility.InstantiatePrefab(prefab1);
            inst1.transform.position = pos1;

            // --- Stage 2 (Center, X = -6): Massive Avatar Lightning Bending (shoots 14m across center-front!) ---
            Vector3 pos2 = new Vector3(-6f, 0f, -2.5f);
            CreatePedestalAndMannequin("Stage_2_LightningBending", pos2, 2.0f, 6.0f, matPedestal, matHero, false);
            GameObject inst2 = (GameObject)PrefabUtility.InstantiatePrefab(prefab2);
            inst2.transform.position = pos2 + new Vector3(0.55f, 1.35f, 0.35f);
            inst2.transform.rotation = Quaternion.Euler(0f, 62f, 0f); // Heroic sweeping left-to-right lightning bolt!

            // --- Stage 3 (Right, X = +20): Colossal Final Boss Auramaxxing ---
            Vector3 pos3 = new Vector3(20f, 0f, 0f);
            CreatePedestalAndMannequin("Stage_3_FinalBoss", pos3, 2.8f, 11.5f, matPedestal, matBoss, true);
            GameObject inst3 = (GameObject)PrefabUtility.InstantiatePrefab(prefab3);
            inst3.transform.position = pos3;

            // Main Camera + Cinematic Views
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Hex("#090B14");
            cam.fieldOfView = 48f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 200f;
            camObj.AddComponent<AudioListener>();
            var urpCam = camObj.AddComponent<UniversalAdditionalCameraData>();
            urpCam.renderPostProcessing = true;
            urpCam.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;

            GameObject camRig = new GameObject("CameraViews");
            Transform viewOverview = CreateCamAnchor(camRig, "View_0_Overview",     new Vector3(0f, 9.2f, -38.5f),   new Vector3(0f, 4.0f, 1.0f));
            Transform viewVFX1     = CreateCamAnchor(camRig, "View_1_MagicalGirl",  new Vector3(-20f, 4.6f, -18.5f), pos1 + new Vector3(0f, 4.4f, 0f));
            Transform viewVFX2     = CreateCamAnchor(camRig, "View_2_Lightning",    new Vector3(-1.0f, 4.5f, -18.5f),pos2 + new Vector3(5.8f, 1.8f, 3.2f));
            Transform viewVFX3     = CreateCamAnchor(camRig, "View_3_FinalBoss",    new Vector3(20f, 4.8f, -19.5f),  pos3 + new Vector3(0f, 4.4f, 0f));

            camObj.transform.position = viewOverview.position;
            camObj.transform.rotation = viewOverview.rotation;

            GameObject ctrlObj = new GameObject("VFXShowcaseController");
            var ctrl = ctrlObj.AddComponent<VFXShowcaseController>();
            ctrl.magicalGirlHenshin = inst1.GetComponent<ParticleSystem>();
            ctrl.elementalBendingStrike = inst2.GetComponent<ParticleSystem>();
            ctrl.finalBossAuramaxxing = inst3.GetComponent<ParticleSystem>();
            ctrl.mainCamera = camObj.transform;
            ctrl.cameraViews = new[] { viewOverview, viewVFX1, viewVFX2, viewVFX3 };

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        private static VolumeProfile CreateShowcaseVolumeProfile(string path)
        {
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }
            else
            {
                profile.components.Clear();
            }

            Bloom bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(0.95f);
            bloom.intensity.Override(0.85f);
            bloom.scatter.Override(0.68f);
            bloom.clamp.Override(8.0f);

            Tonemapping tm = profile.Add<Tonemapping>(true);
            tm.mode.Override(TonemappingMode.Neutral);

            Vignette vig = profile.Add<Vignette>(true);
            vig.intensity.Override(0.22f);
            vig.smoothness.Override(0.45f);

            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void CreatePedestalAndMannequin(
            string name,
            Vector3 pos,
            float charHeight,
            float discDiameter,
            Material matPedestal,
            Material matChar,
            bool isBoss)
        {
            GameObject group = new GameObject(name);
            group.transform.position = pos;

            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pad.name = "StageDisc";
            pad.transform.SetParent(group.transform, false);
            pad.transform.localPosition = new Vector3(0f, 0.015f, 0f);
            pad.transform.localScale = new Vector3(discDiameter, 0.015f, discDiameter);
            pad.GetComponent<Renderer>().sharedMaterial = matPedestal;
            UnityEngine.Object.DestroyImmediate(pad.GetComponent<Collider>());

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = isBoss ? "BossSilhouette" : "HeroSilhouette";
            body.transform.SetParent(group.transform, false);
            float radiusScale = isBoss ? 1.0f : 0.62f;
            body.transform.localScale = new Vector3(radiusScale, charHeight * 0.5f, radiusScale);
            body.transform.localPosition = new Vector3(0f, charHeight * 0.5f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = matChar;
            UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());

            if (isBoss)
            {
                GameObject shoulders = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shoulders.name = "BossShoulders";
                shoulders.transform.SetParent(group.transform, false);
                shoulders.transform.localScale = new Vector3(1.9f, 0.42f, 0.78f);
                shoulders.transform.localPosition = new Vector3(0f, charHeight * 0.74f, 0f);
                shoulders.GetComponent<Renderer>().sharedMaterial = matChar;
                UnityEngine.Object.DestroyImmediate(shoulders.GetComponent<Collider>());
            }
        }

        private static Transform CreateCamAnchor(GameObject parent, string name, Vector3 position, Vector3 lookAt)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.position = position;
            go.transform.LookAt(lookAt);
            return go.transform;
        }

        private static Material CreateLitMaterial(string path, Shader shader, Color color, float metallic, float smoothness)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        // =========================================================================
        // PREVIEW CAPTURE UTILITY (VISUAL QA IN BATCHMODE)
        // =========================================================================
        private static void CapturePreviews(string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var ctrl = UnityEngine.Object.FindAnyObjectByType<VFXShowcaseController>();
            Camera cam = Camera.main;
            if (ctrl == null || cam == null)
            {
                Debug.LogWarning("[BuildActivityVFX] Cannot capture previews: controller or camera missing.");
                return;
            }

            // Simulate each system to its peak dramatic frame (stepped cleanly so Trails build!)
            SimulateStepped(ctrl.magicalGirlHenshin, 0.65f, 1f / 60f);
            SimulateStepped(ctrl.elementalBendingStrike, 0.34f, 1f / 60f);
            SimulateStepped(ctrl.finalBossAuramaxxing, 2.08f, 1f / 30f);

            string[] shotNames = { "00_overview.png", "01_magical_girl.png", "02_lightning_bending.png", "03_final_boss.png" };
            for (int i = 0; i < ctrl.cameraViews.Length && i < shotNames.Length; i++)
            {
                cam.transform.position = ctrl.cameraViews[i].position;
                cam.transform.rotation = ctrl.cameraViews[i].rotation;
                RenderCameraToPng(cam, Path.Combine(outputDir, shotNames[i]), 1280, 720);
            }

            ctrl.magicalGirlHenshin.Clear(true);
            ctrl.elementalBendingStrike.Clear(true);
            ctrl.finalBossAuramaxxing.Clear(true);
            Debug.Log($"[BuildActivityVFX] Captured 4 URP preview screenshots to {outputDir}");
        }

        private static void SimulateStepped(ParticleSystem rootPs, float totalSeconds, float step)
        {
            if (rootPs == null) return;
            rootPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            rootPs.Simulate(step, true, true, true);
            float t = step;
            while (t < totalSeconds)
            {
                float dt = Mathf.Min(step, totalSeconds - t);
                rootPs.Simulate(dt, true, false, true);
                t += dt;
            }
        }

        private static void RenderCameraToPng(Camera cam, string filePath, int width, int height)
        {
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var prevTarget = cam.targetTexture;
            var prevActive = RenderTexture.active;

            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            File.WriteAllBytes(filePath, tex.EncodeToPNG());

            cam.targetTexture = prevTarget;
            RenderTexture.active = prevActive;
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(rt);
        }

        // =========================================================================
        // HELPERS
        // =========================================================================
        private static GameObject CreateMasterController(string name, float duration, bool loop, bool prewarm)
        {
            GameObject root = new GameObject(name);
            var ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = duration;
            main.loop = loop;
            main.prewarm = prewarm;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.None;

            var em = ps.emission;
            em.enabled = false;
            var shape = ps.shape;
            shape.enabled = false;
            root.GetComponent<ParticleSystemRenderer>().enabled = false;
            return root;
        }

        private static ParticleSystem CreateChildPS(GameObject parent, string name, Vector3 localPos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPos;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = true;
            return ps;
        }

        private static GameObject SaveAndDestroyPrefab(GameObject root, string prefabPath)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static Mesh GetBuiltinQuadMesh()
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

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }

        private static ParticleSystem.MinMaxGradient MakeRandomPalette(params Color[] colors)
        {
            var g = new Gradient { mode = GradientMode.Fixed };
            var cks = new GradientColorKey[colors.Length];
            for (int i = 0; i < colors.Length; i++)
                cks[i] = new GradientColorKey(colors[i], (i + 1f) / colors.Length);
            var aks = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };
            g.SetKeys(cks, aks);
            return new ParticleSystem.MinMaxGradient(g) { mode = ParticleSystemGradientMode.RandomColor };
        }

        private static ParticleSystem.MinMaxGradient MakeGradient((float t, Color c)[] colorKeys, (float t, float a)[] alphaKeys)
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

        private static ParticleSystem.MinMaxCurve MakeCurve(params (float t, float v)[] keys)
        {
            var kfs = new Keyframe[keys.Length];
            for (int i = 0; i < keys.Length; i++)
                kfs[i] = new Keyframe(keys[i].t, keys[i].v);

            var curve = new AnimationCurve(kfs);
            for (int i = 0; i < kfs.Length; i++)
                AnimationUtility.SetKeyBroken(curve, i, false);

            return new ParticleSystem.MinMaxCurve(1f, curve);
        }
    }
}
