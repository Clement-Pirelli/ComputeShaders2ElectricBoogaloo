using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    [SerializeField]
    private float speed;


    private float angle;

    void Update()
    {
        angle += Time.deltaTime * speed;

        transform.rotation = Quaternion.Euler(new Vector3(.0f, angle, .0f));
    }
}
