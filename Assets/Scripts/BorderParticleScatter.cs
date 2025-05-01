using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BorderParticleScatter : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private float animationTime = 1f;
    [SerializeField] private float baseOffset = 0.1f;
    [SerializeField] private float maxOffsetVariation = 0.05f;
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float speedVariation = 0.2f;
    [SerializeField] private int particlesPerSide = 7;
    
    private bool _isScattering = false;
    
    private List<ParticleData> _particles = new List<ParticleData>();

    public void EmitParticles(SpriteRenderer target)
    {
        if (_isScattering)
            return;
        _isScattering = true;
        
        ClearParticles();
        StopAllCoroutines();
        Vector3[] points = CalculateOffsetPoints(target);
        CreateParticles(target, points);
        StartCoroutine(AnimateParticles());
    }

    public void ManuallyStopScattering()
    {
        if (_isScattering)
            _isScattering = false;
    }

    private Vector3[] CalculateOffsetPoints(SpriteRenderer target)
    {
        Bounds bounds = target.bounds;
        Vector3 center = bounds.center;
        List<Vector3> points = new List<Vector3>();

        void AddSide(Vector3 start, Vector3 end)
        {
            for (int i = 0; i < particlesPerSide; i++)
            {
                float t = (float)i / (particlesPerSide - 1);
                Vector3 point = Vector3.Lerp(start, end, t);
                Vector3 dir = (point - center).normalized;
                
                float randomOffset = baseOffset + Random.Range(-maxOffsetVariation, maxOffsetVariation);
                points.Add(point + dir * randomOffset);
            }
        }

        AddSide(bounds.min, new Vector3(bounds.max.x, bounds.min.y));
        AddSide(new Vector3(bounds.max.x, bounds.min.y), bounds.max);
        AddSide(bounds.max, new Vector3(bounds.min.x, bounds.max.y));
        AddSide(new Vector3(bounds.min.x, bounds.max.y), bounds.min);

        return points.ToArray();
    }

    private void CreateParticles(SpriteRenderer target, Vector3[] points)
    {
        foreach (Vector3 point in points)
        {
            GameObject particle = new GameObject("BorderParticle");
            particle.transform.position = point;
            
            SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[Random.Range(0, sprites.Length)];
            sr.sortingOrder = target.sortingOrder - 1;

            Vector3 moveDirection = (point - target.bounds.center).normalized;
            float randomSpeed = moveSpeed + Random.Range(-speedVariation, speedVariation);
            
            _particles.Add(new ParticleData 
            {
                transform = particle.transform,
                direction = moveDirection,
                speed = randomSpeed,
                initialScale = Vector3.one
            });
        }
    }

    private IEnumerator AnimateParticles()
    {
        float timer = 0f;
        
        while (timer < animationTime)
        {
            float progress = timer / animationTime;
            
            foreach (ParticleData data in _particles)
            {
                if (data.transform == null) continue;
                
                data.transform.localScale = Vector3.Lerp(data.initialScale, Vector3.zero, progress);
                data.transform.position += data.direction * data.speed * Time.deltaTime;
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        _isScattering = false;
        ClearParticles();
    }

    private void ClearParticles()
    {
        foreach (ParticleData data in _particles)
            if (data.transform != null) Destroy(data.transform.gameObject);
        
        _particles.Clear();
    }

    private struct ParticleData
    {
        public Transform transform;
        public Vector3 direction;
        public float speed;
        public Vector3 initialScale;
    }
}
