using UnityEditor;
using UnityEngine;

namespace GamedevVFX.Editor
{
    internal static class BossAuraVFXBuilder
    {
        public static GameObject Build()
        {
            // Dark smoke uses Alpha blending so near-black purple hues remain visible behind additive layers
            var matSmoke    = VFXBuilderUtils.CreateParticleMaterial("MAT_Boss_AbyssSmoke_Alpha.mat", VFXBuilderUtils.BrackeysFlipbooks + "cloud_01_8x8.tga",       false, 0.0f,  true, 2.2f);
            var matSpikes   = VFXBuilderUtils.CreateParticleMaterial("MAT_Boss_EnergySpike_Add.mat",  VFXBuilderUtils.BrackeysParticles + "trace_06_a.png",         true,  1.40f, true, 1.0f);
            var matBolt     = VFXBuilderUtils.CreateParticleMaterial("MAT_Boss_Lightning_Add.mat",    VFXBuilderUtils.BrackeysParticles + "spark_02_a.png",         true,  1.60f, true, 1.6f);
            var matVertBolt = VFXBuilderUtils.CreateParticleMaterial("MAT_Boss_VertBolt_Add.mat",     VFXBuilderUtils.BrackeysParticles + "spark_06_a.png",         true,  1.65f, true, 1.6f);
            var matGeo      = VFXBuilderUtils.CreateParticleMaterial("MAT_Boss_GeoAccent_Add.mat",    VFXBuilderUtils.KenneyDir + "kenney_decagon_pattern_0024.png",  true,  1.30f);
            var matRing     = VFXBuilderUtils.CreateParticleMaterial("MAT_Boss_DreadRing_Add.mat",    VFXBuilderUtils.BrackeysParticles + "circle_03_a.png",        true,  1.55f, true, 0.6f);
            var matSigil    = VFXBuilderUtils.CreateParticleMaterial("MAT_Boss_FloorSigil_Add.mat",   VFXBuilderUtils.BrackeysParticles + "magic_02_a.png",         true,  1.25f, true, 0.6f);

            AssetDatabase.SaveAssets();

            GameObject root = VFXBuilderUtils.CreateRoot("VFX_FinalBoss_Auramaxxing", 2.0f, true, true);

            AddDarkSmoke(root, matSmoke);
            AddRisingSpikes(root, matSpikes);
            AddHighVoltageCrackles(root, matBolt);
            AddVerticalDreadBolts(root, matVertBolt);
            AddGeometricAccents(root, matGeo);
            AddGroundDreadRing(root, matRing);
            AddGroundSigil(root, matSigil);

            return VFXBuilderUtils.SavePrefab(root, "VFX_FinalBoss_Auramaxxing.prefab");
        }

        private static void AddDarkSmoke(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_AbyssalDarkSmoke", new Vector3(0f, 0.5f, 0f));
            var main = ps.main;
            main.duration = 2.0f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.0f, 3.0f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.4f, 2.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(3.8f, 6.4f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 85;

            VFXBuilderUtils.SetBursts(ps, 30f);

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 11f;
            shape.radius = 2.4f;
            shape.radiusThickness = 0.75f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = 0f;
            vel.y = 3.5f;
            vel.z = 0f;

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 2.1f;
            noise.frequency = 0.45f;
            noise.scrollSpeed = 0.45f;
            noise.octaveCount = 2;
            noise.quality = ParticleSystemNoiseQuality.Medium;

            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[]
                {
                    (0.00f, VFXBuilderUtils.Hex("#1A062B")),
                    (0.45f, VFXBuilderUtils.Hex("#4A0080")),
                    (0.85f, VFXBuilderUtils.Hex("#1F0533")),
                    (1.00f, VFXBuilderUtils.Hex("#0D0216"))
                },
                new[] { (0.00f, 0.00f), (0.20f, 0.88f), (0.65f, 0.76f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.65f), (0.50f, 1.18f), (1.00f, 1.72f));
            VFXBuilderUtils.SetRotationZ(ps, -35f, 35f);
            VFXBuilderUtils.SetFlipbook(ps, 8, 8, 20);
            VFXBuilderUtils.ConfigureBillboard(ps, mat, ParticleSystemRenderMode.Billboard, 2.5f);
        }

        private static void AddRisingSpikes(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_RisingEnergySpikes", new Vector3(0f, 0.25f, 0f));
            var main = ps.main;
            main.duration = 2.0f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.82f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(10.0f, 18.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.42f, 0.88f);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 135;

            VFXBuilderUtils.SetBursts(ps, 88f);

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 6f;
            shape.radius = 3.1f;
            shape.radiusThickness = 0.50f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = 0f;
            vel.y = 4.2f;
            vel.z = 0f;
            vel.radial = -0.85f;

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.95f;
            noise.frequency = 1.4f;
            noise.scrollSpeed = 1.4f;

            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[]
                {
                    (0.00f, VFXBuilderUtils.Hex("#FFD6E0")),
                    (0.25f, VFXBuilderUtils.Hex("#FF003C")),
                    (0.65f, VFXBuilderUtils.Hex("#B5179E")),
                    (1.00f, VFXBuilderUtils.Hex("#4A0080"))
                },
                new[] { (0.00f, 0.00f), (0.15f, 1.00f), (0.72f, 0.76f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.30f), (0.20f, 1.00f), (1.00f, 0.00f));
            VFXBuilderUtils.ConfigureStretched(ps, mat, 0.16f, 3.2f, 0.0f);
        }

        private static void AddHighVoltageCrackles(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_HighVoltageCrackles", new Vector3(0f, 2.8f, 0f));
            var main = ps.main;
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

            VFXBuilderUtils.SetBursts(ps, 0f,
                new ParticleSystem.Burst(0.00f, 3, 5, 1, 0.01f) { probability = 0.96f },
                new ParticleSystem.Burst(0.54f, 3, 5, 1, 0.01f) { probability = 0.92f },
                new ParticleSystem.Burst(1.08f, 2, 4, 1, 0.01f) { probability = 0.88f },
                new ParticleSystem.Burst(1.58f, 3, 5, 1, 0.01f) { probability = 0.95f });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 2.8f;
            shape.radiusThickness = 0.65f;

            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[]
                {
                    (0.00f, VFXBuilderUtils.Hex("#FFE6EE")),
                    (0.30f, VFXBuilderUtils.Hex("#FF003C")),
                    (0.72f, VFXBuilderUtils.Hex("#B5179E")),
                    (1.00f, VFXBuilderUtils.Hex("#4A0080"))
                },
                new[] { (0.00f, 1.00f), (0.55f, 1.00f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.72f), (0.20f, 1.18f), (1.00f, 0.96f));
            VFXBuilderUtils.ConfigureBillboard(ps, mat, ParticleSystemRenderMode.Billboard, -2.0f);
        }

        private static void AddVerticalDreadBolts(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_VerticalDreadBolts", new Vector3(0f, 5.2f, 0f));
            var main = ps.main;
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

            VFXBuilderUtils.SetBursts(ps, 0f,
                new ParticleSystem.Burst(0.02f, 2),
                new ParticleSystem.Burst(0.75f, 2),
                new ParticleSystem.Burst(1.42f, 2));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 3.2f;
            shape.radiusThickness = 0.45f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[] { (0.00f, VFXBuilderUtils.Hex("#FFE0EC")), (0.35f, VFXBuilderUtils.Hex("#FF0044")), (1.00f, VFXBuilderUtils.Hex("#8E00B8")) },
                new[] { (0.00f, 1.00f), (0.60f, 0.90f), (1.00f, 0.00f) });
            VFXBuilderUtils.ConfigureBillboard(ps, mat, ParticleSystemRenderMode.VerticalBillboard, -2.2f);
        }

