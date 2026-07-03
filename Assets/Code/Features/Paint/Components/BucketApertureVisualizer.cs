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
            ApplyScale();
        }

        public void UpdateApertureScale()
        {
            if (_pendulumController != null && _pendulumController.StartButton != null)
            {
                _pendulumController.StartButton.onClick.RemoveListener(ApplyScale);
                _pendulumController.StartButton.onClick.AddListener(ApplyScale);
            }
        }

        private void ApplyScale()
        {
            if (_pendulumController == null) return;

            float diameter = _pendulumController.CurrentApertureDiameter;

            transform.localScale = new Vector3(diameter, 0.001f, diameter);
        }
    }
}