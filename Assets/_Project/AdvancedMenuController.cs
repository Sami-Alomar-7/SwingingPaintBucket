using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using SwingingPaintBucket.Features.Surface.Components;
using SwingingPaintBucket.Features.Paint.Components;

public class AdvancedMenuController : MonoBehaviour
{
    [Header("زر الإعدادات الرئيسي والقائمة الكاملة")]
    public GameObject mainSettingsButton;
    public GameObject mainSettingsMenuPanel;

    [Header("قوائم المحتوى للأقسام (المستوى الثالث)")]
    public GameObject ropeContent;
    public GameObject bucketContent;
    public GameObject fluidContent;
    public GameObject forcesContent;
    public GameObject groundContent;

    [Header("ربط سكربت الحركة والمحاكاة")]
    public MonoBehaviour physicsMovementScript;

    [Header("ربط نظام الرسم والباعث (مهم للـ Reset والحفظ)")]
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
            Debug.Log("انطلقت المحاكاة برمجياً!");
        }
    }

    // زر الريسيت المطور: يمسح اللوحة والجسيمات فوراً مع الحفاظ على القوائم والقيم المعدلة كما هي
    public void ResetToOriginalValues()
    {
        // 1. مسح وتفريغ جزيئات الطلاء الطائرة في الهواء فوراً لمنع تساقطها بعد التنظيف
        if (paintEmitter != null)
        {
            paintEmitter.ClearParticles();
            Debug.Log("تم مسح الجسيمات الطائرة بنجاح.");
        }

        // 2. استدعاء دالة التنظيف المخصصة لتبييض اللوحة بالكامل وتصفير مصفوفة الـ SPH الداخلية
        if (paintSurfaceSystem != null)
        {
            paintSurfaceSystem.ClearSurfaceCustom();
            Debug.Log("تم تنظيف السطح وتبييض الورقة برمجياً دون إعادة تحميل المشهد!");
        }

        // 3. إعادة تهيئة السائل في الدلو
        BucketLiquidVolume volume = FindObjectOfType<BucketLiquidVolume>();
        if (volume != null)
        {
            volume.ResetVolume();
        }
    }

    // زر حفظ النتيجة الحالية للوحة كصورة JPG داخل ملفات المشروع
    public void ExportPaintingToJPG()
    {
        if (paintSurfaceSystem == null)
        {
            Debug.LogError("لا يمكن الحفظ، سكربت PaintSurfaceSystem غير مرتبط!");
            return;
        }

        // جلب التيكستشر المحدث والمكتوب عليه فعلياً من نظام السطح
        Texture2D currentTexture = paintSurfaceSystem.GetCurrentSurfaceTexture();

        if (currentTexture != null)
        {
            // تحويل مصفوفة البكسلات الحالية اللوحة الفنية إلى صيغة JPG
            byte[] jpgBytes = currentTexture.EncodeToJPG(95);

            // تحديد مسار الحفظ داخل مجلد مشروعك (Assets/SavedPaintings)
            string folderPath = Path.Combine(Application.dataPath, "SavedPaintings");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = "Painting_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";
            string fullPath = Path.Combine(folderPath, fileName);

            File.WriteAllBytes(fullPath, jpgBytes);
            Debug.Log($"تم حفظ اللوحة الفنية بنجاح بالتيكستشر المحدث في: {fullPath}");

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh(); // إظهار الصورة فوراً في نافذة الـ Project
#endif
        }
        else
        {
            Debug.LogError("لم يتم العثور على قوام نسيجي (Texture2D) جاهز للحفظ!");
        }
    }

    // ==================== إعدادات تغيير الألوان السريعة عبر الواجهة ====================
    [Header("إعدادات تغيير الألوان السريعة عبر الواجهة")]
    [SerializeField] private BucketLiquidVolume _bucketVolumeRef;

    // دوال جاهزة للربط المباشر مع الأزرار في Unity Inspector لتغيير اللون
    public void SetColorRed() => ChangeSystemColor(Color.red);
    public void SetColorBlue() => ChangeSystemColor(Color.blue);
    public void SetColorYellow() => ChangeSystemColor(Color.yellow);
    public void SetColorGreen() => ChangeSystemColor(Color.green);

    public void ChangeSystemColor(Color newColor)
    {
        // 1. تغيير لون الجزيئات التي ستسقط في الهواء مستقبلاً
        if (paintEmitter != null && paintEmitter.emissionConfig != null)
        {
            paintEmitter.emissionConfig.particleColor = newColor;
        }

        // 2. تغيير لون الجزيئات والمادة الموجودة حالياً داخل الدلو
        if (_bucketVolumeRef == null) _bucketVolumeRef = FindObjectOfType<BucketLiquidVolume>();
        if (_bucketVolumeRef != null)
        {
            _bucketVolumeRef.UpdateLiquidColor(newColor);
        }
    }

    // ==================== دالة إغلاق البرنامج بالكامل (نسخة الـ EXE) ====================
    /// <summary>
    /// تقوم هذه الدالة بإنهاء تشغيل البرنامج بشكل كامل فوراً عند الضغط على الزر،
    /// وتعمل بنجاح سواء كنت داخل محرّر Unity أو بعد تصدير البرنامج كملف EXE مستقل.
    /// </summary>
    public void ExitApplication()
    {
        Debug.Log("جاري إنهاء وإغلاق التطبيق بالكامل...");

#if UNITY_EDITOR
        // إذا كنتِ تختبرين البرنامج داخل محرك اليونيتي، سيتم إيقاف وضع التشغيل (Play Mode)
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // إذا تم تشغيل البرنامج كنسخة مستقلة EXE على الويندوز، سيغلق البرنامج فوراً
        Application.Quit();
#endif
    }
    // ==================== استقبال وتحديث نوع الأرضية عبر الـ Dropdown ====================
    /// <summary>
    /// يتم استدعاء هذه الدالة تلقائياً عند تغيير الخيار داخل الـ Dropdown في الواجهة
    /// </summary>
    /// <param name="dropdownIndex">رقم الخيار المحدد (0, 1, 2, 3)</param>
    public void OnGroundDropdownValueChanged(int dropdownIndex)
    {
        if (paintSurfaceSystem != null)
        {
            paintSurfaceSystem.ChangeSurfaceMaterialType(dropdownIndex);
        }
        else
        {
            // محاولة جلب المرجع تلقائياً إذا لم يكن مسحوباً في الـ Inspector
            paintSurfaceSystem = FindFirstObjectByType<PaintSurfaceSystem>();
            if (paintSurfaceSystem != null)
            {
                paintSurfaceSystem.ChangeSurfaceMaterialType(dropdownIndex);
            }
            else
            {
                Debug.LogError("لم يتم العثور على سكربت PaintSurfaceSystem في المشهد لتحديث نوع الأرضية!");
            }
        }
    }
    public void LoadSandboxScene()
    {
        // الانتقال إلى مشهد المعاينة اليدوية لتخفيف الضغط
        SceneManager.LoadScene(1);
    }

    public void LoadMainScene()
    {
        // العودة للمشهد الفيزيائي الرئيسي
        SceneManager.LoadScene(0);
    }
}