using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649

public class LookAt : MonoBehaviour
{
    [SerializeField]
    Transform target;

    [SerializeField]
    float radius;

    [SerializeField]
    float elevation;

    [SerializeField]
    float speed;

    float currentAngle = .0f;

    void Update()
    {
        currentAngle += Time.deltaTime * speed;

        float x = Mathf.Cos(currentAngle);
        float y = Mathf.Sin(currentAngle);
        transform.position = new Vector3(x*radius, target.position.y + elevation, y*radius);
        transform.LookAt(target, Vector3.up);
    }
}
