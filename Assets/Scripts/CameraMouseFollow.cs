using UnityEngine;

public class CameraMouseFollow : MonoBehaviour
{
    [SerializeField] private float sensitivity = 0.5f;
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private float maxOffset = 2f;
    
    private Vector3 _initialPosition;
    private Vector3 _targetPosition;
    private Vector3 _velocity;

    private void Start()
    {
        _initialPosition = transform.position;
    }

    private void Update()
    {
        Vector3 mouseViewportPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        Vector3 mouseOffset = new Vector3(
            (mouseViewportPos.x - 0.5f) * 2f, 
            (mouseViewportPos.y - 0.5f) * 2f, 
            0
        );

        _targetPosition = _initialPosition + mouseOffset * sensitivity * maxOffset;
        _targetPosition.z = transform.position.z;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            _targetPosition,
            ref _velocity,
            smoothTime
        );
    }
}
