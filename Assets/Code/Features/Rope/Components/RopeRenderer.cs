using UnityEngine;
using SwingingPaintBucket.Features.Rope.Interfaces;
using SwingingPaintBucket.Features.Rope.Data;

namespace SwingingPaintBucket.Features.Rope.Components
{
    [RequireComponent(typeof(LineRenderer))]
    public class RopeRenderer : MonoBehaviour, IRope
    {
        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            InitializeLineRenderer();
        }

        private void InitializeLineRenderer()
        {
            if (_lineRenderer == null) return;

            _lineRenderer.positionCount = 2;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.numCapVertices = 5;

            // استخدام شيدر يدعم الألوان الشفافة والأساسية بوضوح
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startColor = Color.gray;
            _lineRenderer.endColor = Color.gray;
        }

        /// <inheritdoc />
        public void UpdateRope(Vector3 pivotPos, Vector3 bucketPos)
        {
            if (_lineRenderer == null) return;

            _lineRenderer.SetPosition(0, pivotPos);
            _lineRenderer.SetPosition(1, bucketPos);
        }

        // دالة جديدة لتحديث مظهر الحبل فوراً من الكود عند اختيار نوع جديد
        public void ApplyVisualConfig(RopeConfig config)
        {
            if (_lineRenderer == null || config == null) return;

            _lineRenderer.startWidth = config.StartWidth;
            _lineRenderer.endWidth = config.EndWidth;
            _lineRenderer.startColor = config.RopeColor;
            _lineRenderer.endColor = config.RopeColor;
        }
    }
}