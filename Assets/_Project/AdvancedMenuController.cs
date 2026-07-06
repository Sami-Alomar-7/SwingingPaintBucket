using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using SwingingPaintBucket.Features.Surface.Components;
using SwingingPaintBucket.Features.Paint.Components;

public class AdvancedMenuController : MonoBehaviour
{
    [Header("System Configration")]
    public GameObject mainSettingsButton;
    public GameObject mainSettingsMenuPanel;

    [Header("Contents")]
    public GameObject ropeContent;
    public GameObject bucketContent;
    public GameObject fluidContent;
    public GameObject forcesContent;
    public GameObject groundContent;

    [Header("Physics Script")]
    public MonoBehaviour physicsMovementScript;

    [Header("Paint settings")]
    public PaintSurfaceSystem paintSurfaceSystem;
    public PaintEmitter paintEmitter;

    private bool isPaused = false;

    void Start()
    {
        if (mainSettingsButton != null) mainSettingsButton.SetActive(true);
        if (mainSettingsMenuPanel != null) mainSettingsMenuPanel.SetActive(false);

        if (paintSurfaceSystem == null) paintSurfaceSystem = FindFirstObjectByType<PaintSurfaceSystem>();
        if (paintEmitter == null) paintEmitter = FindFirstObjectByType<PaintEmitter>();

        CloseAllSubMenus();
    }

    public void ToggleMainSettingsMenu()
    {
        if (mainSettingsMenuPanel != null)
        {
            bool isMenuOpen = !mainSettingsMenuPanel.activeSelf;
            mainSettingsMenuPanel.SetActive(isMenuOpen);

            if (!isMenuOpen) CloseAllSubMenus();

            Canvas.ForceUpdateCanvases();
        }
    }

    public void ToggleRope() { ropeContent.SetActive(!ropeContent.activeSelf); Canvas.ForceUpdateCanvases(); }
    public void ToggleBucket() { bucketContent.SetActive(!bucketContent.activeSelf); Canvas.ForceUpdateCanvases(); }
    public void ToggleFluid() { fluidContent.SetActive(!fluidContent.activeSelf); Canvas.ForceUpdateCanvases(); }
    public void ToggleForces() { forcesContent.SetActive(!forcesContent.activeSelf); Canvas.ForceUpdateCanvases(); }
    public void ToggleGround() { groundContent.SetActive(!groundContent.activeSelf); Canvas.ForceUpdateCanvases(); }

    private void CloseAllSubMenus()
    {
        if (ropeContent != null) ropeContent.SetActive(false);
        if (bucketContent != null) bucketContent.SetActive(false);
        if (fluidContent != null) fluidContent.SetActive(false);
        if (forcesContent != null) forcesContent.SetActive(false);
        if (groundContent != null) groundContent.SetActive(false);
        Canvas.ForceUpdateCanvases();
    }

    public void StartSimulation()
    {
        if (physicsMovementScript != null)
        {
            physicsMovementScript.enabled = true;
            Time.timeScale = 1f;
        }
    }

    public void ResetToOriginalValues()
    {
        if (paintEmitter != null)
        {
            paintEmitter.ClearParticles();
        }

        if (paintSurfaceSystem != null)
        {
            paintSurfaceSystem.ClearSurfaceCustom();
        }

        BucketLiquidVolume volume = FindObjectOfType<BucketLiquidVolume>();
        if (volume != null)
        {
            volume.ResetVolume();
        }
    }

    public void ExportPaintingToJPG()
    {
        if (paintSurfaceSystem == null)
        {
            return;
        }

        Texture2D currentTexture = paintSurfaceSystem.GetCurrentSurfaceTexture();

        if (currentTexture != null)
        {
            byte[] jpgBytes = currentTexture.EncodeToJPG(95);

            string folderPath = Path.Combine(Application.dataPath, "SavedPaintings");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = "Painting_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";
            string fullPath = Path.Combine(folderPath, fileName);

            File.WriteAllBytes(fullPath, jpgBytes);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh(); 
#endif
        }
        else
        {
            Debug.LogError("saved");
        }
    }

    [Header("Change color")]
    [SerializeField] private BucketLiquidVolume _bucketVolumeRef;

    public void SetColorRed() => ChangeSystemColor(Color.red);
    public void SetColorBlue() => ChangeSystemColor(Color.blue);
    public void SetColorYellow() => ChangeSystemColor(Color.yellow);
    public void SetColorGreen() => ChangeSystemColor(Color.green);

    public void ChangeSystemColor(Color newColor)
    {
        if (paintEmitter != null && paintEmitter.emissionConfig != null)
        {
            paintEmitter.emissionConfig.particleColor = newColor;
        }

        if (_bucketVolumeRef == null) _bucketVolumeRef = FindObjectOfType<BucketLiquidVolume>();
        if (_bucketVolumeRef != null)
        {
            _bucketVolumeRef.UpdateLiquidColor(newColor);
        }
    }

    public void ExitApplication()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // إذا تم تشغيل البرنامج كنسخة مستقلة EXE على الويندوز، سيغلق البرنامج فوراً
        Application.Quit();
#endif
    }

    /// <param name="dropdownIndex">رقم الخيار المحدد (0, 1, 2, 3)</param>
    public void OnGroundDropdownValueChanged(int dropdownIndex)
    {
        if (paintSurfaceSystem != null)
        {
            paintSurfaceSystem.ChangeSurfaceMaterialType(dropdownIndex);
        }
        else
        {
            paintSurfaceSystem = FindFirstObjectByType<PaintSurfaceSystem>();
            if (paintSurfaceSystem != null)
            {
                paintSurfaceSystem.ChangeSurfaceMaterialType(dropdownIndex);
            }
            else
            {
                Debug.LogError("no script found");
            }
        }
    }
    public void LoadSandboxScene()
    {
        SceneManager.LoadScene(1);
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene(0);
    }
}