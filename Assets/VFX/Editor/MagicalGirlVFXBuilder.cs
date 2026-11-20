using UnityEditor;
using UnityEngine;

namespace GamedevVFX.Editor
{
    internal static class MagicalGirlVFXBuilder
    {
        public static GameObject Build()
        {
            var matCore      = VFXBuilderUtils.CreateParticleMaterial("MAT_MG_CoreGlow_Add.mat",    VFXBuilderUtils.BrackeysParticles + "circle_05_a.png", true, 0.75f, true,  1.6f);
            var matPillar    = VFXBuilderUtils.CreateParticleMaterial("MAT_MG_LightPillar_Add.mat", VFXBuilderUtils.BrackeysParticles + "trace_06_a.png",  true, 0.90f, true,  1.8f);
            var matSigil     = VFXBuilderUtils.CreateParticleMaterial("MAT_MG_FloorSigil_Add.mat",  VFXBuilderUtils.BrackeysParticles + "magic_01_a.png",  true, 1.10f, true,  0.6f);
            var matRing      = VFXBuilderUtils.CreateParticleMaterial("MAT_MG_EnergyRing_Add.mat",  VFXBuilderUtils.BrackeysParticles + "circle_02_a.png", true, 1.15f);
            var matRibbon    = VFXBuilderUtils.CreateParticleMaterial("MAT_MG_Ribbon_Add.mat",      VFXBuilderUtils.BrackeysParticles + "twirl_02_a.png",  true, 1.05f);
            var matSparkle   = VFXBuilderUtils.CreateParticleMaterial("MAT_MG_Sparkle_Add.mat",     VFXBuilderUtils.BrackeysParticles + "star_08_a.png",   true, 1.25f);
            var matStar5Pt   = VFXBuilderUtils.CreateParticleMaterial("MAT_MG_SkyStar5Pt_Add.mat",  VFXBuilderUtils.BrackeysParticles + "symbol_02_a.png", true, 1.15f);
            var matCometHead = VFXBuilderUtils.CreateParticleMaterial("MAT_MG_CometHead_Add.mat",   VFXBuilderUtils.BrackeysParticles + "star_09_a.png",   true, 1.35f);
            var matCometTail = VFXBuilderUtils.CreateParticleMaterial("MAT_MG_CometTrail_Add.mat",  VFXBuilderUtils.KenneyDir + "kenney_comet_trail_beam.png", true, 1.20f);
            var matSubStar   = VFXBuilderUtils.CreateParticleMaterial("MAT_MG_GlitterPop_Add.mat",  VFXBuilderUtils.BrackeysParticles + "star_07_a.png",   true, 1.35f);
            var matShockwave = VFXBuilderUtils.CreateParticleMaterial("MAT_MG_Shockwave_Add.mat",   VFXBuilderUtils.BrackeysParticles + "effect_02_a.png", true, 1.10f, true,  1.8f);
            var matFlare     = VFXBuilderUtils.CreateParticleMaterial("MAT_MG_Flare_Add.mat",       VFXBuilderUtils.BrackeysParticles + "flare_01_a.png",  true, 1.35f, true,  1.2f);

            AssetDatabase.SaveAssets();

            GameObject root = VFXBuilderUtils.CreateRoot("VFX_MagicalGirl_Henshin", 3.4f, false);

            AddCoreAura(root, matCore);
            AddFloorSigil(root, matSigil);
            AddLightPillar(root, matPillar);
            AddEnergyRings(root, matRing);
            AddLuminousRibbons(root, matRibbon);
            AddSpiralMotes(root, matSparkle, matSubStar);
            AddSkyStarfall(root, matStar5Pt, matSubStar);
            AddShootingStars(root, matCometHead, matCometTail);
            AddImpactFlash(root, matShockwave, matFlare);

            return VFXBuilderUtils.SavePrefab(root, "VFX_MagicalGirl_Henshin.prefab");
        }

