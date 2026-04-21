using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public sealed class VideoRecorder : IDisposable
{
    private Process? _ffmpeg;
    private BinaryWriter? _videoWriter;
    private Task? _stderrTask;

    private volatile bool _stopping;

    public bool IsRunning => _ffmpeg != null && !_ffmpeg.HasExited;

    public void Start(int width, int height, int fps, string rawVideoPath)
    {
        if (_ffmpeg != null)
            throw new InvalidOperationException("VideoRecorder already started.");

        _stopping = false;

        // Use a List for arguments to prevent spacing/parsing errors
        var args = new[]
        {
        "-y",
        "-hide_banner", "-loglevel", "info",
        "-f", "rawvideo", "-pixel_format", "rgba", "-video_size", $"{width}x{height}", "-framerate", $"{fps.ToString()}", "-i", "-",
        "-vf", "vflip",
        "-c:v", "libx264", "-preset", "veryfast", "-crf", "18", "-pix_fmt", "yuv420p",
        $"\"{rawVideoPath}\""
    };

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = string.Join(" ", args),
            UseShellExecute = false,
            CreateNoWindow = false, // Keep false for now to see errors
            RedirectStandardInput = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            WorkingDirectory = Path.GetDirectoryName(rawVideoPath) ?? "."
        };

        _ffmpeg = new Process { StartInfo = startInfo };

        // 1. Hook up the event handler
        _ffmpeg.ErrorDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine($"FFMPEG ERROR: {e.Data}");
        };

        // 2. Start the process FIRST
        if (!_ffmpeg.Start())
            throw new InvalidOperationException("Failed to start FFmpeg.");

        // 3. Begin reading ONLY after Start()
        _ffmpeg.BeginErrorReadLine();

        // Note: Removed the _stderrTask entirely as it conflicts with ErrorDataReceived

        // 4. Set up stdin writer
        _videoWriter = new BinaryWriter(_ffmpeg.StandardInput.BaseStream);
    }

    public void WriteFrame(byte[] frameData)
    {
        if (_stopping)
            return;

        try
        {
            _videoWriter?.Write(frameData);
            _videoWriter?.Flush();
        }
        catch
        {
            // If writing fails, schedule a clean stop
            Stop();
        }
    }

    public void Stop()
    {
        if (_stopping)
            return;

        _stopping = true;

        // 1. Close the writer (signal EOF to FFmpeg)
        try
        {
            _videoWriter?.Dispose();
            _videoWriter = null;
        }
        catch
        {
        }

        // 2. Wait up to 5 seconds for FFmpeg to exit cleanly
        if (_ffmpeg != null && !_ffmpeg.HasExited)
        {
            try
            {
                if (!_ffmpeg.WaitForExit(5000))
                {
                    // If it still refuses to exit, kill it
                    try
                    {
                        _ffmpeg.Kill();
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        // 3. Dispose FFmpeg and stderr task
        try
        {
            _ffmpeg?.Dispose();
            _ffmpeg = null;
        }
        catch
        {
        }

        _stderrTask = null;
    }

    public void Dispose() => Stop();
}