using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brain_Fodder.Recording
{
    internal class Recorder
    {
        private VideoRecorder? _videoRecorder;
        private AudioRecorder? _audioRecorder;

        private string rawVideoPath;
        private string rawAudioPath;
        private string FinalPath;
        private int _recordingWidth;
        private int _recordingHeight;
        private int _recordingFps;

        public Recorder()
        {
            string userVideos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            string myEngineFolder = Path.Combine(userVideos, "BrainFodder");
            Directory.CreateDirectory(myEngineFolder);
            rawVideoPath = Path.Combine(myEngineFolder, "raw_video.mp4");
            rawAudioPath = Path.Combine(myEngineFolder, "audio.wav");
            FinalPath = Path.Combine(myEngineFolder, "Final.mp4");
        }

        public void StartRecording(Vector2i resolution, int frameRate)
        {
            if (File.Exists(rawVideoPath))
            {
                File.Delete(rawVideoPath);
            }
            if (File.Exists(rawAudioPath))
            {
                File.Delete(rawAudioPath);
            }

            _recordingWidth = resolution.X;
            _recordingHeight = resolution.Y;
            _recordingFps = frameRate;

            _videoRecorder = new VideoRecorder();
            _videoRecorder.Start(resolution.X, resolution.Y, frameRate, rawVideoPath);

            // Start audio recording AFTER video with a small delay to ensure sync
            System.Threading.Thread.Sleep(100);

            _audioRecorder = new AudioRecorder();
            _audioRecorder.Start(rawAudioPath);
        }

        public void RenderFrame(byte[] rgbaFrame)
        {
            _videoRecorder?.WriteFrame(rgbaFrame);
        }

        public void StopRecording()
        {
            _videoRecorder?.Stop();
            _audioRecorder?.Stop();

            _videoRecorder?.Dispose();
            _audioRecorder?.Dispose();

            _videoRecorder = null;
            _audioRecorder = null;

            // Now, if files exist, mux them into final MP4
            if (File.Exists(rawVideoPath) &&
                File.Exists(rawAudioPath))
            {
                FFmpegMuxer.CombineVideoAndAudio(
                    rawVideoPath,
                    rawAudioPath,
                    FinalPath,
                    _recordingWidth,
                    _recordingHeight,
                    _recordingFps
                );
            }
        }
    }
}