        private static void AddGeometricAccents(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_GeometricRuneAccents", new Vector3(0f, 2.7f, 0f));
            var main = ps.main;
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

            VFXBuilderUtils.SetBursts(ps, 0f,
                new ParticleSystem.Burst(0.02f, 2, 2, 1, 0.01f) { probability = 0.92f },
                new ParticleSystem.Burst(0.62f, 2, 2, 1, 0.01f) { probability = 0.85f },
                new ParticleSystem.Burst(1.44f, 2, 2, 1, 0.01f) { probability = 0.90f });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.65f;

            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[]
                {
                    (0.00f, VFXBuilderUtils.Hex("#FFE0EA")),
                    (0.30f, VFXBuilderUtils.Hex("#FF0044")),
                    (0.80f, VFXBuilderUtils.Hex("#B5179E")),
                    (1.00f, VFXBuilderUtils.Hex("#4A0080"))
                },
                new[] { (0.00f, 0.00f), (0.15f, 0.92f), (0.62f, 0.62f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.35f), (0.25f, 1.05f), (1.00f, 1.38f));
            VFXBuilderUtils.SetRotationZ(ps, -45f, 45f);
            VFXBuilderUtils.ConfigureQuadMesh(ps, mat, -1.0f);
        }

        private static void AddGroundDreadRing(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_GroundDreadRing", new Vector3(0f, 0.09f, 0f));
            var main = ps.main;
            main.duration = 2.0f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = 1.45f;
            main.startSpeed = 0f;
            main.startSize = 13.5f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 6;

            VFXBuilderUtils.SetBursts(ps, 0f,
                new ParticleSystem.Burst(0.00f, 1),
                new ParticleSystem.Burst(1.00f, 1));
            VFXBuilderUtils.DisableShape(ps);
            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[]
                {
                    (0.00f, VFXBuilderUtils.Hex("#4A0080")),
                    (0.25f, VFXBuilderUtils.Hex("#FF0044")),
                    (0.65f, VFXBuilderUtils.Hex("#B5179E")),
                    (1.00f, VFXBuilderUtils.Hex("#1A062B"))
                },
                new[] { (0.00f, 0.00f), (0.18f, 0.90f), (0.65f, 0.58f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.50f), (1.00f, 1.38f));
            VFXBuilderUtils.SetRotationZ(ps, 22f);
            VFXBuilderUtils.ConfigureBillboard(ps, mat, ParticleSystemRenderMode.HorizontalBillboard, -0.5f);
        }

        private static void AddGroundSigil(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_GroundDreadSigil", new Vector3(0f, 0.07f, 0f));
            var main = ps.main;
            main.duration = 4.0f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = 4.0f;
            main.startSpeed = 0f;
            main.startSize = 9.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 2;

            VFXBuilderUtils.SetBursts(ps, 0f, new ParticleSystem.Burst(0.0f, 1));
            VFXBuilderUtils.DisableShape(ps);
            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[] { (0.00f, VFXBuilderUtils.Hex("#4A0080")), (0.50f, VFXBuilderUtils.Hex("#FF0055")), (1.00f, VFXBuilderUtils.Hex("#4A0080")) },
                new[] { (0.00f, 0.00f), (0.15f, 0.58f), (0.85f, 0.58f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetRotationZ(ps, -18f);
            VFXBuilderUtils.ConfigureBillboard(ps, mat, ParticleSystemRenderMode.HorizontalBillboard, 0.5f);
        }
    }
}
