using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using OpenTK.Audio.OpenAL;
using SpaceEngine.Util;
using System.Runtime.InteropServices;

class SoundManager
{
    private static List<short> recordingBuffer = new List<short>();
    public static bool IsRecording = false;

    private static WaveOutEvent outputDevice = new WaveOutEvent();
    private static NAudio.Wave.SampleProviders.MixingSampleProvider mixer;

    public SoundManager()
    {
        var format = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);
        mixer = new NAudio.Wave.SampleProviders.MixingSampleProvider(format)
        {
            ReadFully = true // Keeps the mixer playing even when no sounds are active
        };
        outputDevice.Init(mixer);
        outputDevice.Play();
    }

    public static void Play(short[] soundData)
    {
        var ms = new MemoryStream();
        var writer = new BinaryWriter(ms);

        foreach (var sample in soundData)
        {
            writer.Write(sample);
        }

        ms.Position = 0;

        // 2. Create the raw stream (16-bit Mono)
        var rawStream = new RawSourceWaveStream(ms, new WaveFormat(44100, 16, 1));

        // 3. Convert the 16-bit integer stream into a 32-bit Float stream
        var floatStream = new Wave16ToFloatProvider(rawStream);

        // 4. Push the float stream directly into the mixer!
        mixer.AddMixerInput(floatStream);

        // 5. Save it to our export buffer for the final 30-sec video
        if (IsRecording)
        {
            recordingBuffer.AddRange(soundData);
        }
    }

    public static void update(float delta)
    {
    }


    public static short[] GenerateLaser()
    {
        int sampleRate = 44100;
        double duration = 1.0; // 0.3 seconds long
        int totalSamples = (int)(sampleRate * duration);
        short[] audioData = new short[totalSamples];

        double volume = 1.0;
        double freq = 0;
        float rand = MyMath.rng();
        if (rand > 0.8) freq = 261.63;
        else if (rand > 0.6) freq = 329.63;
        else if (rand > 0.4) freq = 392.0;
        else if (rand > 0.2) freq = 440.0;
        freq *= 1.0;
        double wave = 0;
        for (int i = 0; i < totalSamples; i++)
        {
            double progress = (double)i / totalSamples;
            double time = (double)i / sampleRate;

            wave += 1.0 * Math.Sin(Math.Tau * freq * 1 * time);
            wave += 0.8 * Math.Sin(Math.Tau * freq * 2 * time);
            wave += 0.6 * Math.Sin(Math.Tau * freq * 3 * time);
            wave += 0.4 * Math.Sin(Math.Tau * freq * 6 * time);

            wave /= 2.8; 

            double envelope = Math.Exp(-4.0 * progress);
            audioData[i] = (short)(wave * short.MaxValue * volume * envelope);
        }
        return audioData;
    }
}