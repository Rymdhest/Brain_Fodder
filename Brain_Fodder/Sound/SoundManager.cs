using NAudio.Wave;
using NAudio.Wave.SampleProviders;

class SoundManager
{
    public enum SoundEffect
    {
        MinecraftXP,
        Explosion,
        Jump,
        CoinPickup
    }
    private static readonly Dictionary<SoundEffect, string> SoundPaths = new Dictionary<SoundEffect, string>
{
    { SoundEffect.MinecraftXP, "Audio\\minecraft experience orb.wav" },
};


    public static bool IsRecording = false;

    private static WaveOutEvent outputDevice = new WaveOutEvent();
    private static NAudio.Wave.SampleProviders.MixingSampleProvider mixer;
    private static List<List<int>> currentSongChords;
    private static Dictionary<string, short[]> cachedWavSounds = new Dictionary<string, short[]>();

    private static int currentSongShift = 0;
    private static int noteIndex = 0;
    private static float cooldownTimer = 0;
    private static float COOLDOWN_DURATION = 0.05f;
    
    public SoundManager()
    {
        var format = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);
        mixer = new NAudio.Wave.SampleProviders.MixingSampleProvider(format)
        {
            ReadFully = true // Keeps the mixer playing even when no sounds are active
        };
        outputDevice.Init(mixer);
        outputDevice.Play();
        currentSongChords = SoundManager.LoadSong(SongList.GetRandomSong());

        PreloadSounds();
    }

    public static List<List<int>> LoadSong(Song song)
    {
        string fileName = song.FileName;
        int shift = song.Shift;
        int targetTrack = song.TrackID;

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

    public static void PreloadSounds()
    {
        Console.WriteLine("--- Preloading Sound Effects ---");
        foreach (var entry in SoundPaths)
        {
            string filePath = entry.Value;

            // We only load if it's not already in the cache
            if (!cachedWavSounds.ContainsKey(filePath))
            {
                try
                {
                    using (var audioFile = new AudioFileReader(filePath))
                    {
                        ISampleProvider sampleProvider = audioFile;

                        // Match your mixer format: Mono
                        if (sampleProvider.WaveFormat.Channels > 1)
                            sampleProvider = new StereoToMonoSampleProvider(sampleProvider);

                        // Match your mixer format: 44100Hz
                        if (sampleProvider.WaveFormat.SampleRate != 44100)
                            sampleProvider = new WdlResamplingSampleProvider(sampleProvider, 44100);

                        var samples = new List<short>();
                        float[] buffer = new float[8192];
                        int read;
                        while ((read = sampleProvider.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            for (int i = 0; i < read; i++)
                                samples.Add((short)(Math.Clamp(buffer[i], -1.0f, 1.0f) * short.MaxValue));
                        }

                        cachedWavSounds[filePath] = samples.ToArray();
                        Console.WriteLine($"Loaded: {entry.Key} ({filePath})");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to preload {entry.Key}: {ex.Message}");
                }
            }
        }
        Console.WriteLine("--- Preload Complete ---\n");
    }

    private static void PlayWav(string filePath)
    {
        // 1. Check if the sound is already loaded to save CPU and disk reads
        if (!cachedWavSounds.ContainsKey(filePath))
        {
            try
            {
                using (var audioFile = new NAudio.Wave.AudioFileReader(filePath))
                {
                    NAudio.Wave.ISampleProvider sampleProvider = audioFile;

                    // 2. Force conversion to Mono if the WAV is Stereo
                    if (sampleProvider.WaveFormat.Channels > 1)
                    {
                        sampleProvider = new NAudio.Wave.SampleProviders.StereoToMonoSampleProvider(sampleProvider);
                    }

                    // 3. Force conversion to 44100Hz if the WAV has a different sample rate
                    if (sampleProvider.WaveFormat.SampleRate != 44100)
                    {
                        sampleProvider = new NAudio.Wave.SampleProviders.WdlResamplingSampleProvider(sampleProvider, 44100);
                    }

                    // 4. Read the entire file into a memory list
                    var samples = new List<short>();
                    float[] buffer = new float[8192];
                    int read;

                    while ((read = sampleProvider.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        for (int i = 0; i < read; i++)
                        {
                            // Convert the float data back into shorts for your existing Play() method
                            samples.Add((short)(Math.Clamp(buffer[i], -1.0f, 1.0f) * short.MaxValue));
                        }
                    }

                    // Cache the processed short array
                    cachedWavSounds[filePath] = samples.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading WAV '{filePath}': {ex.Message}");
                return;
            }
        }

        // 5. Send the cached data into your existing playback pipeline
        Play(cachedWavSounds[filePath]);
    }

    public static void PlayWav(SoundEffect effect)
    {
        // Look up the file path linked to this enum
        if (SoundPaths.TryGetValue(effect, out string filePath))
        {
            // Call the original PlayWav method that handles the caching and NAudio playback
            PlayWav(filePath);
        }
        else
        {
            Console.WriteLine($"[Warning] No file path mapped for sound effect: {effect}");
        }
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

        //PlayWav(SoundEffect.MinecraftXP);
        //return;

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
        double duration = 1.0; // Short duration for a snappy "plink" sound
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
        using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, true))
        {
            foreach (var sample in soundData) writer.Write(sample);
        }
        ms.Position = 0;

        var rawStream = new RawSourceWaveStream(ms, new WaveFormat(44100, 16, 1));
        var sampleProvider = new Wave16ToFloatProvider(rawStream).ToSampleProvider();

        // Add this to the mixer at a reasonable volume
        // We do NOT use a limiter here; we apply it to the MIXER output
        mixer.AddMixerInput(sampleProvider);
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


    public static short[] GenerateSound(int step)
    {
        int sampleRate = 44100;
        double duration = 0.6;
        int totalSamples = (int)(sampleRate * duration);
        short[] audioData = new short[totalSamples];

        int[] scale = { 60, 62, 64, 65, 67, 69, 71, 72, 74, 76, 77, 79, 81, 83, 84 };
        double freq = GetFrequency(scale[step % scale.Length]);

        for (int i = 0; i < totalSamples; i++)
        {
            double time = (double)i / sampleRate;

            // 1. Crisp Attack (The Tine)
            double hammer = Math.Sin(Math.Tau * freq * 8.0 * time) * Math.Exp(-100.0 * time);

            // 2. Warm Body
            // We use a slight "overdrive" math here to make it sound thick but smooth
            double rawSine = Math.Sin(Math.Tau * freq * time);
            double body = Math.Tanh(rawSine * 1.5); // Tanh "squashes" the wave smoothly

            // 3. The Envelope
            double envelope = Math.Exp(-6.0 * time);
            double attack = Math.Min(1.0, time / 0.002);

            // Anti-static hard release
            double fadeStart = duration - 0.02;
            double release = (time < fadeStart) ? 1.0 : (duration - time) / 0.02;

            double combined = (hammer * 0.4) + (body * envelope);

            // --- SOFT CLIPPING (The Secret Sauce) ---
            // Instead of hard-clipping at 1.0, we use a sigmoid function.
            // This makes sure that even at high volumes, the wave stays "round."
            double finalSample = Math.Atan(combined * 2.0) / (Math.PI / 2.0);

            audioData[i] = (short)(finalSample * 30000 * attack * release);
        }
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