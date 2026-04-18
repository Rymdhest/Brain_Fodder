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
    public static double GetFrequency(int midiNote)
    {
        // 69 is the MIDI note for A4 (440Hz)
        // The formula: f = 440 * 2^((n-69)/12)
        return 440.0 * Math.Pow(2.0, (midiNote - 69) / 12.0);
    }


    public static short[]  GenerateSound(int step)
    {
        int sampleRate = 44100;
        double duration = 1.0;
        int totalSamples = (int)(sampleRate * duration);
        short[] audioData = new short[totalSamples];

        // Pick a note
        //int[] scale = { 60, 62, 64, 65, 67, 69, 71 };
        int[] scale = { 60, 62, 63, 65, 67, 68, 70 };

        double freq = GetFrequency(scale[Math.Clamp(step, 0, 6)]);

        freq /= 1.0;

        // Pre-calculate phase steps for efficiency
        double phaseStep = Math.Tau * freq / sampleRate;
        double phase = 0;

        for (int i = 0; i < totalSamples; i++)
        {
            double time = (double)i / sampleRate;

            // 1. Advance the fundamental phase
            phase += phaseStep;

            // 2. Additive Synthesis with Harmonic-Specific Decay
            double sampleValue = 0;
            // Fundamental
            sampleValue += 1.0 * Math.Sin(phase) * Math.Exp(-2.0 * time);
            // 2nd Harmonic (Octave) - Decays faster
            sampleValue += 0.5 * Math.Sin(phase * 2.0) * Math.Exp(-4.0 * time);
            // 3rd Harmonic - Decays even faster
            sampleValue += 0.2 * Math.Sin(phase * 3.01) * Math.Exp(-6.0 * time);

            // 10ms Fade In
            double attack = Math.Min(1.0, time / 0.01);
            // 50ms Fade Out
            double release = Math.Min(1.0, (duration - time) / 0.05);

            // Apply both
            double finalEnvelope = attack * release * Math.Exp(-3.0 * time);

            double output = sampleValue * finalEnvelope * 0.5;
            audioData[i] = (short)(Math.Clamp(output, -1.0, 1.0) * short.MaxValue);
        }
        //Console.WriteLine("Generated sound with frequency: " + freq + " Hz");
        return audioData;
    }
}