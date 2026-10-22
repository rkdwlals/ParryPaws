using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // 따라갈 대상 (Player)
    public float smoothSpeed = 5f;    // 부드럽게 따라오는 속도
    public Vector3 offset;            // 카메라 위치 오프셋

    [Header("Camera Clamp")]
    public bool useClamp = false;     // 맵 제한 사용 여부
    public Vector2 minLimit;          // 최소 x, y
    public Vector2 maxLimit;          // 최대 x, y

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        if (useClamp)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minLimit.x, maxLimit.x);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minLimit.y, maxLimit.y);
        }

        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }
}

