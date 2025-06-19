using UnityEngine;

[DisallowMultipleComponent]
public class Rotator : MonoBehaviour
{
    [Tooltip("Vitesse de rotation en degrés/sec")]
    public float degreesPerSecond = 180f;  // 0.5 tour/sec

    void Update()
    {
        transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime, Space.Self);
    }
}