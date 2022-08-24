using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] [Range(0f, 1f)] float masterVolumnSFX = 0.5f;
    [SerializeField] [Range(0f, 1f)] float masterVolumnVoice = 0.5f;

    private bool isPlayingVoice = false;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SFXPlay(string sfxName, AudioClip clip)
    {
        GameObject child = new GameObject(sfxName + "Sound");
        AudioSource audioSource = child.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = masterVolumnSFX;
        audioSource.Play();

        Destroy(child, clip.length);
    }
    public void VoicePlay(string voiceName, AudioClip clip)
    {
        if(isPlayingVoice){ return; }
        GameObject child = new GameObject(voiceName + "Sound");
        AudioSource audioSource = child.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = masterVolumnSFX;
        audioSource.Play();

        Destroy(child, clip.length);
        StartCoroutine(BlockPlay(clip.length));
    }

    IEnumerator BlockPlay(float length)
    {
        isPlayingVoice = true;
        yield return new WaitForSeconds(length);
        isPlayingVoice = false;
    }
}
