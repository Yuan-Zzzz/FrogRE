using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Configuration")]
    [Tooltip("将配置好的AudioConfigSO拖入此处")]
    public AudioConfigSO audioConfig;
    
    [Header("Pool Settings")]
    [Tooltip("初始生成的音效播放器数量（对象池）")]
    public int initialSFXPoolSize = 10;

    private Dictionary<string, AudioItem> audioDict = new Dictionary<string, AudioItem>();
    private AudioSource bgmSource;
    private List<AudioSource> sfxPool = new List<AudioSource>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("Awake");
            transform.SetParent(null); // 确保在根节点
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (Instance != this)
        {
            // 如果旧的 AudioManager 没绑定配置，把这个新配置塞给它
            if (Instance.audioConfig == null && this.audioConfig != null)
            {
                Debug.Log($"[AudioManager] 正在用新场景的配置覆写旧有 AudioManager 的空配置。");
                Instance.audioConfig = this.audioConfig;
                Instance.Initialize();
            }
            // 不要选着它点运行就能彻底避免 Inspector OnDisable 错误
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        if (audioConfig != null)
        {
            audioDict.Clear(); // 防止重复执行时重复添加
            foreach (var item in audioConfig.audioItems)
            {
                if (!audioDict.ContainsKey(item.name))
                {
                    audioDict.Add(item.name, item);
                }
            }
            Debug.Log($"[AudioManager] 成功读取配置字典，包含 {audioDict.Count} 个音频信息。");
        }
        else
        {
            Debug.LogError("[AudioManager] 严重错误：AudioConfigSO 没有拖拽赋值给 AudioManager！");
        }

        // 初始化背景音乐播放通道
        if (bgmSource == null) // 防止重复生成
        {
            GameObject bgmGo = new GameObject("BGM_Source");
            bgmGo.transform.SetParent(this.transform);
            bgmSource = bgmGo.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.spatialBlend = 0f; // BGM 始终为 2D
        }

        // 初始化音效对象池
        for (int i = 0; i < initialSFXPoolSize; i++)
        {
            CreateNewSFXSource();
        }
    }

    private AudioSource CreateNewSFXSource()
    {
        GameObject go = new GameObject("SFX_Source");
        go.transform.SetParent(transform);
        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        sfxPool.Add(source);
        return source;
    }

    private AudioSource GetAvailableSFXSource()
    {
        // 清理空引用（如果附着在其他物体上的声音由于该物体被销毁，导致Source丢失）
        sfxPool.RemoveAll(s => s == null);

        foreach (var source in sfxPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        // 如果池子里的声音当前都在播放中，则自动扩容
        return CreateNewSFXSource();
    }

    private AudioItem GetAudioItem(string name)
    {
        if (audioDict.TryGetValue(name, out AudioItem item))
        {
            return item;
        }
        Debug.LogWarning($"[AudioManager] Audio item '{name}' not found in AudioConfigSO!");
        return null;
    }

    private void ApplySettingsToSource(AudioSource source, AudioItem item)
    {
        if (item.clip == null)
        {
            Debug.LogWarning($"[AudioManager] {item.name} 的 AudioClip 为空！请检查 AudioConfigSO 中是否正确绑定了音乐文件。");
        }
        
        source.clip = item.clip;
        source.volume = item.volume;
        source.pitch = item.pitch;
        source.loop = item.loop;
        source.outputAudioMixerGroup = item.mixerGroup;
        source.spatialBlend = item.spatialBlend;
        source.minDistance = item.minDistance;
        source.maxDistance = item.maxDistance;
        source.rolloffMode = item.rolloffMode;
    }

    // ================= PUBLIC API (外部调用接口) =================

    /// <summary>
    /// 播放背景音乐 (2D全局)
    /// </summary>
    public void PlayBGM(string name)
    {
        AudioItem item = GetAudioItem(name);
        if (item == null) return;

        ApplySettingsToSource(bgmSource, item);
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    /// <summary>
    /// 播放 2D 系统/UI音效（如按键、获得金币等）
    /// </summary>
    public void PlaySFX(string name)
    {
        AudioItem item = GetAudioItem(name);
        if (item == null) return;

        AudioSource source = GetAvailableSFXSource();
        source.transform.SetParent(transform); 
        source.transform.localPosition = Vector3.zero;
        
        ApplySettingsToSource(source, item);
        source.spatialBlend = 0f; // 强制2D播放
        source.Play();
    }

    /// <summary>
    /// 播放静态的 3D 空间音效（如爆炸等固定位置发出的声音）
    /// </summary>
    public void PlaySFX3D(string name, Vector3 position)
    {
        AudioItem item = GetAudioItem(name);
        if (item == null) return;

        AudioSource source = GetAvailableSFXSource();
        source.transform.SetParent(transform);
        source.transform.position = position;

        ApplySettingsToSource(source, item);
        source.Play();
    }

    /// <summary>
    /// 播放跟随物体的 3D 空间音效（如怪物持续走动的脚步声、角色的持续施法等）
    /// 会将音源挂载到目标 Transform 上
    /// </summary>
    public void PlaySFXAttached(string name, Transform target)
    {
        AudioItem item = GetAudioItem(name);
        if (item == null) return;

        AudioSource source = GetAvailableSFXSource();
        source.transform.SetParent(target);
        source.transform.localPosition = Vector3.zero;

        ApplySettingsToSource(source, item);
        source.Play();
        
        StartCoroutine(ReturnToPoolCoroutine(source));
    }

    // 协程：当跟随式音效播放完毕后，将其归还回 AudioManager 节点下
    private IEnumerator ReturnToPoolCoroutine(AudioSource source)
    {
        while (source != null && source.isPlaying)
        {
            yield return null;
        }

        // 如果播放完了或者被停止了，把父节点设置回来，以免跟随的物体销毁时带走音源
        if (source != null)
        {
            source.transform.SetParent(transform);
            source.transform.localPosition = Vector3.zero;
        }
    }
}