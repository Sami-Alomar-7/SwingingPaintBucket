using UnityEngine;
using System.Collections.Generic;
using SwingingPaintBucket.Features.Paint.Data;
using SwingingPaintBucket.Features.Paint.Interfaces;
using SwingingPaintBucket.Features.Surface.Components;
using SwingingPaintBucket.Features.Surface.Data;

namespace SwingingPaintBucket.Features.Paint.Services
{
    /// <summary>
    /// محرك SPH احترافي (Müller 2003) معاد هيكلته للأداء والاستقرار:
    /// 1) بحث الجيران عبر تجزئة فراغية (Spatial Hash Grid) بدل المقارنة الثنائية O(n²).
    /// 2) تخزين مؤقت لمرجع السطح بدل البحث في المشهد كل إطار.
    /// 3) إعادة استخدام كل المخازن المؤقتة لمنع ضغط الـ GC.
    /// 4) دمج زمني شبه-ضمني مع تقييد CFL لمنع انفجار المحاكاة.
    /// </summary>
    public class ParticlePhysicsService : IParticlePhysicsService
    {
        private const float PI = Mathf.PI;

        // حدود الأمان لمنع الانفجار العددي (Numerical Blow-up)
        private const float MaxAcceleration = 200f;
        private const float MaxSpeed = 40f;

        // ===== مخازن معاد استخدامها عبر الإطارات (Zero per-frame allocation) =====
        private readonly Dictionary<long, List<int>> _grid = new Dictionary<long, List<int>>(1024);
        private readonly List<List<int>> _cellPool = new List<List<int>>();
        private int _cellPoolUsed;
        private readonly List<int> _neighbors = new List<int>(64);
        private readonly List<int> _toRemove = new List<int>(64);
        private float _cellSize = 0.1f;

        // مرجع السطح مخزن مؤقتاً لتفادي FindObjectOfType في كل FixedUpdate
        private PaintSurfaceSystem _cachedSurface;

        public List<int> UpdateParticles(List<ParticleData> particles, float deltaTime, float surfaceY, PaintEmissionConfig config)
        {
            _toRemove.Clear();
            if (particles == null || particles.Count == 0 || config == null) return _toRemove;

            // جلب نوع المادة من السطح المخزن مؤقتاً (قراءة materialType رخيصة وتعكس تغييرات القائمة فوراً)
            if (_cachedSurface == null) _cachedSurface = Object.FindObjectOfType<PaintSurfaceSystem>();
            SurfaceMaterialType material = _cachedSurface != null ? _cachedSurface.materialType : SurfaceMaterialType.Wood;

            // تحديد الاحتكاك (Friction) والامتصاص (Absorption) بحسب نوع السطح
            float friction = 0.2f;
            float absorption = 0.0f;
            switch (material)
            {
                case SurfaceMaterialType.Wood:
                    friction = 0.8f; absorption = 0.2f; break; // احتكاك قوي يثبت الكرات مكانها
                case SurfaceMaterialType.Metal:
                    friction = 0.05f; absorption = 0.0f; break; // انزلاق سلس للجزيئات
                case SurfaceMaterialType.Paper:
                    friction = 0.4f; absorption = 0.9f; break; // امتصاص هائل يجعل الجزيء ينكمش ويختفي بسرعة
                case SurfaceMaterialType.Glass:
                    friction = 0.01f; absorption = 0.0f; break;
            }

            float h = Mathf.Max(config.smoothingRadius, 0.0001f);
            float h2 = h * h;
            float mass = config.particleMass;
            float restDensity = config.restDensity;
            float k = config.pressureStiffness;
            float mu = config.viscosity;
            float sigma = config.surfaceTension;
            float gravity = config.gravity;

            int count = particles.Count;

            // ثوابت نوى SPH ثلاثية الأبعاد (Poly6 / Spiky / Viscosity-Laplacian)
            float poly6Constant = 315f / (64f * PI * Mathf.Pow(h, 9));
            float spikyGradientConstant = -45f / (PI * Mathf.Pow(h, 6));
            float viscLaplacianConstant = 45f / (PI * Mathf.Pow(h, 6));

            // بناء شبكة التجزئة الفراغية بحجم خلية = نصف قطر التنعيم h
            BuildGrid(particles, count, h);

            // ===== المرحلة 1: حساب الكثافة والضغط لكل جزيء عبر جيرانه فقط =====
            for (int i = 0; i < count; i++)
            {
                ParticleData pi = particles[i];
                CollectNeighbors(pi.position);
                float density = 0f;
                int nCount = _neighbors.Count;
                for (int n = 0; n < nCount; n++)
                {
                    Vector3 diff = pi.position - particles[_neighbors[n]].position;
                    float r2 = diff.sqrMagnitude;
                    if (r2 < h2)
                    {
                        float term = h2 - r2;
                        density += mass * poly6Constant * term * term * term;
                    }
                }

                if (density < restDensity * 0.1f) density = restDensity * 0.1f;
                pi.density = density;
                pi.pressure = Mathf.Max(0f, k * (density - restDensity)); // لا ضغط شدّي لتفادي عدم الاستقرار
                pi.inverseDensity = 1f / density;
            }

            // ===== المرحلة 2: حساب قوى الضغط واللزوجة والتوتر السطحي =====
            for (int i = 0; i < count; i++)
            {
                ParticleData pi = particles[i];
                CollectNeighbors(pi.position);

                Vector3 forcePressure = Vector3.zero;
                Vector3 forceViscosity = Vector3.zero;
                Vector3 colorFieldGradient = Vector3.zero;
                float colorFieldLaplacian = 0f;

                int nCount = _neighbors.Count;
                for (int n = 0; n < nCount; n++)
                {
                    int j = _neighbors[n];
                    if (i == j) continue;

                    ParticleData pj = particles[j];
                    Vector3 diff = pi.position - pj.position;
                    float r2 = diff.sqrMagnitude;

                    if (r2 < h2 && r2 > 0.000001f)
                    {
                        float r = Mathf.Sqrt(r2);
                        Vector3 dir = diff / r;
                        float hMinusR = h - r;

                        float gradW = spikyGradientConstant * hMinusR * hMinusR;
                        forcePressure += -mass * ((pi.pressure + pj.pressure) / (2f * pj.density)) * gradW * dir;

                        float lapW = viscLaplacianConstant * hMinusR;
                        forceViscosity += mu * mass * (pj.velocity - pi.velocity) / pj.density * lapW;

                        float poly6Term = h2 - r2;
                        colorFieldGradient += (mass / pj.density) * poly6Constant * 3f * poly6Term * poly6Term * (-2f) * diff;
                        colorFieldLaplacian += (mass / pj.density) * poly6Constant * 6f * poly6Term * (r2 - 3f * poly6Term);
                    }
                }

                Vector3 forceSurfaceTension = Vector3.zero;
                float normalMagnitude = colorFieldGradient.magnitude;
                if (normalMagnitude > 0.1f)
                {
                    forceSurfaceTension = -sigma * colorFieldLaplacian * (colorFieldGradient / normalMagnitude);
                }

                pi.forcePhysics = forcePressure + forceViscosity + forceSurfaceTension;
            }

            // ===== المرحلة 3: الدمج الزمني والاصطدام مع السطح =====
            for (int i = 0; i < count; i++)
            {
                ParticleData p = particles[i];
                Vector3 acceleration = (p.forcePhysics * p.inverseDensity) + new Vector3(0f, -gravity, 0f);

                if (acceleration.sqrMagnitude > MaxAcceleration * MaxAcceleration)
                    acceleration = acceleration.normalized * MaxAcceleration;

                // دمج شبه-ضمني (Semi-implicit Euler) مع تقييد السرعة (CFL) لمنع الانفجار
                p.velocity += acceleration * deltaTime;
                if (p.velocity.sqrMagnitude > MaxSpeed * MaxSpeed)
                    p.velocity = p.velocity.normalized * MaxSpeed;

                p.position += p.velocity * deltaTime;
                p.lifeRemaining -= deltaTime;
                // امتصاص السطح: تقليص حجم الجزيء المستقر حتى يختفي
                if (p.isGrounded && absorption > 0f)
                {
                    p.size = Mathf.MoveTowards(p.size, 0f, absorption * deltaTime * 0.04f);
                    if (p.size <= 0.01f)
                    {
                        _toRemove.Add(i);
                        continue;
                    }
                }

                // اصطدام مستوى السطح + احتكاك أفقي
                if (p.position.y <= surfaceY + 0.02f)
                {
                    p.position.y = surfaceY + 0.01f;
                    p.isGrounded = true;

                    p.velocity.x *= (1f - friction);
                    p.velocity.z *= (1f - friction);
                    p.velocity.y = 0f;
                }

                // الجزيئات الطائرة فقط تنتهي بانتهاء عمرها (المستقرة تبقى كبقعة)
                if (p.lifeRemaining <= 0f && !p.isGrounded)
                    _toRemove.Add(i);
            }

            return _toRemove;
        }

