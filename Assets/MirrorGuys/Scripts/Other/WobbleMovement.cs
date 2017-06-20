using UnityEngine;
using System.Collections;

public class WobbleMovement : MonoBehaviour
{

    public Vector3 distance;
    public Vector3 frequency;
    public Vector3 phaseOffset;
    Vector3 delta;
    private float _t;

    public bool rotation = false;
    public Vector3 localEulerAngleDist;
    public Vector3 localRotationFreq;
    public Vector3 localRotationPhase;
    Vector3 deltaRot;

    void Update()
    {
        _t += Time.deltaTime;

        transform.localPosition -= delta;

        delta = new Vector3(
            distance.x * Mathf.Sin((frequency.x * _t + phaseOffset.x) * Mathf.PI),
            distance.y * Mathf.Sin((frequency.y * _t + phaseOffset.y) * Mathf.PI),
            distance.z * Mathf.Sin((frequency.z * _t + phaseOffset.z) * Mathf.PI)
        );

        transform.localPosition += delta;

        if (rotation) {
            transform.localEulerAngles -= deltaRot;

            deltaRot = new Vector3(
                localEulerAngleDist.x * Mathf.Sin((localRotationFreq.x * _t + localRotationPhase.x) * Mathf.PI),
                localEulerAngleDist.y * Mathf.Sin((localRotationFreq.y * _t + localRotationPhase.y) * Mathf.PI),
                localEulerAngleDist.z * Mathf.Sin((localRotationFreq.z * _t + localRotationPhase.z) * Mathf.PI)
            );

            transform.localEulerAngles += deltaRot;
        }
    }
}
