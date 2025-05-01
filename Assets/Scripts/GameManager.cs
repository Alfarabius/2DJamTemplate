using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    [SerializeField] private List<AudioClip> hoverSounds;
    
    private int _hoverSoundIndex = 0;

    private bool _isSomethingDragging;
    private bool _isCursorHovered;
    
    public bool IsSomethingDragging() => _isSomethingDragging;
    public bool IsCursorHovered() => _isCursorHovered;

    public void SetSomethingDragging(bool state)
    {
        _isSomethingDragging = state;
    }

    public void SetCursorHovered(bool state)
    {
        _isCursorHovered = state;
    }

    public void PlayNextHoverSound(AudioSource audioSource)
    {
        _hoverSoundIndex = Mathf.Clamp(_hoverSoundIndex + 1, 0, hoverSounds.Count - 1);
        audioSource.PlayOneShot(hoverSounds[_hoverSoundIndex ]);
        if (_hoverSoundIndex == hoverSounds.Count - 1)
            _hoverSoundIndex = 0;
    }
    
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    _instance = obj.AddComponent<GameManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
