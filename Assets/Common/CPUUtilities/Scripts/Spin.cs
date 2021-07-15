using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 //disable Serializefield private warnings


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
