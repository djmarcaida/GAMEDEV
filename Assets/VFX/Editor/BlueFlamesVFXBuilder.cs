using UnityEditor;
using UnityEngine;

namespace GamedevVFX.Editor
{
    internal static class BlueFlamesVFXBuilder
    {
        public static GameObject Build()
        {
            // Use grayscale RGBA textures so blue color gradients never multiply against pre-baked orange hues
            var matVortex    = VFXBuilderUtils.CreateParticleMaterial("MAT_BlueFire_PalmVortex_Add.mat",   VFXBuilderUtils.BrackeysParticles + "twirl_03_a.png",        true,  1.15f);
            var matHeatGlow  = VFXBuilderUtils.CreateParticleMaterial("MAT_BlueFire_HeatGlow_Add.mat",     VFXBuilderUtils.BrackeysParticles + "circle_05_a.png",       true,  0.95f, true, 1.5f);
            var matCoreFlip  = VFXBuilderUtils.CreateParticleMaterial("MAT_BlueFire_CoreFlipbook_Add.mat", VFXBuilderUtils.BrackeysFlipbooks + "wispy_smoke_01_8x8.tga",true,  1.35f, true, 1.2f);
            var matPlume     = VFXBuilderUtils.CreateParticleMaterial("MAT_BlueFire_FlamePlume_Add.mat",   VFXBuilderUtils.BrackeysParticles + "flame_02_a.png",        true,  1.45f, true, 1.0f);
            var matTongue    = VFXBuilderUtils.CreateParticleMaterial("MAT_BlueFire_FlameTongue_Add.mat",  VFXBuilderUtils.BrackeysParticles + "fire_02_a.png",         true,  1.30f, true, 1.2f);
            var matHeatRing  = VFXBuilderUtils.CreateParticleMaterial("MAT_BlueFire_HeatRing_Add.mat",     VFXBuilderUtils.BrackeysParticles + "circle_02_a.png",       true,  1.10f);
            var matEmbers    = VFXBuilderUtils.CreateParticleMaterial("MAT_BlueFire_Embers_Add.mat",       VFXBuilderUtils.BrackeysParticles + "flame_05_a.png",        true,  1.50f);
            var matEmberPuff = VFXBuilderUtils.CreateParticleMaterial("MAT_BlueFire_EmberSmoke_Alpha.mat", VFXBuilderUtils.BrackeysParticles + "smoke_04_a.png",        false, 0.0f,  true, 1.0f);
            var matHeatSmoke = VFXBuilderUtils.CreateParticleMaterial("MAT_BlueFire_HeatSmoke_Alpha.mat",  VFXBuilderUtils.BrackeysFlipbooks + "wispy_smoke_01_8x8.tga",false, 0.0f,  true, 2.2f);

            AssetDatabase.SaveAssets();

            GameObject root = VFXBuilderUtils.CreateRoot("VFX_ElementalBending_AzulaBlueFlames", 2.2f, false);

            AddPalmVortex(root, matVortex);
            AddPalmGlow(root, matHeatGlow);
            AddFireFlipbookCore(root, matCoreFlip);
            AddFlamePlumes(root, matPlume);
            AddFlameTongues(root, matTongue);
            AddHeatWaveRings(root, matHeatRing);
            AddEmbers(root, matEmbers, matEmberPuff);
            AddHeatSmoke(root, matHeatSmoke);

            return VFXBuilderUtils.SavePrefab(root, "VFX_ElementalBending_AzulaBlueFlames.prefab");
        }

        private static void AddPalmVortex(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_PalmFireVortex", new Vector3(0f, 0f, 0.55f));
            var main = ps.main;
            main.duration = 0.55f;
            main.loop = false;
            main.startLifetime = 0.45f;
            main.startSpeed = 0f;
            main.startSize = 4.0f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 6;

            VFXBuilderUtils.SetBursts(ps, 0f,
                new ParticleSystem.Burst(0.00f, 2),
                new ParticleSystem.Burst(0.10f, 2));
            VFXBuilderUtils.DisableShape(ps);
            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[]
                {
                    (0.00f, Color.white),
                    (0.28f, VFXBuilderUtils.Hex("#00EAFF")),
                    (0.72f, VFXBuilderUtils.Hex("#0066FF")),
                    (1.00f, VFXBuilderUtils.Hex("#061878"))
                },
                new[] { (0.00f, 1.00f), (0.38f, 0.88f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.25f), (0.30f, 1.05f), (1.00f, 1.38f));
            VFXBuilderUtils.SetRotationZ(ps, 520f);
            VFXBuilderUtils.ConfigureQuadMesh(ps, mat, -1.5f);
        }

