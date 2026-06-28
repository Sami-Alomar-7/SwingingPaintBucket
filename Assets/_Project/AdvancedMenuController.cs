using UnityEngine;
using UnityEngine.SceneManagement;

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

    private bool isPaused = false;

    public void ToggleMainSettingsMenu()
    {
        if (mainSettingsMenuPanel != null)
        {
            bool isMenuOpen = !mainSettingsMenuPanel.activeSelf;

            mainSettingsMenuPanel.SetActive(isMenuOpen);

            if (!isMenuOpen)
            {
                CloseAllSubMenus();
            }

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

    public void ResetToOriginalValues()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}