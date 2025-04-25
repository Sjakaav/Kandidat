using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RecordAudio : MonoBehaviour
{
    private AudioClip recordedClip;
    [SerializeField] AudioSource audioSource;
    public GameObject recordButton;
    public GameObject stopButton;

    private string device;
    private int sampleRate = 44100;
    private int maxLengthSec = 20; // Maximum recording length
    private float startRecordingTime;

    private void Start()
    {
        stopButton.SetActive(false);
    }

    public void StartRecording()
    {
        device = Microphone.devices[0];
        recordedClip = Microphone.Start(device, false, maxLengthSec, sampleRate);

        // Record time starts from here
        startRecordingTime = Time.time;

        recordButton.SetActive(false);
        stopButton.SetActive(true);
    }

    public void StopRecording()
    {
        // End the microphone recording manually
        Microphone.End(device);

        // Optionally, you can stop when the maximum length is reached too
        if (Time.time - startRecordingTime >= maxLengthSec)
        {
            Debug.Log("Recording automatically stopped after reaching max length.");
        }

        // Get the actual length of the recording (how much audio was actually recorded)
        int recordedSamples = Mathf.FloorToInt((Time.time - startRecordingTime) * sampleRate);
        if (recordedSamples > recordedClip.samples)
        {
            recordedSamples = recordedClip.samples; // Prevent out of bounds
        }

        // Create a new AudioClip with the recorded data only
        AudioClip trimmedClip = AudioClip.Create("TrimmedClip", recordedSamples, recordedClip.channels, recordedClip.frequency, false);
        float[] trimmedData = new float[recordedSamples];
        recordedClip.GetData(trimmedData, 0);
        trimmedClip.SetData(trimmedData, 0);

        // Now we have the trimmed clip
        recordedClip = trimmedClip;

        recordButton.SetActive(true);
        stopButton.SetActive(false);

        // Save the trimmed clip
        SaveRecording();
    }

    private void Update()
    {
        // Automatically stop recording after maxLengthSec seconds
        if (recordedClip != null && Microphone.IsRecording(device) && Time.time - startRecordingTime >= maxLengthSec)
        {
            StopRecording();
        }
    }

    public void PlayRecording()
    {
        audioSource.clip = recordedClip;
        audioSource.Play();
    }

    private void SaveRecording()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "recording.wav");
        SaveWave.Save(filePath, recordedClip);

        if (File.Exists(filePath))
        {
            Debug.Log("✅ File successfully saved at: " + filePath);
            FindObjectOfType<SocketManager>().SendAudioToServer();
        }
        else
        {
            Debug.LogError("❌ Failed to save file at: " + filePath);
        }
    }
}
