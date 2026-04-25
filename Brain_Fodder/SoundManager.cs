using Brain_Fodder;
using NAudio.Wave;

class SoundManager
{
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
    public static short[] GenerateCelebrationSound()
    {
        int sampleRate = 44100;
        double duration = 2.0;
        int totalSamples = (int)(sampleRate * duration);
        float[] buffer = new float[totalSamples];

        // 1. The Start: Rising Arpeggio (C Major)
        int[] arpeggio = { 72, 76, 79 };
        double[] arpeggioStarts = { 0.0, 0.15, 0.3 };

        // 2. The Finish: Major Chord (C, E, G, C)
        int[] chord = { 72, 76, 79, 84 };

        for (int i = 0; i < totalSamples; i++)
        {
            double time = (double)i / sampleRate;
            float sampleValue = 0;

            // --- PLAY ARPEGGIO ---
            for (int n = 0; n < arpeggio.Length; n++)
            {
                if (time >= arpeggioStarts[n] && time < arpeggioStarts[n] + 0.15)
                {
                    double localTime = time - arpeggioStarts[n];
                    sampleValue += GenerateBellWave(arpeggio[n], localTime) * 0.4f;
                }
            }

            // --- PLAY CHORD FINALE ---
            if (time >= 0.6 && time < duration)
            {
                double localTime = time - 0.6;
                // Longer decay for the finale to let it ring out
                float decay = (float)Math.Exp(-2.0 * localTime);

                foreach (int note in chord)
                {
                    sampleValue += GenerateBellWave(note, localTime) * decay * 0.3f;
                }
            }

            buffer[i] = sampleValue * 0.15f; // Safe total volume
        }

        // Convert to short
        short[] audioData = new short[totalSamples];
        for (int i = 0; i < totalSamples; i++)
        {
            audioData[i] = (short)(Math.Clamp(buffer[i], -1.0f, 1.0f) * short.MaxValue);
        }
        return audioData;
    }

    // The "Golden" Synthesis: Fundamental + Octave + Fifth
    // This creates the chime sound without the "beepy" sine wave feel.
    private static float GenerateBellWave(int midiNote, double time)
    {
        double freq = GetFrequency(midiNote);
        double phase = Math.Tau * freq * time;

        // Fundamental (1.0) + Octave (0.5) + Fifth (0.25)
        float bell = (float)(Math.Sin(phase) * 1.0 +
                             Math.Sin(phase * 2.0) * 0.5 +
                             Math.Sin(phase * 3.0) * 0.25);

        // Sharp Attack (prevent clicking) + Exponential Decay
        float envelope = (float)(Math.Min(1.0, time / 0.01) * Math.Exp(-5.0 * time));

        return bell * envelope;
    }
}