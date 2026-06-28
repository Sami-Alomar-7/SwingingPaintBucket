using UnityEngine;
using SwingingPaintBucket.Features.Rope.Interfaces;
using SwingingPaintBucket.Features.Rope.Data;

namespace SwingingPaintBucket.Features.Rope.Components
{
    [RequireComponent(typeof(LineRenderer))]
    public class RopeRenderer : MonoBehaviour, IRope
    {
        private LineRenderer _lineRenderer;
        private RopeConfig _activeConfig;

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

            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startColor = Color.gray;
            _lineRenderer.endColor = Color.gray;
        }

        /// <inheritdoc />
        public void UpdateRope(Vector3 pivotPos, Vector3 bucketPos)
        {
            if (_lineRenderer == null) return;

            // رسم مكان الحبل الأساسي
            _lineRenderer.SetPosition(0, pivotPos);
            _lineRenderer.SetPosition(1, bucketPos);

            // 🌟 التأثير البصري المطور لتمدد ونحافة الحبل في الزمن الحقيقي
            if (_activeConfig != null && _activeConfig.AllowStretch && _activeConfig.RestLength > 0f)
            {
                float currentLength = Vector3.Distance(pivotPos, bucketPos);

                // حساب معامل الاستطالة (إذا كان أكبر من 1 يعني الحبل ممدود)
                float stretchFactor = currentLength / _activeConfig.RestLength;
                stretchFactor = Mathf.Max(1f, stretchFactor); // حصر المعامل لمنع التضخم عند الانكماش

                // قاعدة فيزيائية: كلما زاد الطول قل السمك (الحفاظ على الحجم بصرياً)
                float dynamicWidthFactor = 1f / Mathf.Sqrt(stretchFactor);

                _lineRenderer.startWidth = _activeConfig.StartWidth * dynamicWidthFactor;
                _lineRenderer.endWidth = _activeConfig.EndWidth * dynamicWidthFactor;
            }
        }

        public void ApplyVisualConfig(RopeConfig config)
        {
            if (_lineRenderer == null || config == null) return;

            _activeConfig = config;

            _lineRenderer.startWidth = config.StartWidth;
            _lineRenderer.endWidth = config.EndWidth;
            _lineRenderer.startColor = config.RopeColor;
            _lineRenderer.endColor = config.RopeColor;
        }
    }
}