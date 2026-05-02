using Brain_Fodder;
using NAudio.Midi;
using NAudio.Wave;

class SoundManager
{
    public static bool IsRecording = false;

    private static WaveOutEvent outputDevice = new WaveOutEvent();
    private static NAudio.Wave.SampleProviders.MixingSampleProvider mixer;
    private static List<List<int>> currentSongChords;
    private static readonly List<(string, int, int)> SongMelodyMap = new List<(string, int, int)>

{
    ("Avicii - ID (Levels)", 1, -1),
    ("eminem-stan", 3, -1),
    ("jingle-bells-keyboard", 1, 0),
    ("kalinka", 1, -1),
    ("Mario Bros. - Super Mario Bros. Theme", 0, -1),
    ("Zelda - Ocarina of Time - Lost Woods Theme", 1, -1),
};
    private static int currentSongShift = 0;
    private static int noteIndex = 0;
    private static float cooldownTimer = 0;
    private static float COOLDOWN_DURATION = 0.1f;
    
    public SoundManager()
    {
        var format = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);
        mixer = new NAudio.Wave.SampleProviders.MixingSampleProvider(format)
        {
            ReadFully = true // Keeps the mixer playing even when no sounds are active
        };
        outputDevice.Init(mixer);
        outputDevice.Play();
        var song = SongMelodyMap[5];
        currentSongChords = SoundManager.LoadSong(song.Item1, song.Item2, song.Item3);
    }

    public static List<List<int>> LoadSong(string fileName, int targetTrack, int shift)
    {
        currentSongShift = shift;
        var songData = new List<List<int>>();
        var midiFile = new NAudio.Midi.MidiFile("MIDI\\" + fileName+".mid", false);

        // --- NEW: SCAN ALL TRACKS ---
        Console.WriteLine($"\n--- MIDI Analysis: {fileName} ---");
        for (int i = 0; i < midiFile.Tracks; i++)
        {
            int noteCount = 0;
            string trackName = "Unnamed";

            foreach (var midiEvent in midiFile.Events[i])
            {
                // Count actual notes
                if (midiEvent is NAudio.Midi.NoteOnEvent n && n.Velocity > 0)
                    noteCount++;

                // Extract track name if available
                if (midiEvent is NAudio.Midi.TextEvent te && te.MetaEventType == NAudio.Midi.MetaEventType.SequenceTrackName)
                {
                    trackName = te.Text;
                }
            }

            // Color-code the targeted track in the console for clarity
            string indicator = (i == targetTrack) ? " >> SELECTED <<" : "";
            if (noteCount > 0 || i == targetTrack)
            {
                Console.WriteLine($"Track {i}: {trackName} ({noteCount} notes){indicator}");
            }
        }
        Console.WriteLine("------------------------------------------\n");
        // ----------------------------

        // Check if the track exists
        if (targetTrack >= midiFile.Tracks) targetTrack = 0;

        var groups = new SortedDictionary<long, List<int>>();
        var melodyEvents = midiFile.Events[targetTrack];

        foreach (var midiEvent in melodyEvents)
        {
            if (midiEvent is NAudio.Midi.NoteOnEvent noteOn && noteOn.Velocity > 0)
            {
                // Quantize to merge fast flurries
                long quantizedTime = (noteOn.AbsoluteTime / 10) * 10;

                if (!groups.ContainsKey(quantizedTime))
                    groups[quantizedTime] = new List<int>();

                groups[quantizedTime].Add(noteOn.NoteNumber);
            }
        }

        int lastNote = -1;
        foreach (var group in groups.Values)
        {
            int currentHighest = group.Max();

            // REMOVE OR COMMENT OUT THIS IF STATEMENT:
            // if (currentHighest != lastNote) 
            // {
            songData.Add(group);
            lastNote = currentHighest;
            // }
        }

        // This shows the count AFTER filtering/quantizing
        Console.WriteLine($"Loaded Track {targetTrack}: Resulted in {songData.Count} unique bounce events.");
        return songData;
    }

    public void Update(float deltaTime)
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= deltaTime;
        }
    }

    public static void OnBallBounce()
    {

        if (cooldownTimer > 0) return;
        if (currentSongChords == null || currentSongChords.Count == 0) return;

        cooldownTimer = COOLDOWN_DURATION;
        if (noteIndex >= currentSongChords.Count) noteIndex = 0;

        // --- CHANGE IS HERE ---
        // Instead of taking the whole list, we only take the HIGHEST note.
        // In MIDI, the highest number is the highest pitch (the melody).
        int highestNote = currentSongChords[noteIndex].Max()+12* currentSongShift;
        List<int> melodyNoteOnly = new List<int> { highestNote };
        // -----------------------

        // Update your print to show the single note
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        Console.WriteLine($"[Bounce {noteIndex}] Lead Note: {noteNames[highestNote % 12]}{(highestNote / 12) - 1}");

        // Generate and Play
        short[] audio = SoundManager.GenerateChordSound(melodyNoteOnly);
        SoundManager.Play(audio);

        noteIndex = (noteIndex + 1) % currentSongChords.Count;
    }

    public static short[] GenerateChordSound(List<int> notes)
    {
        int sampleRate = 44100;
        double duration = 0.5; // Short duration for a snappy "plink" sound
        int totalSamples = (int)(sampleRate * duration);
        float[] mixBuffer = new float[totalSamples];

        foreach (int midiNote in notes)
        {
            double freq = GetFrequency(midiNote);
            double phaseStep = Math.Tau * freq / sampleRate;
            double phase = 0;

            for (int i = 0; i < totalSamples; i++)
            {
                double time = (double)i / sampleRate;
                phase += phaseStep;

                // Synthesis: Reduced Fundamental (0.3) + High Octaves for "Sparkle"
                // This prevents the "boomy" bass feel.
                double sample = (Math.Sin(phase) * 0.3) +
                               (Math.Sin(phase * 2.0) * 0.7) +
                               (Math.Sin(phase * 4.0) * 0.4);

                // Snappy Envelope: Fast Attack, very fast Exponential Decay (-12.0)
                double attack = Math.Min(1.0, time / 0.005);
                double envelope = attack * Math.Exp(-12.0 * time);

                // Mix into buffer (normalized by note count to prevent clipping)
                mixBuffer[i] += (float)(sample * envelope * (0.5 / notes.Count));
            }
        }

        // Convert mixed float buffer to 16-bit PCM (Short)
        short[] audioData = new short[totalSamples];
        for (int i = 0; i < totalSamples; i++)
        {
            audioData[i] = (short)(Math.Clamp(mixBuffer[i], -1.0f, 1.0f) * short.MaxValue);
        }
        return audioData;
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