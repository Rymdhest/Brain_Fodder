using System;
using System.Diagnostics;
using System.IO;

public static class FFmpegMuxer
{
    public static void CombineVideoAndAudio(
        string rawVideoPath,
        string audioWavPath,
        string outputPath,
        int videoWidth,
        int videoHeight,
        int videoFps)
    {
        var videoInfo = new FileInfo(rawVideoPath);
        if (!videoInfo.Exists || videoInfo.Length < 100)
            throw new FileNotFoundException("Video file missing or too small", rawVideoPath);

        var audioInfo = new FileInfo(audioWavPath);
        if (!audioInfo.Exists || audioInfo.Length <= 44)
            throw new FileNotFoundException("Audio file missing or just header", audioWavPath);

        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        // FFmpeg command to:
        // 1. Read raw RGBA video and encode to H.264
        // 2. Read audio WAV and re-encode to AAC
        // 3. Mux them together with proper synchronization

        // Use -itsoffset 0 for audio to delay it slightly to match video start time
        string[] args =
        {
            "-y",
            "-hide_banner",
            "-loglevel", "info",

            // Raw video input
            "-f", "rawvideo",
            "-pixel_format", "rgba",
            "-video_size", $"{videoWidth}x{videoHeight}",
            "-framerate", $"{videoFps}",
            "-i", rawVideoPath,

            // Audio input with offset to sync to video
            "-itsoffset", "0",
            "-i", audioWavPath,

            // Video codec settings with vflip to correct OpenGL orientation
            "-vf", "vflip",
            "-c:v", "libx264",
            "-preset", "veryfast",
            "-crf", "18",
            "-pix_fmt", "yuv420p",

            // Audio codec settings
            "-c:a", "aac",
            "-b:a", "192k",

            // Map streams 
            "-map", "0:v:0",
            "-map", "1:a:0",

            outputPath
        };

        string commandLine = string.Join(" ", args);
        Debug.WriteLine($"FFmpeg command: {commandLine}");

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = commandLine,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(outputPath) ?? "."
        };

        using var proc = new Process { StartInfo = startInfo };

        proc.Start();

        // Read output and stderr so it cannot hang buffering
        Task<string> stdoutTask = proc.StandardOutput.ReadToEndAsync();
        Task<string> stderrTask = proc.StandardError.ReadToEndAsync();

        proc.WaitForExit();

        string stdout = stdoutTask.Result;
        string stderr = stderrTask.Result;

        Debug.WriteLine("--- FFmpeg stdout ---");
        Debug.WriteLine(stdout);
        Debug.WriteLine("--- End FFmpeg stdout ---");

        Debug.WriteLine("--- FFmpeg stderr ---");
        Debug.WriteLine(stderr);
        Debug.WriteLine("--- End FFmpeg stderr ---");

        if (proc.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"FFmpeg failed with exit code {proc.ExitCode}.\nArguments: {commandLine}\nstderr:\n{stderr}");
        }

        var outputFinal = new FileInfo(outputPath);
        if (!outputFinal.Exists || outputFinal.Length == 0)
        {
            throw new InvalidOperationException(
                $"Output file is 0 bytes or missing.\nstderr:\n{stderr}");
        }

        if (File.Exists(audioWavPath))
            File.Delete(audioWavPath);
        if (File.Exists(rawVideoPath))
            File.Delete(rawVideoPath);
    }
}