        private static void AddCoreAura(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_CoreAuraGlow", new Vector3(0f, 1.8f, 0f));
            var main = ps.main;
            main.duration = 1.2f;
            main.loop = false;
            main.startLifetime = 1.0f;
            main.startSpeed = 0f;
            main.startSize = 7.0f;
            main.startColor = Color.white;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 4;

            VFXBuilderUtils.SetBursts(ps, 0f,
                new ParticleSystem.Burst(0.00f, 1),
                new ParticleSystem.Burst(0.10f, 1));
            VFXBuilderUtils.DisableShape(ps);
            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[] { (0.00f, VFXBuilderUtils.Hex("#FF148A")), (0.50f, VFXBuilderUtils.Hex("#B829FF")), (1.00f, VFXBuilderUtils.Hex("#00E5FF")) },
                new[] { (0.00f, 0.00f), (0.14f, 0.65f), (0.55f, 0.30f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.35f), (0.28f, 1.05f), (1.00f, 1.45f));
            VFXBuilderUtils.ConfigureBillboard(ps, mat, ParticleSystemRenderMode.Billboard, 2.5f);
        }

        private static void AddFloorSigil(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_CelestialFloorSigil", new Vector3(0f, 0.08f, 0f));
            var main = ps.main;
            main.duration = 2.6f;
            main.loop = false;
            main.startLifetime = 2.6f;
            main.startSpeed = 0f;
            main.startSize = 10.5f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 3;

            VFXBuilderUtils.SetBursts(ps, 0f, new ParticleSystem.Burst(0.00f, 1));
            VFXBuilderUtils.DisableShape(ps);
            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[]
                {
                    (0.00f, VFXBuilderUtils.Hex("#FF148A")),
                    (0.45f, VFXBuilderUtils.Hex("#B429FF")),
                    (0.80f, VFXBuilderUtils.Hex("#00EAFF")),
                    (1.00f, VFXBuilderUtils.Hex("#FFE047"))
                },
                new[] { (0.00f, 0.00f), (0.10f, 0.90f), (0.78f, 0.72f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.25f), (0.14f, 1.00f), (0.85f, 1.08f), (1.00f, 1.25f));
            VFXBuilderUtils.SetRotationZ(ps, 38f);
            VFXBuilderUtils.ConfigureBillboard(ps, mat, ParticleSystemRenderMode.HorizontalBillboard, 1.5f);
        }

        private static void AddLightPillar(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_CelestialLightPillar", new Vector3(0f, 6.2f, 0f));
            var main = ps.main;
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

            VFXBuilderUtils.SetBursts(ps, 0f, new ParticleSystem.Burst(0.0f, 2));
            VFXBuilderUtils.DisableShape(ps);
            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[]
                {
                    (0.00f, VFXBuilderUtils.Hex("#FFE047")),
                    (0.35f, VFXBuilderUtils.Hex("#FF148A")),
                    (0.75f, VFXBuilderUtils.Hex("#00EAFF")),
                    (1.00f, VFXBuilderUtils.Hex("#00EAFF"))
                },
                new[] { (0.00f, 0.00f), (0.14f, 0.65f), (0.55f, 0.35f), (1.00f, 0.00f) });

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.separateAxes = true;
            sol.x = VFXBuilderUtils.MakeCurve((0.00f, 0.25f), (0.20f, 1.00f), (1.00f, 1.55f));
            sol.y = VFXBuilderUtils.MakeCurve((0.00f, 1.00f), (1.00f, 1.05f));
            sol.z = VFXBuilderUtils.MakeCurve((0.00f, 1.00f), (1.00f, 1.00f));

            VFXBuilderUtils.ConfigureBillboard(ps, mat, ParticleSystemRenderMode.VerticalBillboard, 2.0f);
        }

