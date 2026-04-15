using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class AudioItem
{
    [Tooltip("调用该音频时使用的名称字符串")]
    public string name;
    public AudioClip clip;
    
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;
    public AudioMixerGroup mixerGroup;

    [Header("3D Settings (3D音效设置)")]
    [Tooltip("0代表全局2D音效，1代表完全3D空间音效")]
    [Range(0f, 1f)] public float spatialBlend = 0f; 
    [Tooltip("声音开始衰减的最小距离")]
    public float minDistance = 1f;
    [Tooltip("声音完全听不到的最大距离")]
    public float maxDistance = 50f;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
}

[CreateAssetMenu(fileName = "NewAudioConfig", menuName = "Audio/Audio Settings")]
public class AudioConfigSO : ScriptableObject
{
    [Tooltip("在此统一配置所有音频（BGM和SFX）")]
    public List<AudioItem> audioItems = new List<AudioItem>();
}