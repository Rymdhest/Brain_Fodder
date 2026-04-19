using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.IO;

public sealed class AudioRecorder : IDisposable
{
    private WasapiLoopbackCapture? _capture;
    private WaveFileWriter? _writer;

    public void Start(string wavPath)
    {
        if (_capture != null)
            throw new InvalidOperationException("AudioRecorder already started.");

        _capture = new WasapiLoopbackCapture();

        WaveFormat format = _capture.WaveFormat;

        _writer = new WaveFileWriter(wavPath, format);

        _capture.DataAvailable += (sender, e) =>
        {
            _writer?.Write(e.Buffer, 0, e.BytesRecorded);
        };

        _capture.StartRecording();
    }

    public void Stop()
    {
        if (_capture == null)
            return;

        _capture.StopRecording();
        _capture?.Dispose();
        _capture = null;

        _writer?.Dispose();
        _writer = null;
    }

    public void Dispose() => Stop();
}