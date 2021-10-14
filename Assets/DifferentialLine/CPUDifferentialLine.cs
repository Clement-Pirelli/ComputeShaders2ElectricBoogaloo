using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPUDifferentialLine : MonoBehaviour
{
    class Point 
    {
        public Vector2 position;
        public uint previousPoint;
        public uint nextPoint;
        public const uint invalidPoint = ~0u;

        public Point(Vector2 position, uint previousPoint, uint nextPoint) 
        {
            this.position = position;
            this.previousPoint = previousPoint;
            this.nextPoint = nextPoint;
        }
    }

    Texture2D texture;

    List<Point> readPoints;
    List<Point> writePoints;

    List<Point> addedPoints;
    [SerializeField]
    uint startingCount = 1000;

    [SerializeField]
    float startingRadius = 100.0f;

    [SerializeField]
    float minimumNeighborDistance = .1f;
    [SerializeField]
    float repulsionRadius = .3f;
    [SerializeField]
    float movementSpeed = .01f;

    LineRenderer lineRenderer;

    [SerializeField]
    int addEverySecond = 5;
    float add = .0f;

    void InitList(ref List<Point> points) 
    {
        points = new List<Point>((int)startingCount);
        for(uint i = 0; i < startingCount; i++) 
        {
            float angle = (float)i / startingCount * 2.0f * 3.14f;
            Vector2 position = new Vector2(Mathf.Cos(angle) * startingRadius, Mathf.Sin(angle) * startingRadius);
            uint next = (i + 1) % startingCount;
            uint previous = i - 1;
            if (i == 0) previous = startingCount - 1;
            points.Add(new Point(position, previous, next));
        }
    }

    Vector2 KeepWithinDistance(Vector2 point, Vector2 other, float minDistance) 
    {
        Vector2 delta = point - other;
        float magnitude = delta.magnitude;
        if(magnitude > minDistance) 
        {
            return other + delta / magnitude * minDistance;
        }
        return point;
    }

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        InitList(ref readPoints);
        InitList(ref writePoints);
    }

    // Update is called once per frame
    void Update()
    {
        AddPoints();
        MovePoints();
        Render();
        (readPoints, writePoints) = (writePoints, readPoints);
    }


    void AddPointToList(ref List<Point> points, int chosenIndex) 
    {
        uint newIndex = (uint)points.Count;
        int nextIndex = (int)points[chosenIndex].nextPoint;
        Vector2 position = Vector2.Lerp(points[chosenIndex].position, points[nextIndex].position, .5f);
        
        points[chosenIndex] = new Point(points[chosenIndex].position, points[chosenIndex].previousPoint, newIndex);
        points[nextIndex] = new Point(points[nextIndex].position, newIndex, points[nextIndex].nextPoint);

        points.Add(new Point(position, (uint)chosenIndex, (uint)nextIndex));
    }

    void AddPoints() 
    {
        add += Time.deltaTime * addEverySecond;
        while(add >= 1.0f) 
        {
            add -= 1.0f;
            int chosenIndex = Random.Range(0, readPoints.Count);
            AddPointToList(ref readPoints, chosenIndex);
            AddPointToList(ref writePoints, chosenIndex);
        }
    }

    void MovePoints() 
    {
        foreach(Point currentPoint in readPoints)
        {
            Point previous = readPoints[(int)currentPoint.previousPoint];
            Point next = readPoints[(int)currentPoint.nextPoint];
            //non-neighbor nodes, want to keep their distance
            Vector2 repulsionMovement = Vector2.zero;
            for (uint i = 0; i < readPoints.Count; i++)
            {
                if (i == previous.nextPoint //hack 'cause we're in a foreach
                    || i == currentPoint.previousPoint
                    || i == currentPoint.nextPoint)
                {
                    continue;
                }

                Point other = readPoints[(int)i];
                Vector2 delta = currentPoint.position - other.position;
                float deltaLength = delta.magnitude;
                if (deltaLength > float.Epsilon && deltaLength <= repulsionRadius)
                {
                    repulsionMovement += delta / deltaLength;
                }
            }

            float movementLength = repulsionMovement.magnitude;
            if (movementLength > float.Epsilon)
            {
                Point newPoint = new Point(currentPoint.position + repulsionMovement / movementLength * movementSpeed * Time.deltaTime, currentPoint.previousPoint, currentPoint.nextPoint);
                newPoint.position = KeepWithinDistance(newPoint.position, next.position, minimumNeighborDistance);
                newPoint.position = KeepWithinDistance(newPoint.position, previous.position, minimumNeighborDistance);
                writePoints[(int)previous.nextPoint] = newPoint;
            }
        }
    }

    void Render() 
    {
        lineRenderer.positionCount = readPoints.Count + 1;
        Point p = readPoints[0];
        for (int i = 0; i < readPoints.Count; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(p.position.x, .0f, p.position.y));
            if (p.nextPoint == Point.invalidPoint)
            {
                break;
            }

            if (p.nextPoint == 0)
            {
                p = readPoints[(int)p.nextPoint];
                lineRenderer.SetPosition(i + 1, new Vector3(p.position.x, .0f, p.position.y));
                break;
            }

            p = readPoints[(int)p.nextPoint];
        }
    }
}