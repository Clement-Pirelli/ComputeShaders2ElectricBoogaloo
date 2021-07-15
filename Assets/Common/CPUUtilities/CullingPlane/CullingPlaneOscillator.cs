using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 //disable Serializefield private warnings

public class CullingPlaneOscillator : MonoBehaviour
{
    [SerializeField]
    float speed;
    [SerializeField]
    CullingPlane cullingPlane;

    float startPosition;
    private void Start()
    {
        startPosition = cullingPlane.cullingPlaneOrigin.y;
    }

    // Update is called once per frame
    void Update()
    {
        Math.Range<float> sinRange = new Math.Range<float>(-1f, 1f);
        Math.Range<float> endRange = new Math.Range<float>(.0f, Vector3.Distance(Vector3.zero, new Vector3(.5f,.5f,.5f)));
        float y = startPosition + Math.Remap(Mathf.Sin(Time.time * speed), sinRange, endRange);

        cullingPlane.cullingPlaneOrigin.y = y;
    }

    private void OnDestroy()
    {
        cullingPlane.cullingPlaneOrigin.y = startPosition;
    }
}
