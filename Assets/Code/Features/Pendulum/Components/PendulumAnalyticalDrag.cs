using UnityEngine;
using SwingingPaintBucket.Features.Pendulum.Data;

namespace SwingingPaintBucket.Features.Pendulum.Components
{
    public class PendulumAnalyticalDrag : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PendulumController _pendulumController;

        [Header("Detection Settings")]
        [SerializeField] private float _clickDetectionRadius = 0.5f; // نصف قطر مسافة النقر الافتراضية حول الدلو بديل الـ Collider

        private Camera _mainCamera;
        private bool _isDragging = false;

        // متغيرات لحساب السرعة المتولدة من السحب يدعائياً
        private Vector3 _lastWorldPos;
        private Vector3 _dragVelocity;

        private void Start()
        {
            _mainCamera = Camera.main;
            if (_pendulumController == null)
            {
                _pendulumController = GetComponent<PendulumController>();
            }
        }

        private void Update()
        {
            HandleManualInput();
        }

        private void HandleManualInput()
        {
            // 1. تحويل موقع الماوس إلى العالم ثلاثي الأبعاد في مستوى الحركة (Z الثابتة)
            Vector3 mouseWorldPos = GetMouseWorldPosition();

            // 2. التحقق من النقر (بديل الـ Collision)
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 bucketPos = _pendulumController.BucketTransform != null ? _pendulumController.BucketTransform.position : Vector3.zero;

                // حساب المسافة الرياضية بين الماوس والدلو
                float distanceToBucket = Vector2.Distance(new Vector2(mouseWorldPos.x, mouseWorldPos.y), new Vector2(bucketPos.x, bucketPos.y));

                if (distanceToBucket <= _clickDetectionRadius)
                {
                    StartDragging(mouseWorldPos);
                }
            }

            // 3. الاستمرار في السحب
            if (_isDragging && Input.GetMouseButton(0))
            {
                ContinueDragging(mouseWorldPos);
            }

            // 4. إفلات الماوس وحقن الدفع الابتدائي
            if (_isDragging && Input.GetMouseButtonUp(0))
            {
                StopDragging();
            }
        }

        private void StartDragging(Vector3 startMousePos)
        {
            _isDragging = false;
            _pendulumController.ResetSimulation(); // إيقاف المحاكاة لتعديل الإحداثيات يدوياً
            _isDragging = true;

            _lastWorldPos = startMousePos;
            _dragVelocity = Vector3.zero;
        }

        private void ContinueDragging(Vector3 currentMousePos)
        {
            if (Time.deltaTime > 0f)
            {
                _dragVelocity = (currentMousePos - _lastWorldPos) / Time.deltaTime;
            }
            _lastWorldPos = currentMousePos;

            // حسابات النواس الرياضية الماصة للحبل
            Vector3 pivotPos = _pendulumController.PivotTransform != null ? _pendulumController.PivotTransform.position : Vector3.zero;

            // حساب متجه الاتجاه من الارتكاز إلى موقع الماوس
            Vector3 directionToMouse = currentMousePos - pivotPos;

            // حساب الزاوية Theta رياضياً (التانغانت العكسي للمثلث القائم القائم)
            float theta = Mathf.Atan2(directionToMouse.x, -directionToMouse.y);

            // إجبار الموضع على التواجد تماماً على محيط الدائرة بناءً على الزاوية وطول الحبل الحالي
            float length = 3f; // القيمة الافتراضية وسيتم تحديثها تلقائياً من الإعدادات

            Vector3 constrainedBucketPos = pivotPos + length * new Vector3(Mathf.Sin(theta), -Mathf.Cos(theta), 0f);

            // تحديث المحاكي والمظهر البصري مباشرة
            _pendulumController.UpdateBucketPosition(constrainedBucketPos);
            _pendulumController.UpdateRope(pivotPos, constrainedBucketPos);
        }

        private void StopDragging()
        {
            _isDragging = false;

            // حساب الزاوية النهائية عند نقطة الإفلات
            Vector3 pivotPos = _pendulumController.PivotTransform != null ? _pendulumController.PivotTransform.position : Vector3.zero;
            Vector3 bucketPos = _pendulumController.BucketTransform.position;
            Vector3 direction = bucketPos - pivotPos;
            float finalTheta = Mathf.Atan2(direction.x, -direction.y);

            // حقن الحالة الفيزيائية والسرعة المكتسبة يدوياً لبدء المحاكاة تحليلياً
        }

        private Vector3 GetMouseWorldPosition()
        {
            // تحويل شاشة الماوس عبر إنشاء مسافة افتراضية ثابتة بين الكاميرا ومستوى اللعب (العمق Z)
            Vector3 pivotPos = _pendulumController.PivotTransform != null ? _pendulumController.PivotTransform.position : Vector3.zero;

            Vector3 mouseScreenPos = Input.mousePosition;
            // حساب بعد مستوى الحركة عن الكاميرا على المحور Z
            mouseScreenPos.z = Mathf.Abs(_mainCamera.transform.position.z - pivotPos.z);

            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
            worldPos.z = pivotPos.z; // التأكيد على بقاء الحركة ثنائية الأبعاد هندسياً في الفراغ

            return worldPos;
        }
    }
}