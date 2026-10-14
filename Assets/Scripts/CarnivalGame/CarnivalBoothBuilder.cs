using System.Collections.Generic;
using UnityEngine;

namespace CarnivalGame
{
    [ExecuteAlways]
    public class CarnivalBoothBuilder : MonoBehaviour
    {
        [SerializeField] private bool autoBuildOnAwake = true;

        private Material _stripedAwningMat;
        private Material _redCylinderMat;
        private Material _woodFloorMat;
        private Material _woodShelfMat;
        private Material _boothWallMat;
        private Material _counterSkirtMat;
        private Material _goldTrimMat;
        private Material _ballMat;
        private Material _marqueeBulbMat;
        private Material _prizeMatCyan;
        private Material _prizeMatMagenta;
        private Material _prizeMatGold;

#if UNITY_6000_0_OR_NEWER
        private PhysicsMaterial _targetPhysicMat;
#else
        private PhysicMaterial _targetPhysicMat;
#endif

        private static bool _isBuilding;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void RuntimeAutoBootstrapBefore()
        {
            EnsureBootstrap();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void RuntimeAutoBootstrapAfter()
        {
            EnsureBootstrap();
        }

        private static void EnsureBootstrap()
        {
            if (FindAnyObjectByType<CarnivalBoothManager>() != null) return;

            GameObject root = GameObject.Find("CarnivalGameBootstrap");
            if (root == null)
            {
                root = new GameObject("CarnivalGameBootstrap");
            }

            CarnivalBoothBuilder builder = root.GetComponent<CarnivalBoothBuilder>();
            if (builder == null)
            {
                builder = root.AddComponent<CarnivalBoothBuilder>();
            }

            if (FindAnyObjectByType<CarnivalBoothManager>() == null)
            {
                builder.BuildCompleteCarnivalBooth();
            }
        }

        private void Awake()
        {
            if (autoBuildOnAwake && FindAnyObjectByType<CarnivalBoothManager>() == null)
            {
                BuildCompleteCarnivalBooth();
            }
        }

        [ContextMenu("Rebuild Carnival Booth")]
        public void BuildCompleteCarnivalBooth()
        {
            if (_isBuilding) return;

            CarnivalBoothManager existing = FindAnyObjectByType<CarnivalBoothManager>();
            if (existing != null && existing.gameObject != null)
            {
                return;
            }

            _isBuilding = true;
            try
            {
                InitializeMaterials();

                GameObject boothRoot = new GameObject("CarnivalMiniStore");
                boothRoot.transform.SetParent(transform);
                boothRoot.transform.position = Vector3.zero;

                BuildFloor(boothRoot);
                BuildWalls(boothRoot);
                BuildSupportPillars(boothRoot);
                BuildFrontCounter(boothRoot);
                BuildAwningRoof(boothRoot);
                BuildMarqueeSign(boothRoot);
                BuildTargetShelves(boothRoot);
                BuildPrizeDisplays(boothRoot);
                BuildLighting(boothRoot);

                GameObject ballPrefab = CreateBallPrefab(boothRoot);
                BallThrower thrower = SetupFirstPersonPlayer(boothRoot, ballPrefab, out FirstPersonPlayer player);

                List<ToppleTarget> staticTargets = new List<ToppleTarget>();
                GameObject staticGroup = BuildStaticTargets(boothRoot, staticTargets);

                List<MovingTargetConveyor> movingTargets = new List<MovingTargetConveyor>();
                GameObject movingGroup = BuildMovingTargets(boothRoot, movingTargets);

                CarnivalBoothManager manager = boothRoot.AddComponent<CarnivalBoothManager>();
                manager.ConfigureTargets(staticGroup, movingGroup, staticTargets, movingTargets, thrower);

                GameObject uiGO = new GameObject("CarnivalUI");
                uiGO.transform.SetParent(transform);
                CarnivalUIController uiCtrl = uiGO.AddComponent<CarnivalUIController>();
                uiCtrl.Configure(manager, player);
            }
            finally
            {
                _isBuilding = false;
            }
        }

        private Shader GetLitShader()
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (s == null) s = Shader.Find("Standard");
            if (s == null) s = Shader.Find("Mobile/Diffuse");
            if (s == null) s = Shader.Find("Sprites/Default");
            return s;
        }

