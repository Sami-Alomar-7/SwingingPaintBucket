using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using SwingingPaintBucket.Features.Pendulum.Components;
using SwingingPaintBucket.Features.Pendulum.Services;
using SwingingPaintBucket.Features.Pendulum.Data;
using SwingingPaintBucket.Features.Rope.Components;
using SwingingPaintBucket.Features.Rope.Data;
using SwingingPaintBucket.Features.Pendulum.Interfaces;
using SwingingPaintBucket.Features.ExternalForces.Interfaces;
using SwingingPaintBucket.Features.ExternalForces.Services;
using SwingingPaintBucket.Features.Paint.Components;
using SwingingPaintBucket.Features.Surface.Components;
using SwingingPaintBucket.Features.Paint.Services;
using SwingingPaintBucket.Features.Surface.Services;
using UnityEngine.InputSystem.UI;

namespace SwingingPaintBucket.Editor
{

    public class SceneBuilder
    {
        [MenuItem("SwingingPaintBucket/Build Main Scene")]
        public static void BuildScene()
        {
            Debug.Log("[SceneBuilder] Starting scene build...");
            
            // Create new scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // ─── Camera ───────────────────────────────────────────────────────────────
            GameObject cameraGO = new GameObject("Main Camera");
            Camera camera = cameraGO.AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 5f, -15f); 
            camera.transform.rotation = Quaternion.Euler(25f, 0f, 0f); 
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.11f, 1f);
            camera.farClipPlane = 100f;
            cameraGO.AddComponent<AudioListener>();

            // ─── Light ─────────────────────────────────────────────────────────────────
            GameObject lightGO = new GameObject("Directional Light");
            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            
             // ─── Ground (Simple Plane) ─────────────────────────────────────────────────
             GameObject groundGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
             groundGO.name = "Ground";
             groundGO.transform.position = new Vector3(0f, -2f, 0f);
             groundGO.transform.localScale = new Vector3(4f, 1f, 4f);
             Renderer groundRenderer = groundGO.GetComponent<Renderer>();
             Material groundMaterial = new Material(groundRenderer.sharedMaterial);
             groundMaterial.color = Color.white;
             groundRenderer.material = groundMaterial;
             
             // Add paint surface system
             PaintSurfaceSystem surfaceSystem = groundGO.AddComponent<PaintSurfaceSystem>();
             surfaceSystem.textureSize = 512;
             surfaceSystem.baseColor = Color.white;
            
             // ─── Pendulum System Container ─────────────────────────────────────────────
             GameObject pendulumSystemGO = new GameObject("PaintBucketSystem");
             pendulumSystemGO.transform.position = Vector3.zero;

             // ─── Mass System ───────────────────────────────────────────────────────────
             MassSystem massSystem = new MassSystem { BaseMass = 1f };

             // ─── Pivot (Hanging Point) ───────────────────────────────────────────────────
             GameObject pivotGO = new GameObject("Pivot");
             pivotGO.transform.SetParent(pendulumSystemGO.transform, false);
             pivotGO.transform.position = new Vector3(0f, 5f, 0f); 

            // Add RopeRenderer
            RopeRenderer ropeRenderer = pivotGO.AddComponent<RopeRenderer>();

            // ─── Bucket (Sphere) ─────────────────────────────────────────────────────
            GameObject bucketGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bucketGO.name = "Bucket";
            bucketGO.transform.SetParent(pendulumSystemGO.transform, false);
            bucketGO.transform.position = new Vector3(0f, 0f, 0f); 
            bucketGO.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            Renderer bucketRenderer = bucketGO.GetComponent<Renderer>();
            Material bucketMaterial = new Material(bucketRenderer.sharedMaterial);
            bucketMaterial.color = new Color(0.15f, 0.15f, 0.18f, 1f);
            bucketRenderer.material = bucketMaterial;
            
            if (bucketGO.GetComponent<Collider>() != null)
            {
                Object.DestroyImmediate(bucketGO.GetComponent<Collider>());
            }
            
             // ─── Paint Emitter (on bucket) ────────────────────────────────────────────
             GameObject paintEmitterGO = new GameObject("PaintEmitter");
             paintEmitterGO.transform.SetParent(bucketGO.transform, false);
             paintEmitterGO.transform.localPosition = new Vector3(0f, -0.2f, 0f);
             
