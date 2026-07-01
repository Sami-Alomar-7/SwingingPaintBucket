using UnityEngine;
using SwingingPaintBucket.Features.Pendulum.Components;

namespace SwingingPaintBucket.Features.Paint.Components
{
    public class BucketApertureVisualizer : MonoBehaviour
    {
        [SerializeField] private PendulumController _pendulumController;

        private void Start()
        {
            if (_pendulumController == null)
                _pendulumController = FindAnyObjectByType<PendulumController>();

            UpdateApertureScale();
            ApplyScale(); // استدعاء أولي لتطبيق القيمة الافتراضية عند الإقلاع
        }

        public void UpdateApertureScale()
        {
            if (_pendulumController != null && _pendulumController.StartButton != null)
            {
                // إزالة المستمع القديم أولاً لمنع التكرار المزدوج في الذاكرة
                _pendulumController.StartButton.onClick.RemoveListener(ApplyScale);
                _pendulumController.StartButton.onClick.AddListener(ApplyScale);
            }
        }

        private void ApplyScale()
        {
            if (_pendulumController == null) return;

            float diameter = _pendulumController.CurrentApertureDiameter;

            // تعديل الحجم المحيطي بناءً على القطر الممرر، مع الحفاظ على سماكة الـ Y رقيقة جداً ومسطحة على القاع
            transform.localScale = new Vector3(diameter, 0.001f, diameter);
        }
    }
}