        private void InitializeMaterials()
        {
            Shader lit = GetLitShader();

            Texture2D stripedTexture = GenerateStripedTexture(256, 256, new Color(0.85f, 0.12f, 0.12f), new Color(0.98f, 0.98f, 0.98f), 16);

            _stripedAwningMat = new Material(lit) { name = "M_CarnivalAwning_Striped", mainTexture = stripedTexture };
            _counterSkirtMat = new Material(lit) { name = "M_CounterSkirt_Striped", mainTexture = stripedTexture };

            _redCylinderMat = new Material(lit)
            {
                name = "M_Target_RedCylinder",
                color = new Color(0.92f, 0.12f, 0.12f)
            };
            SetSmoothness(_redCylinderMat, 0.65f);

            _woodFloorMat = new Material(lit)
            {
                name = "M_Wood_Floor",
                color = new Color(0.28f, 0.18f, 0.12f)
            };

            _woodShelfMat = new Material(lit)
            {
                name = "M_Wood_Shelf",
                color = new Color(0.42f, 0.24f, 0.15f)
            };
            SetSmoothness(_woodShelfMat, 0.45f);

            _boothWallMat = new Material(lit)
            {
                name = "M_Booth_Backdrop",
                color = new Color(0.10f, 0.14f, 0.32f)
            };

            _goldTrimMat = new Material(lit)
            {
                name = "M_Gold_Trim",
                color = new Color(1.0f, 0.82f, 0.22f)
            };
            SetSmoothness(_goldTrimMat, 0.85f);

            _ballMat = new Material(lit)
            {
                name = "M_CarnivalBall",
                color = new Color(0.96f, 0.96f, 0.96f)
            };
            SetSmoothness(_ballMat, 0.35f);
            if (_ballMat.HasProperty("_Cull")) _ballMat.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Back);
            if (_ballMat.HasProperty("_Surface")) _ballMat.SetFloat("_Surface", 0f);
            _ballMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;

            _marqueeBulbMat = new Material(lit)
            {
                name = "M_MarqueeBulb",
                color = new Color(1.0f, 0.92f, 0.55f)
            };
            _marqueeBulbMat.EnableKeyword("_EMISSION");
            _marqueeBulbMat.SetColor("_EmissionColor", new Color(1.0f, 0.9f, 0.5f) * 2f);

            _prizeMatCyan = new Material(lit) { name = "M_Prize_Cyan", color = new Color(0.15f, 0.75f, 0.88f) };
            _prizeMatMagenta = new Material(lit) { name = "M_Prize_Magenta", color = new Color(0.88f, 0.18f, 0.65f) };
            _prizeMatGold = new Material(lit) { name = "M_Prize_Gold", color = new Color(0.95f, 0.75f, 0.15f) };

#if UNITY_6000_0_OR_NEWER
            _targetPhysicMat = new PhysicsMaterial("M_TargetPhysics")
            {
                dynamicFriction = 0.28f,
                staticFriction = 0.40f,
                bounciness = 0.12f,
                frictionCombine = PhysicsMaterialCombine.Average,
                bounceCombine = PhysicsMaterialCombine.Average
            };
#else
            _targetPhysicMat = new PhysicMaterial("M_TargetPhysics")
            {
                dynamicFriction = 0.28f,
                staticFriction = 0.40f,
                bounciness = 0.12f,
                frictionCombine = PhysicsMaterialCombine.Average,
                bounceCombine = PhysicsMaterialCombine.Average
            };
#endif
        }

