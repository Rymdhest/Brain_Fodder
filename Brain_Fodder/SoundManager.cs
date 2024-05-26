
using OpenTK.Audio.OpenAL;
using System.Runtime.InteropServices;

class SoundManager
{
    static readonly string filename1 = "click.wav";
    public SoundManager()
    {
        List<string> list = ALC.GetString(AlcGetStringList.AllDevicesSpecifier);
        list.ForEach(f => { Console.WriteLine(f); });

        ALDevice device = ALC.OpenDevice(null);
        //ALDevice device = ALC.OpenDevice("Generic Software on HP P27h G4(NVIDIA High Definition Audio)");

        ALContext context = ALC.CreateContext(device, new ALContextAttributes());
        ALC.MakeContextCurrent(context);
    }

    public static async void Play(float pitch = 1.0f)
    {
        int bufferId = AL.GenBuffer();
        int sourceId = AL.GenSource();
        int state;

        int channels, bits_per_sample, sample_rate;

        byte[] sound_data = LoadWave(
            File.Open(filename1, FileMode.Open),
            out channels,
            out bits_per_sample,
            out sample_rate
        );

        sound_data = sound_data.Take(sound_data.Length - (4 * 5000)).ToArray();

        IntPtr unmanagedPointer = Marshal.AllocHGlobal(sound_data.Length);
        Marshal.Copy(sound_data, 0, unmanagedPointer, sound_data.Length);

        Console.WriteLine(AL.GetError());

        int bufferSize = sound_data.Length - sound_data.Length % 4;

        AL.BufferData(
            bufferId,
            GetSoundFormat(channels, bits_per_sample),
            unmanagedPointer,
            bufferSize,
            sample_rate
        );

        Console.WriteLine(AL.GetError()); // no error

        AL.Source(sourceId, ALSourcei.Buffer, bufferId);

        // Set the pitch
        AL.Source(sourceId, ALSourcef.Pitch, pitch);

        AL.SourcePlay(sourceId);

        Console.WriteLine(string.Format("Playing[{0}][{1}]({2}) with pitch {3}",
                            sourceId,
                            bufferId,
                            filename1,
                            pitch));

        await Task.Run(() =>
        {
            do
            {
                Thread.Sleep(250);
                Console.Write(".");
                AL.GetSource(sourceId, ALGetSourcei.SourceState, out state);
            }
            while ((ALSourceState)state == ALSourceState.Playing);

            Console.WriteLine("Playing end : " + sourceId);

            AL.SourceStop(sourceId);
            AL.DeleteSource(sourceId);
            AL.DeleteBuffer(sourceId);

            Marshal.FreeHGlobal(unmanagedPointer);
        });
    }


    public static byte[] LoadWave(
     Stream stream
     , out int channels
     , out int bits
     , out int rate)
    {
        if (stream == null)
            throw new ArgumentNullException("stream");

        using (BinaryReader reader = new BinaryReader(stream))
        {
            // RIFF header
            string signature = new string(reader.ReadChars(4));
            if (signature != "RIFF")
                throw new NotSupportedException("Specified stream is not a wave file.");

            int riff_chunck_size = reader.ReadInt32();

            string format = new string(reader.ReadChars(4));
            if (format != "WAVE")
                throw new NotSupportedException("Specified stream is not a wave file.");

            // WAVE header
            string format_signature = new string(reader.ReadChars(4));
            if (format_signature != "fmt ")
                throw new NotSupportedException("Specified wave file is not supported.");

            int format_chunk_size = reader.ReadInt32();
            int audio_format = reader.ReadInt16();
            int num_channels = reader.ReadInt16();
            int sample_rate = reader.ReadInt32();
            int byte_rate = reader.ReadInt32();
            int block_align = reader.ReadInt16();
            int bits_per_sample = reader.ReadInt16();

            string data_signature = new string(reader.ReadChars(4));
           // if (data_signature != "data")
           //     throw new NotSupportedException("Specified wave file is not supported.");

            int data_chunk_size = reader.ReadInt32();

            channels = num_channels;
            bits = bits_per_sample;
            rate = sample_rate;

            return reader.ReadBytes((int)reader.BaseStream.Length);
        }
    }

    public static byte[] GenerateSineWave(int sampleRate, double frequency, double duration, out int channels, out int bits)
    {
        int samples = (int)(sampleRate * duration);
        byte[] buffer = new byte[samples * 2]; // 2 bytes per sample for 16-bit audio
        short amplitude = 32760; // Max amplitude for 16-bit audio
        double theta = frequency * 2 * Math.PI / sampleRate;

        for (int i = 0; i < samples; i++)
        {
            short sample = (short)(amplitude * Math.Sin(theta * i));
            buffer[2 * i] = (byte)(sample & 0xFF);
            buffer[2 * i + 1] = (byte)((sample >> 8) & 0xFF);
        }

        channels = 1; // Mono
        bits = 16; // 16-bit audio
        return buffer;
    }
    public static ALFormat GetSoundFormat(int channels, int bits)
    {
        switch (channels)
        {
            case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
            case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
            default: throw new NotSupportedException("The specified sound format is not supported.");
        }
    }
}