using System.Diagnostics;
using System.IO;

public class VideoRecorder : IDisposable
{
    private Process _ffmpegProcess;
    private BinaryWriter _videoWriter;

    public void Start(int width, int height, int fps, string outputPath)
    {
        // Simple arguments: Just video, no audio pipes
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-y -f rawvideo -pixel_format rgba -video_size {width}x{height} -framerate {fps} -i - " +
            $"-vf \"vflip\" -c:v libx264 -pix_fmt yuv420p \"{outputPath}\"",
            UseShellExecute = false,
            RedirectStandardInput = true,
            CreateNoWindow = true
        };

        _ffmpegProcess = new Process { StartInfo = startInfo };
        _ffmpegProcess.Start();

        // This is the direct, simple pipe
        _videoWriter = new BinaryWriter(_ffmpegProcess.StandardInput.BaseStream);
    }

    public void WriteFrame(byte[] frameData)
    {
        if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
        {
            _videoWriter.Write(frameData);
            _videoWriter.Flush(); // Ensures data is sent immediately
        }
    }

    public void Stop()
    {
        if (_videoWriter != null)
        {
            _videoWriter.Close(); // This signals EOF to FFmpeg
            _videoWriter = null;
        }

        _ffmpegProcess?.WaitForExit();
        _ffmpegProcess?.Dispose();
        _ffmpegProcess = null;
    }

    public void Dispose() => Stop();
}