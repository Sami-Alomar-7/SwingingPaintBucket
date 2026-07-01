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

    void Start()
    {
        if (mainSettingsButton != null) mainSettingsButton.SetActive(true);
        if (mainSettingsMenuPanel != null) mainSettingsMenuPanel.SetActive(false);

        if (paintSurfaceSystem == null) paintSurfaceSystem = FindFirstObjectByType<PaintSurfaceSystem>();
        if (paintEmitter == null) paintEmitter = FindFirstObjectByType<PaintEmitter>();

        CloseAllSubMenus();
    }

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
        // ابحثي عن مرجع الـ BucketLiquidVolume واستدعي الدالة
        FindObjectOfType<BucketLiquidVolume>().ResetVolume();
        // ملاحظة: حذفنا سطر SceneManager.LoadScene تماماً 
        // لتبقى القوائم (UI Panels) مفتوحة وتحتفظ بكل القيم الفيزيائية التي قمتِ بتعديلها.
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
}