        private void SetSmoothness(Material mat, float val)
        {
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", val);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", val);
        }

        private Texture2D GenerateStripedTexture(int width, int height, Color colA, Color colB, int stripeWidth)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isA = ((x / stripeWidth) % 2) == 0;
                    pixels[y * width + x] = isA ? colA : colB;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private void BuildFloor(GameObject parent)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Store_Floor";
            floor.transform.SetParent(parent.transform);
            floor.transform.position = new Vector3(0f, -0.05f, 0.5f);
            floor.transform.localScale = new Vector3(5.6f, 0.1f, 8.0f);
            floor.GetComponent<Renderer>().sharedMaterial = _woodFloorMat;
        }

        private void BuildWalls(GameObject parent)
        {
            GameObject wallsRoot = new GameObject("Store_Walls");
            wallsRoot.transform.SetParent(parent.transform);

            GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backWall.name = "BackWall";
            backWall.transform.SetParent(wallsRoot.transform);
            backWall.transform.position = new Vector3(0f, 1.8f, 3.4f);
            backWall.transform.localScale = new Vector3(5.2f, 3.6f, 0.15f);
            backWall.GetComponent<Renderer>().sharedMaterial = _boothWallMat;

            GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall.name = "LeftWall";
            leftWall.transform.SetParent(wallsRoot.transform);
            leftWall.transform.position = new Vector3(-2.55f, 1.8f, 1.2f);
            leftWall.transform.localScale = new Vector3(0.15f, 3.6f, 4.6f);
            leftWall.GetComponent<Renderer>().sharedMaterial = _boothWallMat;

            GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall.name = "RightWall";
            rightWall.transform.SetParent(wallsRoot.transform);
            rightWall.transform.position = new Vector3(2.55f, 1.8f, 1.2f);
            rightWall.transform.localScale = new Vector3(0.15f, 3.6f, 4.6f);
            rightWall.GetComponent<Renderer>().sharedMaterial = _boothWallMat;

            CreateTrim(wallsRoot, new Vector3(0f, 3.55f, 3.32f), new Vector3(5.2f, 0.12f, 0.08f));
            CreateTrim(wallsRoot, new Vector3(0f, 0.1f, 3.32f), new Vector3(5.2f, 0.12f, 0.08f));
        }

        private void CreateTrim(GameObject parent, Vector3 pos, Vector3 scale)
        {
            GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = "GoldTrim";
            trim.transform.SetParent(parent.transform);
            trim.transform.position = pos;
            trim.transform.localScale = scale;
            trim.GetComponent<Renderer>().sharedMaterial = _goldTrimMat;
        }

        private void BuildSupportPillars(GameObject parent)
        {
            GameObject pillarsRoot = new GameObject("Store_Pillars");
            pillarsRoot.transform.SetParent(parent.transform);

            Vector3[] pillarPositions =
            {
                new Vector3(-2.45f, 1.8f, -1.0f),
                new Vector3( 2.45f, 1.8f, -1.0f),
                new Vector3(-2.45f, 1.8f,  3.3f),
                new Vector3( 2.45f, 1.8f,  3.3f)
            };

            for (int i = 0; i < pillarPositions.Length; i++)
            {
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = $"Pillar_{i + 1}";
                pillar.transform.SetParent(pillarsRoot.transform);
                pillar.transform.position = pillarPositions[i];
                pillar.transform.localScale = new Vector3(0.24f, 1.8f, 0.24f);
                pillar.GetComponent<Renderer>().sharedMaterial = _woodShelfMat;

                GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cap.name = "PillarCap";
                cap.transform.SetParent(pillar.transform);
                cap.transform.localPosition = new Vector3(0f, 0.98f, 0f);
                cap.transform.localScale = new Vector3(1.25f, 0.06f, 1.25f);
                cap.GetComponent<Renderer>().sharedMaterial = _goldTrimMat;

                GameObject baseRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                baseRing.name = "PillarBase";
                baseRing.transform.SetParent(pillar.transform);
                baseRing.transform.localPosition = new Vector3(0f, -0.98f, 0f);
                baseRing.transform.localScale = new Vector3(1.25f, 0.06f, 1.25f);
                baseRing.GetComponent<Renderer>().sharedMaterial = _goldTrimMat;
            }
        }

        private void BuildFrontCounter(GameObject parent)
        {
            GameObject counterRoot = new GameObject("Store_FrontCounter");
            counterRoot.transform.SetParent(parent.transform);

            GameObject counterTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counterTop.name = "CounterTop";
            counterTop.transform.SetParent(counterRoot.transform);
            counterTop.transform.position = new Vector3(0f, 0.88f, -1.0f);
            counterTop.transform.localScale = new Vector3(5.1f, 0.12f, 0.75f);
            counterTop.GetComponent<Renderer>().sharedMaterial = _woodShelfMat;

            GameObject skirt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            skirt.name = "CounterSkirt";
            skirt.transform.SetParent(counterRoot.transform);
            skirt.transform.position = new Vector3(0f, 0.42f, -1.35f);
            skirt.transform.localScale = new Vector3(5.1f, 0.82f, 0.06f);
            skirt.GetComponent<Renderer>().sharedMaterial = _counterSkirtMat;

            GameObject ballCrate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ballCrate.name = "BallTray";
            ballCrate.transform.SetParent(counterRoot.transform);
            ballCrate.transform.position = new Vector3(1.4f, 0.98f, -0.9f);
            ballCrate.transform.localScale = new Vector3(0.6f, 0.08f, 0.4f);
            ballCrate.GetComponent<Renderer>().sharedMaterial = _woodShelfMat;

            for (int b = -1; b <= 1; b++)
            {
                GameObject restingBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                restingBall.name = "DisplayBall";
                restingBall.transform.SetParent(ballCrate.transform);
                restingBall.transform.localPosition = new Vector3(b * 0.3f, 1.2f, 0f);
                restingBall.transform.localScale = new Vector3(0.28f, 2.1f, 0.42f);
                restingBall.GetComponent<Renderer>().sharedMaterial = _ballMat;
                DestroyImmediate(restingBall.GetComponent<Collider>());
            }
        }

        private void BuildAwningRoof(GameObject parent)
        {
            GameObject awningRoot = new GameObject("Store_AwningCanopy");
            awningRoot.transform.SetParent(parent.transform);

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "SlantedRoof";
            roof.transform.SetParent(awningRoot.transform);
            roof.transform.position = new Vector3(0f, 3.42f, 1.15f);
            roof.transform.rotation = Quaternion.Euler(6.5f, 0f, 0f);
            roof.transform.localScale = new Vector3(5.3f, 0.08f, 4.9f);
            roof.GetComponent<Renderer>().sharedMaterial = _stripedAwningMat;

            GameObject valance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            valance.name = "AwningValance";
            valance.transform.SetParent(awningRoot.transform);
            valance.transform.position = new Vector3(0f, 3.02f, -1.28f);
            valance.transform.localScale = new Vector3(5.3f, 0.36f, 0.05f);
            valance.GetComponent<Renderer>().sharedMaterial = _stripedAwningMat;

            GameObject fringe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fringe.name = "ValanceFringe";
            fringe.transform.SetParent(valance.transform);
            fringe.transform.localPosition = new Vector3(0f, -0.48f, 0f);
            fringe.transform.localScale = new Vector3(1f, 0.12f, 1.2f);
            fringe.GetComponent<Renderer>().sharedMaterial = _goldTrimMat;
        }

        private void BuildMarqueeSign(GameObject parent)
        {
            GameObject signRoot = new GameObject("Store_MarqueeSign");
            signRoot.transform.SetParent(parent.transform);

            GameObject plaque = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plaque.name = "SignPlaque";
            plaque.transform.SetParent(signRoot.transform);
            plaque.transform.position = new Vector3(0f, 3.82f, -1.05f);
            plaque.transform.localScale = new Vector3(3.6f, 0.72f, 0.12f);
            plaque.GetComponent<Renderer>().sharedMaterial = _boothWallMat;

            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "SignGoldFrame";
            frame.transform.SetParent(plaque.transform);
            frame.transform.localPosition = new Vector3(0f, 0f, 0.08f);
            frame.transform.localScale = new Vector3(1.04f, 1.08f, 0.5f);
            frame.GetComponent<Renderer>().sharedMaterial = _goldTrimMat;

            int bulbsX = 8;
            for (int i = 0; i < bulbsX; i++)
            {
                float t = (float)i / (bulbsX - 1);
                float x = Mathf.Lerp(-1.65f, 1.65f, t);

                CreateBulb(plaque, new Vector3(x, 0.32f, 0.12f));
                CreateBulb(plaque, new Vector3(x, -0.32f, 0.12f));
            }
        }

        private void CreateBulb(GameObject parent, Vector3 localPos)
        {
            GameObject bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulb.name = "MarqueeBulb";
            bulb.transform.SetParent(parent.transform);
            bulb.transform.localPosition = localPos;
            bulb.transform.localScale = new Vector3(0.06f, 0.18f, 0.4f);
            bulb.GetComponent<Renderer>().sharedMaterial = _marqueeBulbMat;
            DestroyImmediate(bulb.GetComponent<Collider>());
        }

        private void BuildTargetShelves(GameObject parent)
        {
            GameObject shelvesRoot = new GameObject("Store_TargetShelves");
            shelvesRoot.transform.SetParent(parent.transform);

            GameObject lowerShelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lowerShelf.name = "LowerShelf";
            lowerShelf.transform.SetParent(shelvesRoot.transform);
            lowerShelf.transform.position = new Vector3(0f, 0.80f, 2.45f);
            lowerShelf.transform.localScale = new Vector3(4.4f, 0.08f, 0.55f);
            lowerShelf.GetComponent<Renderer>().sharedMaterial = _woodShelfMat;
            if (lowerShelf.TryGetComponent<Collider>(out var lowerCol)) lowerCol.material = _targetPhysicMat;

            GameObject midShelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            midShelf.name = "MiddleShelf";
            midShelf.transform.SetParent(shelvesRoot.transform);
            midShelf.transform.position = new Vector3(0f, 1.65f, 2.45f);
            midShelf.transform.localScale = new Vector3(4.4f, 0.08f, 0.55f);
            midShelf.GetComponent<Renderer>().sharedMaterial = _woodShelfMat;
            if (midShelf.TryGetComponent<Collider>(out var midCol)) midCol.material = _targetPhysicMat;

            GameObject upperLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            upperLeft.name = "UpperShelf_Left";
            upperLeft.transform.SetParent(shelvesRoot.transform);
            upperLeft.transform.position = new Vector3(-1.65f, 2.75f, 2.45f);
            upperLeft.transform.localScale = new Vector3(1.1f, 0.08f, 0.55f);
            upperLeft.GetComponent<Renderer>().sharedMaterial = _woodShelfMat;
            if (upperLeft.TryGetComponent<Collider>(out var ulCol)) ulCol.material = _targetPhysicMat;

            GameObject upperRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            upperRight.name = "UpperShelf_Right";
            upperRight.transform.SetParent(shelvesRoot.transform);
            upperRight.transform.position = new Vector3(1.65f, 2.75f, 2.45f);
            upperRight.transform.localScale = new Vector3(1.1f, 0.08f, 0.55f);
            upperRight.GetComponent<Renderer>().sharedMaterial = _woodShelfMat;
            if (upperRight.TryGetComponent<Collider>(out var urCol)) urCol.material = _targetPhysicMat;
        }

        private void BuildPrizeDisplays(GameObject parent)
        {
            GameObject prizesRoot = new GameObject("Store_PrizeDisplays");
            prizesRoot.transform.SetParent(parent.transform);

            CreatePrizeStack(prizesRoot, new Vector3(-2.2f, 1.3f, 1.8f));
            CreatePrizeStack(prizesRoot, new Vector3(2.2f, 1.3f, 1.8f));
        }

        private void CreatePrizeStack(GameObject parent, Vector3 origin)
        {
            GameObject bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bracket.name = "PrizeBracket";
            bracket.transform.SetParent(parent.transform);
            bracket.transform.position = origin;
            bracket.transform.localScale = new Vector3(0.5f, 0.06f, 1.0f);
            bracket.GetComponent<Renderer>().sharedMaterial = _woodShelfMat;

            Material[] mats = { _prizeMatCyan, _prizeMatMagenta, _prizeMatGold };
            for (int i = 0; i < 3; i++)
            {
                GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                box.name = $"PrizeBox_{i + 1}";
                box.transform.SetParent(bracket.transform);
                box.transform.localPosition = new Vector3(0f, 2.8f + (i * 2.2f), (i - 1) * 0.3f);
                box.transform.localScale = new Vector3(0.65f, 4.5f, 0.3f);
                box.GetComponent<Renderer>().sharedMaterial = mats[i % mats.Length];
                DestroyImmediate(box.GetComponent<Collider>());
            }
        }

        private void BuildLighting(GameObject parent)
        {
            GameObject lightingRoot = new GameObject("Store_Lighting");
            lightingRoot.transform.SetParent(parent.transform);

            GameObject lightLeft = new GameObject("InteriorLight_Left");
            lightLeft.transform.SetParent(lightingRoot.transform);
            lightLeft.transform.position = new Vector3(-1.3f, 2.9f, 0.8f);
            Light l1 = lightLeft.AddComponent<Light>();
            l1.type = LightType.Point;
            l1.color = new Color(1.0f, 0.92f, 0.72f);
            l1.intensity = 1.8f;
            l1.range = 6.5f;

            GameObject lightRight = new GameObject("InteriorLight_Right");
            lightRight.transform.SetParent(lightingRoot.transform);
            lightRight.transform.position = new Vector3(1.3f, 2.9f, 0.8f);
            Light l2 = lightRight.AddComponent<Light>();
            l2.type = LightType.Point;
            l2.color = new Color(1.0f, 0.92f, 0.72f);
            l2.intensity = 1.8f;
            l2.range = 6.5f;
        }

        private GameObject CreateRedCylinder(GameObject parent, string name, Vector3 pos)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent.transform);
            cylinder.transform.position = pos;
            cylinder.transform.localScale = new Vector3(0.18f, 0.13f, 0.18f);

            Collider capCol = cylinder.GetComponent<Collider>();
            if (capCol != null) DestroyImmediate(capCol);

            BoxCollider boxCol = cylinder.AddComponent<BoxCollider>();
            boxCol.center = Vector3.zero;
            boxCol.size = new Vector3(0.96f, 2.0f, 0.96f);
            boxCol.material = _targetPhysicMat;

            cylinder.GetComponent<Renderer>().sharedMaterial = _redCylinderMat;

            Rigidbody rb = cylinder.AddComponent<Rigidbody>();
            rb.mass = 0.26f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.centerOfMass = Vector3.zero;
            rb.solverIterations = 16;
            rb.solverVelocityIterations = 12;

