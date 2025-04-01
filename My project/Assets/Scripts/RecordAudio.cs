using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordAudio : MonoBehaviour
{
    private AudioClip recordedClip;
    [SerializeField] AudioSource audioSource;
    
    public void StartRecording()
    {
        string device = Microphone.devices[0];
        int sampleRate = 44100;
        int lengthSec = 20;

        recordedClip = Microphone.Start(device, false, lengthSec, sampleRate);
    }

    public void PlayRecording()
    {
        audioSource.clip = recordedClip;
        audioSource.Play();
    }

    public void StopRecording()
    {
        Microphone.End(null);
    }
    
}
