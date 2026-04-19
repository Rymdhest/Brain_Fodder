using System;
using System.Diagnostics;
using System.IO;

public static class FFmpegMuxer
{
    public static void CombineVideoAndAudio(
        string videoPath,
        string audioWavPath,
        string outputPath)
    {
        var videoInfo = new FileInfo(videoPath);
        if (!videoInfo.Exists || videoInfo.Length < 100)
            throw new FileNotFoundException("Video file missing or too small", videoPath);

        var audioInfo = new FileInfo(audioWavPath);
        if (!audioInfo.Exists || audioInfo.Length <= 44)
            throw new FileNotFoundException("Audio file missing or just header", audioWavPath);

        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        string[] args =
        {
            "-y",
            "-hide_banner",
            "-loglevel", "info",

            "-i", videoPath,
            "-i", audioWavPath,

            "-c:v", "libx264",
            "-preset", "veryfast",
            "-crf", "18",
            "-pix_fmt", "yuv420p",

            "-c:a", "aac",
            "-b:a", "192k",

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

        File.Delete(audioWavPath);
        File.Delete(videoPath);
    }
}