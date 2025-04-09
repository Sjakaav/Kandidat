using System.IO;
using UnityEngine;

public static class SaveWave
{
    public static void Save(string filePath, AudioClip clip)
    {
        if (!filePath.EndsWith(".wav"))
        {
            filePath += ".wav";
        }

        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        byte[] wavData = ConvertToWav(clip, samples);

        File.WriteAllBytes(filePath, wavData);
        Debug.Log("✅ WAV file successfully saved at: " + filePath);
    }

    private static byte[] ConvertToWav(AudioClip clip, float[] samples)
    {
        int sampleRate = clip.frequency;
        int channels = clip.channels;
        int bitsPerSample = 16;
        int byteRate = sampleRate * channels * (bitsPerSample / 8);
        int dataLength = samples.Length * (bitsPerSample / 8); // Total byte size of PCM data

        using (MemoryStream memoryStream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(memoryStream))
        {
            // **Write WAV header**
            writer.Write("RIFF".ToCharArray());                  // Chunk ID
            writer.Write(36 + dataLength);                      // Chunk Size
            writer.Write("WAVE".ToCharArray());                 // Format
            writer.Write("fmt ".ToCharArray());                 // Subchunk1 ID
            writer.Write(16);                                   // Subchunk1 Size (PCM = 16)
            writer.Write((short)1);                             // Audio Format (PCM = 1)
            writer.Write((short)channels);                      // NumChannels
            writer.Write(sampleRate);                           // SampleRate
            writer.Write(byteRate);                             // ByteRate
            writer.Write((short)(channels * (bitsPerSample / 8))); // BlockAlign
            writer.Write((short)bitsPerSample);                 // BitsPerSample
            writer.Write("data".ToCharArray());                 // Subchunk2 ID
            writer.Write(dataLength);                           // Subchunk2 Size

            // **Write PCM audio data**
            foreach (var sample in samples)
            {
                short convertedSample = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
                writer.Write(convertedSample);
            }

            return memoryStream.ToArray();
        }
    }
}
