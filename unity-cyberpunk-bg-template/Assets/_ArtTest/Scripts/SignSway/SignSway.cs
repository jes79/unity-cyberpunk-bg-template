using UnityEngine;

public class SignSway : MonoBehaviour
{
    public float swaySpeed = 1f;
    public float swayAngle = 3f;      // 도 단위
    public Vector3 swayAxis = Vector3.up;

    private Quaternion _restRotation;
    private float _phase;

    private void Awake()
    {
        _restRotation = transform.localRotation;
        // 피봇 월드 좌표 기반 위상차 — 여러 간판이 다 같이 흔들리지 않게
        _phase = (transform.position.x * 12.9898f + transform.position.z * 78.233f) % 100f;
    }

    private void Update()
    {
        float angle = Mathf.Sin(Time.time * swaySpeed + _phase) * swayAngle;
        transform.localRotation = _restRotation * Quaternion.AngleAxis(angle, swayAxis);
    }
}