using UnityEditor;
using UnityEngine;

namespace GamedevVFX.Editor
{
    public static class BuildActivityVFX
    {
        [MenuItem("VFX Activity/Build All VFX Prefabs & Scene")]
        public static void BuildAll()
        {
            VFXBuilderUtils.EnsureFolders();
            VFXBuilderUtils.ConfigureTextures();

            GameObject magicalGirl = MagicalGirlVFXBuilder.Build();
            GameObject blueFlames  = BlueFlamesVFXBuilder.Build();
            GameObject bossAura    = BossAuraVFXBuilder.Build();

            VFXSceneBuilder.Build(magicalGirl, blueFlames, bossAura);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Rebuilt VFX prefabs, materials, and showcase scene.");
        }
    }
}