        private static void AddPalmGlow(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_PalmHeatGlow", new Vector3(0f, 0f, 0.55f));
            var main = ps.main;
            main.duration = 0.70f;
            main.loop = false;
            main.startLifetime = 0.60f;
            main.startSpeed = 0f;
            main.startSize = 4.8f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 3;

            VFXBuilderUtils.SetBursts(ps, 0f,
                new ParticleSystem.Burst(0.00f, 1),
                new ParticleSystem.Burst(0.12f, 1));
            VFXBuilderUtils.DisableShape(ps);
            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[] { (0.00f, VFXBuilderUtils.Hex("#E8FFFF")), (0.32f, VFXBuilderUtils.Hex("#00D8FF")), (1.00f, VFXBuilderUtils.Hex("#0055FF")) },
                new[] { (0.00f, 0.85f), (0.30f, 0.65f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.30f), (0.22f, 1.08f), (1.00f, 0.45f));
            VFXBuilderUtils.ConfigureBillboard(ps, mat, ParticleSystemRenderMode.Billboard, 0.5f);
        }

        private static void AddFireFlipbookCore(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_BlueFireFlipbookCore", new Vector3(0f, 0f, 0.85f));
            var main = ps.main;
            main.duration = 0.70f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 0.90f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(22.0f, 34.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(2.6f, 4.8f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 75;

            VFXBuilderUtils.SetBursts(ps, 82f,
                new ParticleSystem.Burst(0.02f, 12),
                new ParticleSystem.Burst(0.16f, 12));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 9.5f;
            shape.radius = 0.35f;
            shape.rotation = Vector3.zero;

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = 0f;
            vel.y = 1.2f;
            vel.z = 0f;

            // Linear drag keeps fast bursts moving forward instead of stalling near the emitter
            var limitVel = ps.limitVelocityOverLifetime;
            limitVel.enabled = true;
            limitVel.drag = 1.35f;
            limitVel.multiplyDragByParticleSize = false;
            limitVel.multiplyDragByParticleVelocity = false;

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 2.0f;
            noise.frequency = 0.95f;
            noise.scrollSpeed = 1.8f;

            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[]
                {
                    (0.00f, Color.white),
                    (0.24f, VFXBuilderUtils.Hex("#40F0FF")),
                    (0.62f, VFXBuilderUtils.Hex("#0077FF")),
                    (1.00f, VFXBuilderUtils.Hex("#0A1D88"))
                },
                new[] { (0.00f, 0.00f), (0.10f, 0.95f), (0.68f, 0.78f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.38f), (0.35f, 1.05f), (1.00f, 1.55f));
            VFXBuilderUtils.SetRotationZ(ps, -110f, 110f);
            VFXBuilderUtils.SetFlipbook(ps, 8, 8, 16);
            VFXBuilderUtils.ConfigureBillboard(ps, mat, ParticleSystemRenderMode.Billboard, -1.0f);
        }

        private static void AddFlamePlumes(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_StylizedFlamePlumes", new Vector3(0f, 0f, 1.5f));
            var main = ps.main;
            main.duration = 0.70f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.38f, 0.65f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(25.0f, 38.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.1f, 2.2f);
            main.flipRotation = 0.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 85;

            VFXBuilderUtils.SetBursts(ps, 88f, new ParticleSystem.Burst(0.02f, 14));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 11f;
            shape.radius = 0.40f;
            shape.rotation = Vector3.zero;

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = 0f;
            vel.y = 0.9f;
            vel.z = 0f;
            vel.orbitalX = 0f;
            vel.orbitalY = 0f;
            vel.orbitalZ = 3.4f;

            var limitVel = ps.limitVelocityOverLifetime;
            limitVel.enabled = true;
            limitVel.drag = 1.25f;
            limitVel.multiplyDragByParticleSize = false;
            limitVel.multiplyDragByParticleVelocity = false;

            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[]
                {
                    (0.00f, VFXBuilderUtils.Hex("#E8FFFF")),
                    (0.28f, VFXBuilderUtils.Hex("#00E5FF")),
                    (0.68f, VFXBuilderUtils.Hex("#0066FF")),
                    (1.00f, VFXBuilderUtils.Hex("#071A78"))
                },
                new[] { (0.00f, 0.00f), (0.14f, 0.96f), (0.68f, 0.78f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.35f), (0.28f, 1.08f), (1.00f, 0.18f));
            VFXBuilderUtils.ConfigureStretched(ps, mat, 0.05f, 1.85f, -2.2f);
        }

        private static void AddFlameTongues(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_BlazingFlameTongues", new Vector3(0f, 0f, 1.1f));
            var main = ps.main;
            main.duration = 0.68f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.42f, 0.72f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(20.0f, 32.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(2.2f, 4.2f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.flipRotation = 0.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 55;

            VFXBuilderUtils.SetBursts(ps, 56f,
                new ParticleSystem.Burst(0.03f, 8),
                new ParticleSystem.Burst(0.20f, 8));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 10f;
            shape.radius = 0.38f;
            shape.rotation = Vector3.zero;

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = 0f;
            vel.y = 1.4f;
            vel.z = 0f;

            var limitVel = ps.limitVelocityOverLifetime;
            limitVel.enabled = true;
            limitVel.drag = 1.4f;
            limitVel.multiplyDragByParticleSize = false;
            limitVel.multiplyDragByParticleVelocity = false;

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 1.8f;
            noise.frequency = 1.2f;
            noise.scrollSpeed = 2.0f;

            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[]
                {
                    (0.00f, Color.white),
                    (0.26f, VFXBuilderUtils.Hex("#00DCFF")),
                    (0.65f, VFXBuilderUtils.Hex("#0055FF")),
                    (1.00f, VFXBuilderUtils.Hex("#061466"))
                },
                new[] { (0.00f, 0.00f), (0.12f, 0.92f), (0.65f, 0.72f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.45f), (0.30f, 1.05f), (1.00f, 1.35f));
            VFXBuilderUtils.SetRotationZ(ps, -180f, 180f);
            VFXBuilderUtils.ConfigureBillboard(ps, mat, ParticleSystemRenderMode.Billboard, -1.8f);
        }

        private static void AddHeatWaveRings(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_HeatWaveRings", new Vector3(0f, 0f, 0.9f));
            var main = ps.main;
            main.duration = 0.60f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.58f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(18.0f, 26.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(3.4f, 5.6f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 10;

            VFXBuilderUtils.SetBursts(ps, 0f,
                new ParticleSystem.Burst(0.02f, 1),
                new ParticleSystem.Burst(0.14f, 1),
                new ParticleSystem.Burst(0.28f, 1),
                new ParticleSystem.Burst(0.42f, 1));

            // Cone with angle 0 sends rings straight along +Z rather than radiating sideways like Circle
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 0f;
            shape.radius = 0.05f;
            shape.rotation = Vector3.zero;

            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[] { (0.00f, VFXBuilderUtils.Hex("#E6FFFF")), (0.42f, VFXBuilderUtils.Hex("#00D8FF")), (1.00f, VFXBuilderUtils.Hex("#0055FF")) },
                new[] { (0.00f, 0.00f), (0.14f, 0.78f), (0.70f, 0.55f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.25f), (0.35f, 0.95f), (1.00f, 1.38f));
            VFXBuilderUtils.ConfigureQuadMesh(ps, mat, -0.8f);
        }

        private static void AddEmbers(GameObject root, Material matEmbers, Material matPuff)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_BlueFireEmbers", new Vector3(0f, -0.10f, 0.8f));
            var sub = VFXBuilderUtils.CreateEmitter(ps.gameObject, "PS_Sub_EmberPuff", Vector3.zero);
            ConfigureEmberPuffSubEmitter(sub, matPuff);

            var main = ps.main;
            main.duration = 0.72f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.85f, 1.55f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(14.0f, 28.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.32f, 0.75f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.startColor = VFXBuilderUtils.MakeRandomPalette(Color.white, VFXBuilderUtils.Hex("#40F0FF"), VFXBuilderUtils.Hex("#0088FF"));
            main.gravityModifier = 0.75f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 110;

            VFXBuilderUtils.SetBursts(ps, 70f,
                new ParticleSystem.Burst(0.03f, 24),
                new ParticleSystem.Burst(0.20f, 18));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 20f;
            shape.radius = 0.55f;
            shape.rotation = new Vector3(-6f, 0f, 0f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 2.0f;
            noise.frequency = 1.2f;
            noise.scrollSpeed = 1.5f;

            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[] { (0.00f, Color.white), (0.45f, VFXBuilderUtils.Hex("#00EAFF")), (1.00f, VFXBuilderUtils.Hex("#0044CC")) },
                new[] { (0.00f, 1.00f), (0.75f, 0.90f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.50f), (0.18f, 1.00f), (1.00f, 0.00f));

            var coll = ps.collision;
            coll.enabled = true;
            coll.type = ParticleSystemCollisionType.World;
            coll.mode = ParticleSystemCollisionMode.Collision3D;
            coll.bounce = 0.45f;
            coll.dampen = 0.35f;
            coll.lifetimeLoss = 0.25f;
            coll.radiusScale = 0.5f;

            var subModule = ps.subEmitters;
            subModule.enabled = true;
            subModule.AddSubEmitter(sub, ParticleSystemSubEmitterType.Collision, ParticleSystemSubEmitterProperties.InheritNothing, 0.45f);

            VFXBuilderUtils.ConfigureStretched(ps, matEmbers, 0.05f, 1.6f, -2.4f);
        }

        private static void ConfigureEmberPuffSubEmitter(ParticleSystem subPs, Material matPuff)
        {
            var main = subPs.main;
            main.duration = 0.45f;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.40f, 0.65f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.9f, 1.6f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 50;

            VFXBuilderUtils.SetBursts(subPs, 0f, new ParticleSystem.Burst(0.0f, 1));

            var shape = subPs.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.20f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            VFXBuilderUtils.SetColorOverLifetime(subPs,
                new[] { (0.00f, VFXBuilderUtils.Hex("#52B8FF")), (0.50f, VFXBuilderUtils.Hex("#1A3566")), (1.00f, VFXBuilderUtils.Hex("#0A1024")) },
                new[] { (0.00f, 0.50f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(subPs, (0.00f, 0.45f), (1.00f, 1.65f));
            VFXBuilderUtils.ConfigureBillboard(subPs, matPuff, ParticleSystemRenderMode.Billboard, 1.0f);
        }

        private static void AddHeatSmoke(GameObject root, Material mat)
        {
            var ps = VFXBuilderUtils.CreateEmitter(root, "PS_DarkBlueHeatSmoke", new Vector3(0f, 0.1f, 0.9f));
            var main = ps.main;
            main.duration = 0.75f;
            main.loop = false;
            main.startDelay = 0.06f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.95f, 1.60f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(14.0f, 24.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(2.8f, 5.2f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 55;

            VFXBuilderUtils.SetBursts(ps, 55f, new ParticleSystem.Burst(0.06f, 12));

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 20f;
            shape.radius = 0.65f;
            shape.rotation = Vector3.zero;

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = 0f;
            vel.y = 1.6f;
            vel.z = 0f;
            vel.orbitalX = 0f;
            vel.orbitalY = 0f;
            vel.orbitalZ = 2.4f;

            var limitVel = ps.limitVelocityOverLifetime;
            limitVel.enabled = true;
            limitVel.drag = 1.6f;
            limitVel.multiplyDragByParticleSize = false;
            limitVel.multiplyDragByParticleVelocity = false;

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 1.2f;
            noise.frequency = 0.65f;
            noise.scrollSpeed = 0.65f;

            VFXBuilderUtils.SetColorOverLifetime(ps,
                new[] { (0.00f, VFXBuilderUtils.Hex("#4DB8FF")), (0.40f, VFXBuilderUtils.Hex("#162E5C")), (1.00f, VFXBuilderUtils.Hex("#0A1024")) },
                new[] { (0.00f, 0.00f), (0.18f, 0.56f), (0.65f, 0.36f), (1.00f, 0.00f) });
            VFXBuilderUtils.SetSizeOverLifetime(ps, (0.00f, 0.45f), (1.00f, 1.90f));
            VFXBuilderUtils.SetRotationZ(ps, -80f, 80f);
            VFXBuilderUtils.SetFlipbook(ps, 8, 8, 16);
            VFXBuilderUtils.ConfigureBillboard(ps, mat, ParticleSystemRenderMode.Billboard, 2.0f);
        }
    }
}
