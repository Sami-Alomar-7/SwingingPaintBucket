using UnityEngine;
using SwingingPaintBucket.Features.Pendulum.Components;

namespace SwingingPaintBucket.Features.Paint.Components
{
    public class BucketApertureVisualizer : MonoBehaviour
    {
        [SerializeField] private PendulumController _pendulumController;

        private float _currentDiameter;
        private int _holesCount;

        private void Start()
        {
            if (_pendulumController == null)
                _pendulumController = FindAnyObjectByType<PendulumController>();

            UpdateApertureData();
        }

        private void Update()
        {
            UpdateApertureData();
        }

        private void UpdateApertureData()
        {
            if (_pendulumController == null) return;

            // جلب قطر الفتحة وعدد الثقوب الجديد برمجياً في كل إطار
            _currentDiameter = _pendulumController.CurrentApertureDiameter;
            _holesCount = _pendulumController.HolesCount;

            // الشكل البصري ثابت لا يتغير كما طلبتِ تماماً
        }

        public float CurrentDiameter => _currentDiameter;
        public int HolesCount => _holesCount;
    }
}