using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }
    
    [Header("Bullet Time Settings")]
    [SerializeField] private float bulletTimeScale = 0.2f;
    [SerializeField] private float transitionSpeed = 8f;
    
    [Header("VFX Settings")]
    [SerializeField] private float disperseIntensity = 1f;
    [SerializeField] private float disperseTransitionSpeed = 10f;
    
    private float currentTargetScale = 1f;
    private float currentTimeScale = 1f;
    private float currentDisperse;
    
    private Volume volume;
    private ChromaticAberration chromaticAberration;
    
    public float TimeScale => currentTimeScale;
    public bool IsBulletTime => currentTargetScale < 1f;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        FindVolumeEffect();
    }
    
    void FindVolumeEffect()
    {
        var volumes = FindObjectsOfType<Volume>();
        foreach (var v in volumes)
        {
            if (v.profile != null && v.profile.TryGet(out ChromaticAberration ca))
            {
                volume = v;
                chromaticAberration = ca;
                return;
            }
        }
    }
    
    void Update()
    {
        bool isHoldingLeftButton = Input.GetMouseButton(0);
        
        currentTargetScale = isHoldingLeftButton ? bulletTimeScale : 1f;
        currentTimeScale = Mathf.Lerp(currentTimeScale, currentTargetScale, transitionSpeed * Time.deltaTime);
        
        Time.timeScale = currentTimeScale;
        Time.maximumDeltaTime = Mathf.Max(0.03333f, 0.1f * currentTimeScale);
        
        float targetDisperse = isHoldingLeftButton ? disperseIntensity : 0f;
        currentDisperse = Mathf.Lerp(currentDisperse, targetDisperse, disperseTransitionSpeed * Time.deltaTime);
        
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = currentDisperse;
        }
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Time.timeScale = 1f;
            Time.maximumDeltaTime = 0.1f;
            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.value = 0f;
            }
        }
    }
}