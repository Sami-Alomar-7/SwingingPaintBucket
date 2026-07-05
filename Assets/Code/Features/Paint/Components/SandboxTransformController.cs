using UnityEngine;
using UnityEngine.InputSystem; // استيراد النظام الجديد لضمان عمل الكود

public class SandboxTransformController : MonoBehaviour
{
    [Header("سرعات التحكم اليدوي")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 60f;
    public float scaleSpeed = 1f;

    void Update()
    {
        // 1. التحريك عبر أزرار الأسهم أو WASD + (Q/E للارتفاع)
        float moveX = 0f;
        float moveZ = 0f;
        float moveY = 0f;

        if (Keyboard.current != null)
        {
            // الحركة الأفقية والعميقية
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ = -1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;

            // الارتفاع والانخفاض
            if (Keyboard.current.eKey.isPressed) moveY = 1f;
            if (Keyboard.current.qKey.isPressed) moveY = -1f;
        }

        Vector3 movement = new Vector3(moveX, moveY, moveZ).normalized * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // 2. التدوير عبر الضغط المستمر على زر الفأرة الأيمن وتحريك الفأرة
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            float rotX = mouseDelta.y * (rotateSpeed * 0.1f) * Time.deltaTime;
            float rotY = mouseDelta.x * (rotateSpeed * 0.1f) * Time.deltaTime;

            transform.Rotate(Vector3.up, -rotY, Space.World);
            transform.Rotate(Vector3.right, rotX, Space.Self);
        }

        // 3. التكبير والتصغير باستخدام بكرة الفأرة (Scroll Wheel)
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                float scrollDirection = Mathf.Sign(scroll);
                Vector3 newScale = transform.localScale + Vector3.one * scrollDirection * scaleSpeed * 0.2f;

                // حدود أمان لمنع اختفاء المكعب أو تضخمه بشكل مفرط
                newScale = Vector3.Max(newScale, Vector3.one * 0.2f);
                newScale = Vector3.Min(newScale, Vector3.one * 3f);
                transform.localScale = newScale;
            }
        }
    }
}