        private static void AddEnergyRings(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_EnergyRings", new Vector3(0f, 2.0f, 0f));
            var main = ps.main;
            main.duration = 1.4f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.75f, 1.05f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(6.2f, 9.8f);
            main.startRotation3D = true;
            main.startRotationX = new ParticleSystem.MinMaxCurve(58f * Mathf.Deg2Rad, 122f * Mathf.Deg2Rad);
            main.startRotationY = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.startRotationZ = new ParticleSystem.MinMaxCurve(-35f * Mathf.Deg2Rad, 35f * Mathf.Deg2Rad);
            main.startColor = VFXBuilderUtils.MakeRandomPalette(
                VFXBuilderUtils.Hex("#FF148A"),
                VFXBuilderUtils.Hex("#00EAFF"),
                VFXBuilderUtils.Hex("#FFD83D"));
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 18;

            VFXBuilderUtils.SetBursts(ps, 0f,
                new ParticleSystem.Burst(0.04f, 3),
                new ParticleSystem.Burst(0.18f, 3),
                new ParticleSystem.Burst(0.36f, 3),
                new ParticleSystem.Burst(0.58f, 2));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.15f, 2.6f, 0.15f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = 0f;
            vel.y = 1.4f;
            vel.z = 0f;

            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[] { (0.00f, Color.white), (1.00f, Color.white) },
                new[] { (0.00f, 0.00f), (0.12f, 1.00f), (0.72f, 0.85f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.14f), (0.28f, 0.88f), (1.00f, 1.32f));
            VFXBuilderUtils.SetRotationZ(ps, -170f, 170f);
            VFXBuilderUtils.ConfigureQuadMesh(ps, mat, 0.0f);
        }

        private static void AddLuminousRibbons(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_LuminousRibbons", new Vector3(0f, 0.25f, 0f));
            var main = ps.main;
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

            VFXBuilderUtils.SetBursts(ps, 20f);

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.25f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = 0f;
            vel.y = 5.4f;
            vel.z = 0f;

            // Route through violet so magenta-to-cyan stays saturated instead of fading through gray
            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[] { (0.00f, VFXBuilderUtils.Hex("#FF148A")), (0.48f, VFXBuilderUtils.Hex("#A82BFF")), (1.00f, VFXBuilderUtils.Hex("#00EAFF")) },
                new[] { (0.00f, 0.00f), (0.18f, 0.90f), (0.74f, 0.65f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.55f), (0.42f, 1.08f), (1.00f, 0.35f));
            VFXBuilderUtils.SetRotationZ(ps, 330f);
            VFXBuilderUtils.ConfigureQuadMesh(ps, mat, -0.5f);
        }

        private static void AddSpiralMotes(GameObject root, Material matSparkle, Material matSubStar)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_StarlightSpiralMotes", new Vector3(0f, 0.25f, 0f));
            var sub = VFXBuilderUtils.CreateEmitter(ps.gameObject, "PS_Sub_GlitterPop", Vector3.zero);
            ConfigureGlitterSubEmitter(sub, matSubStar, 0.35f, 0.65f);

            var main = ps.main;
            main.duration = 1.6f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.0f, 1.65f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.2f, 4.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.45f, 1.15f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.startColor = VFXBuilderUtils.MakeRandomPalette(
                VFXBuilderUtils.Hex("#FF148A"),
                VFXBuilderUtils.Hex("#00EAFF"),
                VFXBuilderUtils.Hex("#FFE047"));
            main.gravityModifier = -0.22f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 140;

            VFXBuilderUtils.SetBursts(ps, 65f,
                new ParticleSystem.Burst(0.00f, 24),
                new ParticleSystem.Burst(0.12f, 32));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 16f;
            shape.radius = 1.9f;
            shape.radiusThickness = 0.55f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = 0f;
            vel.y = 4.2f;
            vel.z = 0f;
            vel.orbitalX = 0f;
            vel.orbitalY = 4.2f;
            vel.orbitalZ = 0f;
            vel.radial = 1.1f;

            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[] { (0.00f, Color.white), (1.00f, Color.white) },
                new[] { (0.00f, 0.00f), (0.12f, 1.00f), (0.80f, 0.92f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.00f), (0.14f, 1.18f), (0.30f, 0.92f), (1.00f, 0.00f));
            VFXBuilderUtils.SetRotationZ(ps, -180f, 180f);

            var subModule = ps.subEmitters;
            subModule.enabled = true;
            subModule.AddSubEmitter(sub, ParticleSystemSubEmitterType.Death, ParticleSystemSubEmitterProperties.InheritColor, 0.40f);

