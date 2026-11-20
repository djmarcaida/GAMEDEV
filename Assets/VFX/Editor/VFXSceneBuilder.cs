using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace GamedevVFX.Editor
{
    internal static class VFXSceneBuilder
    {
        public static void Build(GameObject magicalGirlPrefab, GameObject blueFlamesPrefab, GameObject bossAuraPrefab)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ConfigureLighting();

            Material matFloor    = VFXBuilderUtils.CreateLitMaterial("MAT_Stage_Floor.mat",        VFXBuilderUtils.Hex("#161925"), 0.25f, 0.35f);
            Material matPedestal = VFXBuilderUtils.CreateLitMaterial("MAT_Stage_Pedestal.mat",     VFXBuilderUtils.Hex("#23283A"), 0.35f, 0.45f);
            Material matHero     = VFXBuilderUtils.CreateLitMaterial("MAT_Ref_Silhouette.mat",     VFXBuilderUtils.Hex("#3A4056"), 0.20f, 0.45f);
            Material matBoss     = VFXBuilderUtils.CreateLitMaterial("MAT_Ref_BossSilhouette.mat", VFXBuilderUtils.Hex("#1E182B"), 0.45f, 0.55f);

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "ArenaFloor";
            floor.transform.position = new Vector3(0f, -0.25f, 4f);
            floor.transform.localScale = new Vector3(110f, 0.5f, 90f);
            floor.GetComponent<Renderer>().sharedMaterial = matFloor;

            VolumeProfile profile = CreateVolumeProfile(VFXBuilderUtils.SettingsDir + "VFXShowcaseVolumeProfile.asset");
            var volObj = new GameObject("Global Volume (URP Bloom)");
            var vol = volObj.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 10f;
            vol.sharedProfile = profile;

            Vector3 pos1 = new Vector3(-20f, 0f, 0f);
            CreateStage("Stage_1_MagicalGirl", pos1, 2.0f, 9.0f, matPedestal, matHero, false);
            var inst1 = (GameObject)PrefabUtility.InstantiatePrefab(magicalGirlPrefab);
            inst1.transform.position = pos1;

            Vector3 pos2 = new Vector3(-6f, 0f, -2.5f);
            CreateStage("Stage_2_AzulaBlueFlames", pos2, 2.0f, 6.0f, matPedestal, matHero, false);
            var inst2 = (GameObject)PrefabUtility.InstantiatePrefab(blueFlamesPrefab);
            inst2.transform.position = pos2 + new Vector3(0.55f, 1.35f, 0.35f);
            inst2.transform.rotation = Quaternion.Euler(0f, 62f, 0f);

            Vector3 pos3 = new Vector3(20f, 0f, 0f);
            CreateStage("Stage_3_FinalBoss", pos3, 2.8f, 11.5f, matPedestal, matBoss, true);
            var inst3 = (GameObject)PrefabUtility.InstantiatePrefab(bossAuraPrefab);
            inst3.transform.position = pos3;

            var camObj = new GameObject("Main Camera") { tag = "MainCamera" };
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = VFXBuilderUtils.Hex("#090B14");
            cam.fieldOfView = 48f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 200f;
            camObj.AddComponent<AudioListener>();

            var urpCam = camObj.AddComponent<UniversalAdditionalCameraData>();
            urpCam.renderPostProcessing = true;
            urpCam.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;

            var camRig = new GameObject("CameraViews");
            Transform viewOverview = CreateCameraAnchor(camRig, "View_0_Overview",        new Vector3(0f, 9.2f, -38.5f),   new Vector3(0f, 4.0f, 1.0f));
            Transform viewVFX1     = CreateCameraAnchor(camRig, "View_1_MagicalGirl",     new Vector3(-20f, 4.6f, -18.5f), pos1 + new Vector3(0f, 4.4f, 0f));
            Transform viewVFX2     = CreateCameraAnchor(camRig, "View_2_AzulaBlueFlames", new Vector3(-1.0f, 4.5f, -18.5f),pos2 + new Vector3(5.8f, 1.8f, 3.2f));
            Transform viewVFX3     = CreateCameraAnchor(camRig, "View_3_FinalBoss",       new Vector3(20f, 4.8f, -19.5f),  pos3 + new Vector3(0f, 4.4f, 0f));

            camObj.transform.position = viewOverview.position;
            camObj.transform.rotation = viewOverview.rotation;

            var ctrlObj = new GameObject("VFXShowcaseController");
            var ctrl = ctrlObj.AddComponent<VFXShowcaseController>();
            ctrl.magicalGirlHenshin = inst1.GetComponent<ParticleSystem>();
            ctrl.elementalBendingStrike = inst2.GetComponent<ParticleSystem>();
            ctrl.finalBossAuramaxxing = inst3.GetComponent<ParticleSystem>();
            ctrl.mainCamera = camObj.transform;
            ctrl.cameraViews = new[] { viewOverview, viewVFX1, viewVFX2, viewVFX3 };

            EditorSceneManager.SaveScene(scene, VFXBuilderUtils.ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(VFXBuilderUtils.ScenePath, true) };
        }

        private static void ConfigureLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = VFXBuilderUtils.Hex("#1C2238");
            RenderSettings.ambientEquatorColor = VFXBuilderUtils.Hex("#121524");
            RenderSettings.ambientGroundColor = VFXBuilderUtils.Hex("#08090F");
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = VFXBuilderUtils.Hex("#090B14");
            RenderSettings.fogDensity = 0.009f;

            var keyLightObj = new GameObject("Directional Light (Key)");
            Light keyLight = keyLightObj.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.color = VFXBuilderUtils.Hex("#A9C4FF");
            keyLight.intensity = 0.65f;
            keyLight.shadows = LightShadows.Soft;
            keyLightObj.transform.rotation = Quaternion.Euler(42f, -32f, 0f);

            var rimLightObj = new GameObject("Directional Light (Rim)");
            Light rimLight = rimLightObj.AddComponent<Light>();
            rimLight.type = LightType.Directional;
            rimLight.color = VFXBuilderUtils.Hex("#FF88CC");
            rimLight.intensity = 0.30f;
            rimLight.shadows = LightShadows.None;
            rimLightObj.transform.rotation = Quaternion.Euler(22f, 155f, 0f);
        }

        private static VolumeProfile CreateVolumeProfile(string path)
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

            // Neutral tonemapping preserves saturated neon colors better than ACES under HDR bloom
            Tonemapping tm = profile.Add<Tonemapping>(true);
            tm.mode.Override(TonemappingMode.Neutral);

            Vignette vig = profile.Add<Vignette>(true);
            vig.intensity.Override(0.22f);
            vig.smoothness.Override(0.45f);

            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void CreateStage(
            string name,
            Vector3 pos,
            float charHeight,
            float discDiameter,
            Material matPedestal,
            Material matChar,
            bool isBoss)
        {
            var group = new GameObject(name);
            group.transform.position = pos;

            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pad.name = "StageDisc";
            pad.transform.SetParent(group.transform, false);
            pad.transform.localPosition = new Vector3(0f, 0.015f, 0f);
            pad.transform.localScale = new Vector3(discDiameter, 0.015f, discDiameter);
            pad.GetComponent<Renderer>().sharedMaterial = matPedestal;
            Object.DestroyImmediate(pad.GetComponent<Collider>());

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = isBoss ? "BossSilhouette" : "HeroSilhouette";
            body.transform.SetParent(group.transform, false);
            float radiusScale = isBoss ? 1.0f : 0.62f;
            body.transform.localScale = new Vector3(radiusScale, charHeight * 0.5f, radiusScale);
            body.transform.localPosition = new Vector3(0f, charHeight * 0.5f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = matChar;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            if (isBoss)
            {
                GameObject shoulders = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shoulders.name = "BossShoulders";
                shoulders.transform.SetParent(group.transform, false);
                shoulders.transform.localScale = new Vector3(1.9f, 0.42f, 0.78f);
                shoulders.transform.localPosition = new Vector3(0f, charHeight * 0.74f, 0f);
                shoulders.GetComponent<Renderer>().sharedMaterial = matChar;
                Object.DestroyImmediate(shoulders.GetComponent<Collider>());
            }
        }

        private static Transform CreateCameraAnchor(GameObject parent, string name, Vector3 position, Vector3 lookAt)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.position = position;
            go.transform.LookAt(lookAt);
            return go.transform;
        }
    }
}
