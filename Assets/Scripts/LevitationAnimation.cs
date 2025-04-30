using UnityEngine;

public class SpriteLevitation : MonoBehaviour
{
    [SerializeField] private float levitationHeight = 0.5f;
    [SerializeField] private float levitationDistanceX = 0.2f;
    [SerializeField] private float levitationSpeedX = 2.0f;
    [SerializeField] private float levitationSpeedY = 1.0f;
    [SerializeField] private float randomOffsetMagnitude = 0.05f;
    [SerializeField] private float lerpSpeed = 5.0f;

    private Vector3 _initialPosition;
    private Vector3 _targetPosition;

    private void Start()
    {
        _initialPosition = transform.localPosition;
        _targetPosition = CalculateNewTargetPosition();
    }

    private void Update()
    {
        _targetPosition = CalculateNewTargetPosition();
        transform.localPosition = Vector3.Lerp(transform.localPosition, _targetPosition, Time.deltaTime * lerpSpeed);
    }

    private Vector3 CalculateNewTargetPosition()
    {
        float offsetX = Mathf.Sin(Time.time * levitationSpeedX) * levitationDistanceX;
        float offsetY = Mathf.Cos(Time.time * levitationSpeedY) * levitationHeight;

        float randomOffsetX = Random.Range(randomOffsetMagnitude, randomOffsetMagnitude * 2f);
        float randomOffsetY = Random.Range(randomOffsetMagnitude, randomOffsetMagnitude * 2f);

        return _initialPosition + new Vector3(offsetX + randomOffsetX, offsetY + randomOffsetY, 0f);
    }
}