             ParticleRenderer particleRenderer = paintEmitterGO.AddComponent<ParticleRenderer>();
             
             PaintEmitter paintEmitter = paintEmitterGO.AddComponent<PaintEmitter>();
             paintEmitter.spawnPoint = paintEmitterGO.transform;
             paintEmitter.bucket = bucketGO.transform;
             paintEmitter.surfaceSurface = groundGO.transform;
             paintEmitter.paintSurfaceSystem = surfaceSystem;
             paintEmitter.particleRenderer = particleRenderer;
             
             paintEmitter.emissionService = new DynamicPaintEmissionService();
             paintEmitter.particlePhysicsService = new ParticlePhysicsService();
             paintEmitter.massLossNotifier = massSystem;
            
             // ─── Pendulum Controller ───────────────────────────────────────────────────
             PendulumController pendulumController = pendulumSystemGO.AddComponent<PendulumController>();
             pendulumController.PivotY = 5f; 
             pendulumController.BucketTransform = bucketGO.transform;
             pendulumController.Rope = ropeRenderer;
             pendulumController.PaintEmitter = paintEmitter;
            
            // ─── UI Canvas ─────────────────────────────────────────────────────────────
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Ensure UI renders on top of all 3D scene elements

            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer != -1)
            {
                canvasGO.layer = uiLayer;
            }
            else
            {
                // Fallback: Use Default layer if UI layer doesn't exist
                Debug.LogWarning("[SceneBuilder] UI layer not found, using Default layer. Consider creating a 'UI' layer in project settings.");
                canvasGO.layer = 0; // Default layer
            }

            CanvasScaler canvasScaler = canvasGO.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(800, 600);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Event System with fallback for different input system configurations
            GameObject eventSystemGO = new GameObject("EventSystem");
            var eventSystem = eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            
            // Try to add new InputSystemUIInputModule first, fallback to StandaloneInputModule if not available
            try
            {
                eventSystemGO.AddComponent<InputSystemUIInputModule>();
            }
            catch (System.Exception)
            {
                // Fallback to legacy input system module
                Debug.LogWarning("[SceneBuilder] InputSystemUIInputModule not available, using StandaloneInputModule");
                eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            
            // ─── UI Panel (Side Dashboard Panel) ─────────────────────────────────────────────
            GameObject panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            RectTransform panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 0f);      
            panelRect.anchorMax = new Vector2(1f, 1f);      
            panelRect.pivot = new Vector2(1f, 1f);          
            panelRect.sizeDelta = new Vector2(320f, 0f);    
            panelRect.anchoredPosition = new Vector2(0f, 0f); 
            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0.11f, 0.12f, 0.15f, 0.95f); 
            
            // ─── Panel Title Header ────────────────────────────────────────────────────
            GameObject titleGO = new GameObject("PanelTitle");
            titleGO.transform.SetParent(panelGO.transform, false);
            RectTransform titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(280f, 32f);
            titleRect.anchoredPosition = new Vector2(0f, -15f);

            // Purple branding accent update for the dashboard identity context
            TextMeshProUGUI titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "SYSTEM CONFIG";
            titleTMP.fontSize = 16;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = new Color(0.68f, 0.27f, 1f, 1f); // Beautiful Purple Hex #ad46ff branding update

            // ─── UI Input Fields Sequential Top-Down Layout ─────────────────────────────
            float inputFieldWidth = 260f;
            float currentY = -55f;        // Dynamic starting location tracking top anchor axis
            float spacingDelta = 48f;     // Calculated block layout offset preventing overlapping fields

            TMP_InputField lengthInput = CreateInputField(panelGO, "Rope Length (m)", currentY, inputFieldWidth, "3");
            currentY -= spacingDelta;
            
            TMP_InputField gravityInput = CreateInputField(panelGO, "Gravitational Field (m/s²)", currentY, inputFieldWidth, "9.81");
            currentY -= spacingDelta;
            
            TMP_InputField angleInput = CreateInputField(panelGO, "Initial Angle (deg)", currentY, inputFieldWidth, "30");
            currentY -= spacingDelta;
            
            TMP_InputField baseMassInput = CreateInputField(panelGO, "Dry Bucket Mass (kg)", currentY, inputFieldWidth, "1");
            currentY -= spacingDelta;
            
            TMP_InputField paintMassInput = CreateInputField(panelGO, "Initial Paint Load (kg)", currentY, inputFieldWidth, "0.5");
            currentY -= spacingDelta;

            TMP_InputField holeSizeInput = CreateInputField(panelGO, "Bucket Hole Diameter (m)", currentY, inputFieldWidth, "0.01");
            currentY -= spacingDelta;

             // ─── Rope Type Text Input ─────────────────────────────────────────────────────
             TMP_InputField ropeTypeInput = CreateInputField(panelGO, "Rope Type (Rigid, Nylon, Bungee)", currentY, inputFieldWidth, "Rigid", TMP_InputField.ContentType.Standard);
             currentY -= (spacingDelta + 8f); // Extra spacing margin for structural breathing room

             // ─── Action Buttons (Horizontal Layout Grid Coordination) ───────────────────
             float singleButtonWidth = 115f;
             float buttonHeight = 36f;
             
             // Placed safely down the layout stack at the bottom of the form context
             Button startButton = CreateButton(panelGO, "Start Simulation", -70f, currentY, singleButtonWidth, buttonHeight);
             Button resetButton = CreateButton(panelGO, "Reset System", 70f, currentY, singleButtonWidth, buttonHeight);
            
             // ─── Wire Up Pendulum Controller References ───────────────────────────────
             pendulumController.StartButton = startButton;
             pendulumController.ResetButton = resetButton;

            // ─── ربط الأزرار برمجياً بالدوال لتفعيلها عند الضغط ───────────────────
            // ربط زر بدء المحاكاة بالدالة المسؤولة عن تشغيل السكربت أو تفعيل flag الحركة
            startButton.onClick.AddListener(() =>
            {
                // هنا نخبر الكنترولر أن يبدأ تشغيل الفيزياء والحركة
                // تأكدي أن سكربت PendulumController يحتوي على دالة للبدء، غالباً تسمى StartSimulation أو ما يشابهها
                pendulumController.enabled = true;
                Debug.Log("[Runtime] Start Simulation Button Clicked!");
            });

            // ربط زر إعادة الضبط لإعادة تشغيل المشهد من جديد
            resetButton.onClick.AddListener(() =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
                );
            });

            PendulumInputHandler inputHandler = new PendulumInputHandler(
                lengthInput,
                gravityInput,
                null,
                angleInput,
                null,
                baseMassInput,
                paintMassInput
            );
            inputHandler.ropeTypeInput = ropeTypeInput;
            pendulumController.InputHandler = inputHandler;

            // Wire rope type input to reset simulation on change (prevents physics instability)
            ropeTypeInput.onEndEdit.AddListener((text) =>
            {
                if (pendulumController.IsRunning())
                {
                    pendulumController.ResetSimulation();
                }
            });

            // Wire hole size input to update paint emitter config
            holeSizeInput.onEndEdit.AddListener((text) =>
            {
                if (float.TryParse(text, out float holeDiameter))
                {
                    paintEmitter.emissionConfig.holeDiameter = Mathf.Max(0.001f, holeDiameter); // Minimum 1mm
                }
            });
            
            EditorUtility.SetDirty(pendulumController);
            pendulumController.PhysicsEngine = new PendulumPhysicsService();
            pendulumController.MassProvider = massSystem;
            pendulumController.ForceProviders = new List<IForceProvider>
            {
                new GravityForce(massSystem),
                new DragForce()
            };



            EditorUtility.SetDirty(pendulumController);
            EditorUtility.SetDirty(paintEmitter);
            
            // Save current constructed runtime scene
            string directoryPath = "Assets/Scenes/Main";
            if (!System.IO.Directory.Exists(directoryPath))
            {
                System.IO.Directory.CreateDirectory(directoryPath);
            }
            string scenePath = $"{directoryPath}/SwingingPaintBucketMain.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            Debug.Log($"[SceneBuilder] Scene rebuilt and successfully saved to: {scenePath}");
            
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        }
        
        private static TMP_InputField CreateInputField(GameObject parent, string fieldLabelText, float yOffset, float width, string defaultVal, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.DecimalNumber)
        {
            // Create Label text above the input container
            GameObject labelGO = new GameObject($"Label_{fieldLabelText}");
            labelGO.transform.SetParent(parent.transform, false);
            RectTransform labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 1f);
            labelRect.anchorMax = new Vector2(0.5f, 1f);
            labelRect.pivot = new Vector2(0.5f, 1f);
            labelRect.sizeDelta = new Vector2(width, 16f);
            labelRect.anchoredPosition = new Vector2(0f, yOffset);

            TextMeshProUGUI labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.text = fieldLabelText;
            labelTMP.fontSize = 10;
            labelTMP.fontStyle = FontStyles.Bold;
            labelTMP.color = new Color(0.7f, 0.75f, 0.8f, 1f);
            labelTMP.alignment = TextAlignmentOptions.Left;

            // 1. Create Main Input Field Root
            GameObject inputRoot = new GameObject($"InputField_{fieldLabelText}");
            inputRoot.transform.SetParent(parent.transform, false);

            RectTransform rootRect = inputRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 1f);
            rootRect.anchorMax = new Vector2(0.5f, 1f);
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.sizeDelta = new Vector2(width, 26f);
            rootRect.anchoredPosition = new Vector2(0f, yOffset - 18f); // Spaced below text header label

            Image bgImage = inputRoot.AddComponent<Image>();
            bgImage.color = new Color(0.16f, 0.17f, 0.20f, 1f); 
            bgImage.type = Image.Type.Sliced;

            TMP_InputField inputField = inputRoot.AddComponent<TMP_InputField>();

            // 2. Create Text Area Viewport
            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputRoot.transform, false);
            RectTransform areaRect = textArea.AddComponent<RectTransform>();
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.sizeDelta = new Vector2(-12f, -6f); 
            textArea.AddComponent<RectMask2D>();

            // 3. Create Placeholder Text Component
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textArea.transform, false);
            RectTransform placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.sizeDelta = Vector2.zero;

            // Simple composition structure placeholder fallback layout
            TextMeshProUGUI placeholderTMP = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderTMP.text = "Enter value...";
            placeholderTMP.fontSize = 14;
            placeholderTMP.fontStyle = FontStyles.Italic;
            placeholderTMP.color = new Color(0.4f, 0.45f, 0.5f, 1f);
            placeholderTMP.alignment = TextAlignmentOptions.Left;
            placeholderTMP.raycastTarget = false;

            // 4. Create Main Display Text Component
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(textArea.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            // Black shirt style casual flat colors pairing background
            TextMeshProUGUI textTMP = textObj.AddComponent<TextMeshProUGUI>();
            textTMP.text = defaultVal;
            textTMP.fontSize = 14;
            textTMP.color = Color.white;
            textTMP.alignment = TextAlignmentOptions.Left;
            textTMP.raycastTarget = false;

            // 5. Wire references up to the InputField component to prevent NullReference
            inputField.textViewport = areaRect;
            inputField.textComponent = textTMP;
            inputField.placeholder = placeholderTMP;

            inputField.contentType = contentType;
            inputField.text = defaultVal;

            return inputField;
        }
        
        private static Button CreateButton(GameObject parent, string label, float xOffset, float yOffset, float width, float height)
        {
            GameObject btnRoot = new GameObject($"{label}_Button");
            btnRoot.transform.SetParent(parent.transform, false);
            
            RectTransform rect = btnRoot.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f); 
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(xOffset, yOffset);
            
            Image btnImg = btnRoot.AddComponent<Image>();
            btnImg.color = new Color(0.68f, 0.27f, 1f, 1f); // Clean Purple Hex Theme #ad46ff matching branding criteria
            
            Button btn = btnRoot.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(0.68f, 0.27f, 1f, 1f);
            cb.highlightedColor = new Color(0.75f, 0.4f, 1f, 1f); 
            cb.pressedColor = new Color(0.55f, 0.15f, 0.85f, 1f);     
            btn.colors = cb;
            
            GameObject txtObj = new GameObject("BtnText");
            txtObj.transform.SetParent(btnRoot.transform, false);
            RectTransform txtRect = txtObj.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI btnTMP = txtObj.AddComponent<TextMeshProUGUI>();
            btnTMP.text = label;
            btnTMP.fontSize = 13; 
            btnTMP.fontStyle = FontStyles.Bold;
            btnTMP.alignment = TextAlignmentOptions.Center;
            btnTMP.color = Color.white;
            btnTMP.raycastTarget = false; 
            
            return btn;
        }
    }
}