using ToolTip;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DragAndDrop : MonoBehaviour
{
    [SerializeField] private float clickScaleMultiplier = 1.1f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float maxRotationAngle = 30f;
    [SerializeField] private BorderParticleScatter scatter;

    private Vector3 _originalScale;
    private Vector3 _mouseOffset;
    private bool _isDragging;
    private float _currentRotation;
    private Vector3 _previousPosition;
    private Camera _mainCamera;
    private SpriteRenderer _spriteRenderer;
    
    public bool IsDragging() => _isDragging;
    
    [SerializeField] private AudioClip pickUpSound;
    [SerializeField] private AudioClip putSound;
    private AudioSource _audioSource;
    
    [SerializeField] private bool clampToScreen = true;
    [SerializeField] private float screenPadding = 0.5f;
    
    private Vector2 _screenMin;
    private Vector2 _screenMax;
    private Vector2 _objectHalfSize;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _mainCamera = Camera.main;
        _originalScale = transform.localScale;
        
        if (GetComponent<Collider2D>() == null)
            Debug.LogError("Требуется Collider2D для перетаскивания!");
        
        CalculateScreenBounds();
        CalculateObjectSize();
    }
    
    private void CalculateScreenBounds()
    {
        Vector3 bottomLeft = _mainCamera.ViewportToWorldPoint(Vector3.zero);
        Vector3 topRight = _mainCamera.ViewportToWorldPoint(Vector3.one);
        
        _screenMin = new Vector2(bottomLeft.x + screenPadding, bottomLeft.y + screenPadding);
        _screenMax = new Vector2(topRight.x - screenPadding, topRight.y - screenPadding);
    }

    private void CalculateObjectSize()
    {
        Collider2D col = GetComponent<Collider2D>();
        _objectHalfSize = col.bounds.extents;
    }

    private void OnMouseDown()
    {
        if (!IsClickOnObject()) return;
        
        GameManager.Instance.SetSomethingDragging(true);
        
        _audioSource.PlayOneShot(pickUpSound);

        _spriteRenderer.sortingOrder += 1;
        
        TooltipScreenSpaceUI.HideTooltip_Static();

        _isDragging = true;
        transform.localScale = _originalScale * clickScaleMultiplier;
        
        Vector3 mouseWorldPos = GetMouseWorldPos();
        _mouseOffset = transform.position - mouseWorldPos;
        _previousPosition = mouseWorldPos;
        
        scatter.EmitParticles(_spriteRenderer);
    }

    private void OnMouseDrag()
    {
        if (!_isDragging) return;

        UpdatePosition();
        UpdateRotation();
    }

    private void OnMouseUp()
    {
        if (!_isDragging) return;
        
        GameManager.Instance.SetSomethingDragging(false);
        
        _audioSource.PlayOneShot(putSound);
        
        _spriteRenderer.sortingOrder -= 1;

        _isDragging = false;
        transform.localScale = _originalScale;
        transform.rotation = Quaternion.identity;
        scatter.ManuallyStopScattering();
        scatter.EmitParticles(_spriteRenderer);
    }

    private bool IsClickOnObject()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        return hit.collider != null && hit.transform == transform;
    }

    private void UpdatePosition()
    {
        Vector3 newPosition = GetMouseWorldPos() + _mouseOffset;
        
        if (clampToScreen)
        {
            newPosition.x = Mathf.Clamp(
                newPosition.x, 
                _screenMin.x + _objectHalfSize.x, 
                _screenMax.x - _objectHalfSize.x
            );
            
            newPosition.y = Mathf.Clamp(
                newPosition.y, 
                _screenMin.y + _objectHalfSize.y, 
                _screenMax.y - _objectHalfSize.y
            );
        }
        
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
    }

    private void UpdateRotation()
    {
        Vector3 currentMousePos = GetMouseWorldPos();
        float moveDelta = currentMousePos.x - _previousPosition.x;
        _previousPosition = currentMousePos;

        float targetRotation = Mathf.Clamp(moveDelta * rotationSpeed, -maxRotationAngle, maxRotationAngle);
        _currentRotation = Mathf.Lerp(_currentRotation, targetRotation, Time.deltaTime * 10f);
        
        transform.rotation = Quaternion.Euler(0, 0, -_currentRotation);
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = _mainCamera.transform.position.z * -1;
        return _mainCamera.ScreenToWorldPoint(mousePos);
    }
}
