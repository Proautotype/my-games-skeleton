using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Start is called before the first frame update
    public List<SubAudio> audioList;
    public static AudioManager instance;

    public IEnumerator PlayAudio(int index, int delay)
    {
        if (index >= 0 && index < audioList.Count)
        {
            yield return new WaitForSeconds(delay);
            SubAudio subAudio = audioList[index];
            subAudio.audioSource.clip = subAudio.audioClip;
            subAudio.audioSource.loop = subAudio.loop;
            subAudio.audioSource.volume = subAudio.volume;
            subAudio.audioSource.mute= subAudio.mute;
            subAudio.audioSource.Play();
        }
    }

    public void PlaySimpleAudio(int index)
    {
        if (index >= 0 && index < audioList.Count)
        {
            SubAudio subAudio = audioList[index];
            subAudio.audioSource.clip = subAudio.audioClip;
            subAudio.audioSource.loop = subAudio.loop;
            subAudio.audioSource.volume = subAudio.volume;
            subAudio.audioSource.mute = subAudio.mute;
            subAudio.audioSource.Play();
        }
    }

    public void StopAudio(int index)
    {
        if (index >= 0 && index < audioList.Count)
        {
            SubAudio subAudio = audioList[index];
            subAudio.audioSource.Stop();
        }
    }

    public void SetVolume(int index, float volume)
    {
        if (index >= 0 && index < audioList.Count)
        {
            SubAudio subAudio = audioList[index];
            subAudio.volume = Mathf.Clamp01(volume);
            subAudio.audioSource.volume = subAudio.volume;
        }
    }

}

[System.Serializable]
public class SubAudio
{
    public AudioClip audioClip;
    public AudioSource audioSource;
    [Range(0, 1)]
    public float volume = 0.2f;
    public bool loop = false;
    public string name;
    public bool mute;
}