            VFXBuilderUtils.ConfigureBillboard(ps, matSparkle, ParticleSystemRenderMode.Billboard, -1.5f);
        }

        private static void AddSkyStarfall(GameObject root, Material matStar5Pt, Material matSubStar)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_SkyStarfallShower", new Vector3(0f, 10.5f, 0f));
            var sub = VFXBuilderUtils.CreateEmitter(ps.gameObject, "PS_Sub_StarBounceSparkle", Vector3.zero);
            ConfigureGlitterSubEmitter(sub, matSubStar, 0.45f, 0.90f);

            var main = ps.main;
            main.duration = 2.0f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.45f, 2.20f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(4.5f, 8.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.48f, 1.12f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.startColor = VFXBuilderUtils.MakeRandomPalette(
                VFXBuilderUtils.Hex("#FFE047"),
                VFXBuilderUtils.Hex("#FF148A"),
                VFXBuilderUtils.Hex("#00EAFF"),
                VFXBuilderUtils.Hex("#FFF6B3"));
            main.gravityModifier = 0.55f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 220;

            VFXBuilderUtils.SetBursts(ps, 85f,
                new ParticleSystem.Burst(0.00f, 45),
                new ParticleSystem.Burst(0.25f, 35),
                new ParticleSystem.Burst(0.65f, 30));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 5f;
            shape.radius = 6.8f;
            shape.radiusThickness = 0.90f;
            shape.rotation = new Vector3(90f, 0f, 0f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = 0f;
            vel.y = 0f;
            vel.z = 0f;
            vel.orbitalX = 0f;
            vel.orbitalY = 0.85f;
            vel.orbitalZ = 0f;

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 1.15f;
            noise.frequency = 0.7f;
            noise.scrollSpeed = 0.8f;

            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[] { (0.00f, Color.white), (1.00f, Color.white) },
                new[] { (0.00f, 0.00f), (0.10f, 1.00f), (0.82f, 0.95f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.20f), (0.12f, 1.15f), (0.82f, 0.90f), (1.00f, 0.00f));
            VFXBuilderUtils.SetRotationZ(ps, -240f, 240f);

            var coll = ps.collision;
            coll.enabled = true;
            coll.type = ParticleSystemCollisionType.World;
            coll.mode = ParticleSystemCollisionMode.Collision3D;
            coll.bounce = 0.42f;
            coll.dampen = 0.25f;
            coll.lifetimeLoss = 0.35f;
            coll.radiusScale = 0.5f;

            var subModule = ps.subEmitters;
            subModule.enabled = true;
            subModule.AddSubEmitter(sub, ParticleSystemSubEmitterType.Collision, ParticleSystemSubEmitterProperties.InheritColor, 0.75f);

            VFXBuilderUtils.ConfigureBillboard(ps, matStar5Pt, ParticleSystemRenderMode.Billboard, -2.0f);
        }

        private static void AddShootingStars(GameObject root, Material matHead, Material matTail)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_FallingShootingStars", new Vector3(1.8f, 12.2f, 0f));
            var main = ps.main;
            main.duration = 1.9f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.85f, 1.35f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(12.0f, 18.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.85f, 1.55f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.startColor = VFXBuilderUtils.MakeRandomPalette(
                VFXBuilderUtils.Hex("#FFE047"),
                VFXBuilderUtils.Hex("#00EAFF"),
                VFXBuilderUtils.Hex("#FF148A"));
            main.gravityModifier = 0.35f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 45;

            VFXBuilderUtils.SetBursts(ps, 20f,
                new ParticleSystem.Burst(0.02f, 12),
                new ParticleSystem.Burst(0.35f, 10),
                new ParticleSystem.Burst(0.80f, 8));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(13.0f, 1.5f, 10.0f);
            shape.rotation = new Vector3(68f, -24f, 0f);

            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[] { (0.00f, Color.white), (1.00f, Color.white) },
                new[] { (0.00f, 0.00f), (0.10f, 1.00f), (0.85f, 0.90f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.35f), (0.15f, 1.15f), (0.85f, 0.85f), (1.00f, 0.00f));
            VFXBuilderUtils.SetRotationZ(ps, -300f, 300f);

            var trails = ps.trails;
            trails.enabled = true;
            trails.mode = ParticleSystemTrailMode.PerParticle;
            trails.ratio = 1.0f;
            trails.lifetime = new ParticleSystem.MinMaxCurve(0.28f);
            trails.minVertexDistance = 0.15f;
            trails.worldSpace = true;
            trails.dieWithParticles = false;
            trails.sizeAffectsWidth = true;
            trails.inheritParticleColor = true;
            trails.widthOverTrail = VFXBuilderUtils.MakeCurve((0.00f, 0.24f), (1.00f, 0.00f));
            trails.colorOverTrail = VFXBuilderUtils.MakeGradient(
                new[] { (0.00f, Color.white), (1.00f, Color.white) },
                new[] { (0.00f, 0.95f), (1.00f, 0.00f) });

            var coll = ps.collision;
            coll.enabled = true;
            coll.type = ParticleSystemCollisionType.World;
            coll.mode = ParticleSystemCollisionMode.Collision3D;
            coll.bounce = 0.20f;
            coll.lifetimeLoss = 0.85f;

            var rend = ps.GetComponent<ParticleSystemRenderer>();
            rend.renderMode = ParticleSystemRenderMode.Billboard;
            rend.sharedMaterial = matHead;
            rend.trailMaterial = matTail;
            rend.sortingFudge = -2.2f;
        }

        private static void AddImpactFlash(GameObject root, Material matShockwave, Material matFlare)
        {
            var psShock = VFXBuilderUtils.CreateEmitter(root, "PS_OuterFlashShockwave", new Vector3(0f, 2.4f, 0f));
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

                VFXBuilderUtils.SetBursts(psShock, 0f, new ParticleSystem.Burst(0.0f, 1));
                VFXBuilderUtils.DisableShape(psShock);
                VFXBuilderUtils.SetColorOverLifetime(psShock,
                    new[]
                    {
                        (0.00f, VFXBuilderUtils.Hex("#FFE047")),
                        (0.30f, VFXBuilderUtils.Hex("#FF148A")),
                        (0.75f, VFXBuilderUtils.Hex("#B429FF")),
                        (1.00f, VFXBuilderUtils.Hex("#00EAFF"))
                    },
                    new[] { (0.00f, 0.95f), (0.30f, 0.85f), (0.75f, 0.35f), (1.00f, 0.00f) });
                VFXBuilderUtils.SetSizeOverLifetime(psShock, (0.00f, 0.14f), (0.25f, 0.94f), (1.00f, 1.32f));
                VFXBuilderUtils.SetRotationZ(psShock, 85f);
                VFXBuilderUtils.ConfigureBillboard(psShock, matShockwave, ParticleSystemRenderMode.Billboard, -2.5f);
            }

            var psFlare = VFXBuilderUtils.CreateEmitter(root, "PS_AnamorphicSnapFlare", new Vector3(0f, 2.2f, 0f));
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

                VFXBuilderUtils.SetBursts(psFlare, 0f, new ParticleSystem.Burst(0.0f, 1));
                VFXBuilderUtils.DisableShape(psFlare);
                VFXBuilderUtils.SetColorOverLifetime(psFlare,
                    new[] { (0.00f, VFXBuilderUtils.Hex("#FFE66D")), (0.40f, VFXBuilderUtils.Hex("#FF148A")), (1.00f, VFXBuilderUtils.Hex("#00EAFF")) },
                    new[] { (0.00f, 1.00f), (0.40f, 0.88f), (1.00f, 0.00f) });
                VFXBuilderUtils.SetSizeOverLifetime(psFlare, (0.00f, 0.20f), (0.20f, 1.00f), (1.00f, 0.00f));
                VFXBuilderUtils.ConfigureBillboard(psFlare, matFlare, ParticleSystemRenderMode.Billboard, -3.0f);
            }
        }

        private static void ConfigureGlitterSubEmitter(ParticleSystem subPs, Material mat, float minSize, float maxSize)
        {
            var main = subPs.main;
            main.duration = 0.40f;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.28f, 0.45f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 140;

            VFXBuilderUtils.SetBursts(subPs, 0f, new ParticleSystem.Burst(0.0f, 3));

            var shape = subPs.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.22f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            VFXBuilderUtils.SetColorOverLifetime(subPs,
                new[] { (0.00f, Color.white), (1.00f, Color.white) },
                new[] { (0.00f, 1.00f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(subPs, (0.00f, 0.20f), (0.25f, 1.00f), (1.00f, 0.00f));
            VFXBuilderUtils.ConfigureBillboard(subPs, mat, ParticleSystemRenderMode.Billboard, -2.5f);
        }
    }
}