#if UNITY_6000_0_OR_NEWER
            rb.linearDamping = 0.10f;
            rb.angularDamping = 0.15f;
#else
            rb.drag = 0.10f;
            rb.angularDrag = 0.15f;
#endif

            cylinder.AddComponent<ToppleTarget>();
            return cylinder;
        }

        private GameObject BuildStaticTargets(GameObject parent, List<ToppleTarget> outTargets)
        {
            GameObject root = new GameObject("StaticTargets_Group");
            root.transform.SetParent(parent.transform);

            float d = 0.18f;
            float h = 0.26f;
            float halfH = h * 0.5f;
            float epsilon = 0.008f;
            float shelfZ = 2.45f;

            float midShelfSurfaceY = 1.69f;
            float row1Y = midShelfSurfaceY + halfH + epsilon;
            GameObject c1 = CreateRedCylinder(root, "StaticCylinder_Mid_1", new Vector3(-d, row1Y, shelfZ));
            GameObject c2 = CreateRedCylinder(root, "StaticCylinder_Mid_2", new Vector3(0f, row1Y, shelfZ));
            GameObject c3 = CreateRedCylinder(root, "StaticCylinder_Mid_3", new Vector3( d, row1Y, shelfZ));

            float row2Y = row1Y + h + 0.003f;
            GameObject c4 = CreateRedCylinder(root, "StaticCylinder_Mid_4", new Vector3(-d * 0.5f, row2Y, shelfZ));
            GameObject c5 = CreateRedCylinder(root, "StaticCylinder_Mid_5", new Vector3( d * 0.5f, row2Y, shelfZ));

            float peakY = row2Y + h + 0.003f;
            GameObject c6 = CreateRedCylinder(root, "StaticCylinder_Mid_Peak", new Vector3(0f, peakY, shelfZ));

            float lowerShelfSurfaceY = 0.84f;
            float lowerY = lowerShelfSurfaceY + halfH + epsilon;
            GameObject c7 = CreateRedCylinder(root, "StaticCylinder_Lower_L", new Vector3(-1.35f, lowerY, shelfZ));
            GameObject c8 = CreateRedCylinder(root, "StaticCylinder_Lower_R", new Vector3( 1.35f, lowerY, shelfZ));

            float upperShelfSurfaceY = 2.79f;
            float upperY = upperShelfSurfaceY + halfH + epsilon;
            GameObject c9 = CreateRedCylinder(root, "StaticCylinder_Upper_L", new Vector3(-1.65f, upperY, shelfZ));
            GameObject c10 = CreateRedCylinder(root, "StaticCylinder_Upper_R", new Vector3( 1.65f, upperY, shelfZ));

            GameObject[] allCylinders = { c1, c2, c3, c4, c5, c6, c7, c8, c9, c10 };
            for (int i = 0; i < allCylinders.Length; i++)
            {
                Rigidbody rb = allCylinders[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.Sleep();
                }
                outTargets.Add(allCylinders[i].GetComponent<ToppleTarget>());
            }

            root.SetActive(false);
            return root;
        }

        private GameObject BuildMovingTargets(GameObject parent, List<MovingTargetConveyor> outTargets)
        {
            GameObject root = new GameObject("MovingTargets_Group");
            root.transform.SetParent(parent.transform);

            float shelfZ = 2.45f;
            float h = 0.26f;
            float halfH = h * 0.5f;
            float epsilon = 0.008f;

            float lowerY = 0.84f + halfH + epsilon;
            float[] lowerX = { -1.3f, 0.0f, 1.3f };

            for (int i = 0; i < lowerX.Length; i++)
            {
                GameObject target = CreateRedCylinder(root, $"MovingCylinder_Lower_{i + 1}", new Vector3(lowerX[i], lowerY, shelfZ));
                MovingTargetConveyor conveyor = target.AddComponent<MovingTargetConveyor>();
                conveyor.Speed = 1.4f;
                conveyor.LeftBoundX = -1.9f;
                conveyor.RightBoundX = 1.9f;
                outTargets.Add(conveyor);
            }

            float middleY = 1.69f + halfH + epsilon;
            float[] middleX = { -1.1f, 0.2f, 1.5f };

            for (int i = 0; i < middleX.Length; i++)
            {
                GameObject target = CreateRedCylinder(root, $"MovingCylinder_Middle_{i + 1}", new Vector3(middleX[i], middleY, shelfZ));
                MovingTargetConveyor conveyor = target.AddComponent<MovingTargetConveyor>();
                conveyor.Speed = 1.8f;
                conveyor.LeftBoundX = -1.9f;
                conveyor.RightBoundX = 1.9f;
                outTargets.Add(conveyor);
            }

            root.SetActive(false);
            return root;
        }

        private GameObject CreateBallPrefab(GameObject parent)
        {
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "CarnivalBall_Prefab";
            ball.transform.SetParent(parent.transform);
            ball.transform.position = new Vector3(0f, -100f, 0f);
            ball.transform.localScale = Vector3.one * 0.22f;

            ball.GetComponent<Renderer>().sharedMaterial = _ballMat;

            Rigidbody rb = ball.AddComponent<Rigidbody>();
            rb.mass = 0.9f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            ball.AddComponent<CarnivalBall>();
            ball.SetActive(false);
            return ball;
        }

        private BallThrower SetupFirstPersonPlayer(GameObject parent, GameObject ballPrefab, out FirstPersonPlayer outPlayer)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camGO = new GameObject("Main Camera");
                cam = camGO.AddComponent<Camera>();
                camGO.tag = "MainCamera";
                camGO.AddComponent<AudioListener>();
            }

            GameObject playerGO = new GameObject("Player_FPS");
            playerGO.transform.SetParent(parent.transform);
            playerGO.transform.position = new Vector3(0f, 0.05f, -2.4f);
            playerGO.transform.rotation = Quaternion.identity;

            GameObject headGO = new GameObject("Head");
            headGO.transform.SetParent(playerGO.transform);
            headGO.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            headGO.transform.localRotation = Quaternion.identity;

            cam.transform.SetParent(headGO.transform);
            cam.transform.localPosition = Vector3.zero;
            cam.transform.localRotation = Quaternion.identity;
            cam.nearClipPlane = 0.01f;
            cam.fieldOfView = 60f;

            FirstPersonPlayer fps = playerGO.AddComponent<FirstPersonPlayer>();
            fps.Initialize(headGO.transform, cam);
            outPlayer = fps;

            BallThrower thrower = playerGO.AddComponent<BallThrower>();

            GameObject throwOriginGO = new GameObject("ThrowOrigin");
            throwOriginGO.transform.SetParent(headGO.transform);
            throwOriginGO.transform.localPosition = new Vector3(0.24f, -0.16f, 0.46f);

            GameObject heldBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            heldBall.name = "HeldBallVisual";
            heldBall.transform.SetParent(throwOriginGO.transform);
            heldBall.transform.localPosition = Vector3.zero;
            heldBall.transform.localScale = Vector3.one * 0.16f;
            heldBall.GetComponent<Renderer>().sharedMaterial = _ballMat;
            DestroyImmediate(heldBall.GetComponent<Collider>());

            thrower.Initialize(ballPrefab, throwOriginGO.transform, heldBall, cam, 8);

            return thrower;
        }
    }
}
