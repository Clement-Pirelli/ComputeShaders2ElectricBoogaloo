using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
#pragma warning disable CS0649

public struct Boid
{
    public Vector2 position;
    public Vector2 direction;
}

public class Boids : MonoBehaviour
{
    [SerializeField, Range(1, 500)]
    int boidsAmount = 100;

    Boid[] boids = new Boid[100];
    Vector2[] nextFrameDirections = new Vector2[100];
    Matrix4x4[] boidMatrices = new Matrix4x4[100];
    Mesh boidMesh;

    [SerializeField]
    float speed = 1.0f;
    [SerializeField]
    float playAreaHalfDimensions = 10.0f;
    [SerializeField]
    float playAreaBorder = 2.0f;
    float InsideBordersMin { get => -playAreaHalfDimensions + playAreaBorder; }
    float InsideBordersMax { get => playAreaHalfDimensions - playAreaBorder; }

    [Header("Separation")]

    [SerializeField, Range(.1f, 10.0f)]
    float separationRadius = 2.0f;
    [SerializeField, Range(.0f, 1.0f)]
    float separationWeight = .5f;

    [Header("Cohesion")]

    [SerializeField, Range(.1f, 10.0f)]
    float cohesionRadius = 2.0f;
    [SerializeField, Range(.0f, 1.0f)]
    float cohesionWeight = .5f;

    [Header("Alignment")]

    [SerializeField, Range(.1f, 10.0f)]
    float alignmentRadius = 2.0f;
    [SerializeField, Range(.0f, 1.0f)]
    float alignmentWeight = .5f;

    [Header("Other")]

    [SerializeField]
    Material boidMaterial;

    [Button]
    private void Reset()
    {
        boids = new Boid[boidsAmount];
        nextFrameDirections = new Vector2[boidsAmount];
        boidMatrices = new Matrix4x4[boidsAmount];

        RandomizeBoids();
    }

    Vector2 RandomDirection()
    {
        float angle = Random.value * 2.0f * Mathf.PI;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    void RandomizeBoids()
    {
        for (int i = 0; i < boids.Length; i++)
        {
            boids[i].position = new Vector2(Random.Range(InsideBordersMin, InsideBordersMax), Random.Range(InsideBordersMin, InsideBordersMax));
            boids[i].direction = RandomDirection();
        }
    }

    void Start()
    {
        RandomizeBoids();
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(10000000.0f, 10000000.0f, 100000.0f);
        boidMesh = sphere.GetComponent<MeshFilter>().mesh;
    }
    
    void Update()
    {
        CalculateNextDirections();
        UpdateBoidData();
        Render();
    }

    void CalculateNextDirections()
    {
        for (int thisIndex = 0; thisIndex < boids.Length; thisIndex++)
        {
            var separationAmount = 0;
            var alignmentAmount = 0;
            var cohesionAmount = 0;
            var separation = Vector2.zero;
            var alignment = Vector2.zero;
            var cohesion = Vector2.zero;

            var thisPosition = boids[thisIndex].position;
            var thisDirection = boids[thisIndex].direction;

            for (int otherIndex = 0; otherIndex < boids.Length; otherIndex++)
            {
                if (otherIndex == thisIndex) continue;
                var otherPosition = boids[otherIndex].position;
                var distance = Vector2.Distance(thisPosition, otherPosition);
                if (distance < .001f) distance = .001f;

                if (distance < cohesionRadius)
                {
                    cohesionAmount++;
                    cohesion += otherPosition;
                }

                if (distance < separationRadius)
                {
                    separationAmount++;
                    separation += otherPosition;
                }

                if(distance < alignmentRadius)
                {
                    alignmentAmount++;
                    alignment += boids[otherIndex].direction;
                }
            }

            if(cohesionAmount > 0) cohesion = (cohesion / cohesionAmount) - thisPosition;
            if(alignmentAmount > 0) alignment = (alignment/alignmentAmount);
            if(separationAmount > 0) separation = -((separation / separationAmount) - thisPosition);

            var newDirection = (cohesion * cohesionWeight + alignment * alignmentWeight + separation * separationWeight);

            Vector2 nextDirection = (newDirection + thisDirection).normalized;
            
            if (thisPosition.x > InsideBordersMax)
            {
                nextDirection.x = Mathf.Lerp(nextDirection.x, Vector2.left.x, Mathf.InverseLerp(InsideBordersMax, playAreaHalfDimensions, thisPosition.x));
            }
            if (thisPosition.x < InsideBordersMin)
            {
                nextDirection.x = Mathf.Lerp(nextDirection.x, Vector2.right.x, Mathf.InverseLerp(InsideBordersMin, -playAreaHalfDimensions, thisPosition.x));
            }
            if (thisPosition.y > InsideBordersMax)
            {
                nextDirection.y = Mathf.Lerp(nextDirection.x, Vector2.down.y, Mathf.InverseLerp(InsideBordersMax, playAreaHalfDimensions, thisPosition.y));
            }
            if (thisPosition.y < InsideBordersMin)
            {
                nextDirection.y = Mathf.Lerp(nextDirection.y, Vector2.up.y, Mathf.InverseLerp(InsideBordersMin, -playAreaHalfDimensions, thisPosition.y));
            }

            nextFrameDirections[thisIndex] = nextDirection;
        }
    }

    void UpdateBoidData()
    {
        for (int i = 0; i < boids.Length; i++)
        {
            boids[i].position += nextFrameDirections[i] * Time.deltaTime * speed;
            boids[i].direction = nextFrameDirections[i];
        }
    }

    Vector3 scale = new Vector3(.1f, .1f, .1f);
    void Render()
    {
        for (int i = 0; i < boids.Length; i++)
        {
            var position = boids[i].position;
            boidMatrices[i] = Matrix4x4.Translate(new Vector3(position.x, .0f, position.y)) * Matrix4x4.Scale(scale);
        }
        Graphics.DrawMeshInstanced(boidMesh, 0, boidMaterial, boidMatrices);
    }
}