        // ===================== تجزئة فراغية (Spatial Hash Grid) =====================

        private void BuildGrid(List<ParticleData> particles, int count, float h)
        {
            _cellSize = h;
            ClearGrid();

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = particles[i].position;
                long key = CellKey(FloorToCell(pos.x), FloorToCell(pos.y), FloorToCell(pos.z));
                GetCell(key).Add(i);
            }
        }

        // تجميع مرشحي الجيران من الخلايا الـ27 المحيطة (3×3×3)
        private void CollectNeighbors(Vector3 position)
        {
            _neighbors.Clear();
            int cx = FloorToCell(position.x);
            int cy = FloorToCell(position.y);
            int cz = FloorToCell(position.z);

            for (int gx = -1; gx <= 1; gx++)
                for (int gy = -1; gy <= 1; gy++)
                    for (int gz = -1; gz <= 1; gz++)
                    {
                        long key = CellKey(cx + gx, cy + gy, cz + gz);
                        if (_grid.TryGetValue(key, out List<int> cell))
                            _neighbors.AddRange(cell);
                    }
        }

        private int FloorToCell(float v) => Mathf.FloorToInt(v / _cellSize);

        // دالة تجزئة Teschner et al. — التصادمات نادرة وتُصحَّح لاحقاً بفحص المسافة الفعلي
        private static long CellKey(int x, int y, int z)
        {
            const long p1 = 73856093;
            const long p2 = 19349663;
            const long p3 = 83492791;
            return (x * p1) ^ (y * p2) ^ (z * p3);
        }

        private List<int> GetCell(long key)
        {
            if (!_grid.TryGetValue(key, out List<int> cell))
            {
                cell = RentCell();
                _grid[key] = cell;
            }
            return cell;
        }

        private void ClearGrid()
        {
            _grid.Clear();
            _cellPoolUsed = 0; // القوائم تبقى في المخزن وتُمسح عند إعادة طلبها
        }

        private List<int> RentCell()
        {
            if (_cellPoolUsed < _cellPool.Count)
            {
                List<int> reused = _cellPool[_cellPoolUsed++];
                reused.Clear();
                return reused;
            }

            List<int> fresh = new List<int>(8);
            _cellPool.Add(fresh);
            _cellPoolUsed++;
            return fresh;
        }
    }
}