using UnityEngine;
using SwingingPaintBucket.Features.Rope.Interfaces;

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
            _lineRenderer.startWidth = 0.05f;
            _lineRenderer.endWidth = 0.05f;
            _lineRenderer.numCapVertices = 5;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startColor = new Color(0.55f, 0.55f, 0.55f, 1f);
            _lineRenderer.endColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        }

        /// <inheritdoc />
        public void UpdateRope(Vector3 pivotPos, Vector3 bucketPos)
        {
            if (_lineRenderer == null) return;

            _lineRenderer.SetPosition(0, pivotPos);
            _lineRenderer.SetPosition(1, bucketPos);
        }
    }
}