using UnityEngine;
using UnityEngine.SceneManagement;

public class AdvancedMenuController : MonoBehaviour
{
    [Header("زر الإعدادات الرئيسي والقائمة الكاملة")]
    // زر العنوان الأساسي (System Config) - سيبقى ظاهراً للفتح والإغلاق دائماً
    public GameObject mainSettingsButton;
    // الـ Panel الأب التي تحتوي على الأزرار الفرعية وأزرار التحكم
    public GameObject mainSettingsMenuPanel;

    [Header("قوائم المحتوى للأقسام (المستوى الثالث)")]
    public GameObject ropeContent;
    public GameObject bucketContent;
    public GameObject fluidContent;
    public GameObject forcesContent;
    public GameObject groundContent;

    [Header("ربط سكربت الحركة والمحاكاة")]
    public MonoBehaviour physicsMovementScript;

    private bool isPaused = false;

    /// <summary>
    /// تابع زر الإعدادات الأساسي: يفتح ويغلق القائمة التحتية ايمتى ما بدكِ بدون إخفاء الزر نفسه
    /// </summary>
    public void ToggleMainSettingsMenu()
    {
        if (mainSettingsMenuPanel != null)
        {
            // عكس حالة القائمة (إذا مفتوحة تغلق، والعكس)
            bool isMenuOpen = !mainSettingsMenuPanel.activeSelf;

            mainSettingsMenuPanel.SetActive(isMenuOpen);

            // إذا أغلقنا القائمة، نغلق تلقائياً كافة المحتويات الفرعية لتنكمش اللوحة تماماً
            if (!isMenuOpen)
            {
                CloseAllSubMenus();
            }

            // إجبار الـ UI على إعادة حساب الأبعاد والانكماش فوراً
            Canvas.ForceUpdateCanvases();
        }
    }

    // توابع فتح وإغلاق الأقسام الفرعية مع تحديث فوري للأبعاد
    public void ToggleRope() { ropeContent.SetActive(!ropeContent.activeSelf); Canvas.ForceUpdateCanvases(); }
    public void ToggleBucket() { bucketContent.SetActive(!bucketContent.activeSelf); Canvas.ForceUpdateCanvases(); }
    public void ToggleFluid() { fluidContent.SetActive(!fluidContent.activeSelf); Canvas.ForceUpdateCanvases(); }
    public void ToggleForces() { forcesContent.SetActive(!forcesContent.activeSelf); Canvas.ForceUpdateCanvases(); }
    public void ToggleGround() { groundContent.SetActive(!groundContent.activeSelf); Canvas.ForceUpdateCanvases(); }

    void Start()
    {
        // عند تشغيل اللعبة: زر الإعدادات الأساسي ظاهر، بينما القائمة التحتية ومحتوياتها مخفية تماماً
        if (mainSettingsMenuPanel != null) mainSettingsMenuPanel.SetActive(false);
        if (mainSettingsButton != null) mainSettingsButton.SetActive(true);

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

    // ========== أزرار التحكم بالمحاكاة ==========
    public void StartSimulation()
    {
        if (physicsMovementScript != null)
        {
            physicsMovementScript.enabled = true;
            Time.timeScale = 1f;
            Debug.Log("انطلقت المحاكاة برمجياً!");
        }
    }

    public void TogglePauseSimulation()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResetToOriginalValues()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}