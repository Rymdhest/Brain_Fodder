using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public sealed class VideoRecorder : IDisposable
{
    private FileStream? _rawVideoStream;
    private int _width;
    private int _height;
    private int _fps;
    private string _rawVideoPath;

    private volatile bool _stopping;
    private Task? _writeTask;
    private System.Collections.Concurrent.ConcurrentQueue<byte[]>? _frameQueue;
    private CancellationTokenSource? _cancellationTokenSource;
    private object _queueLock = new object();
    private int _queueSize = 0;

    public bool IsRunning => _rawVideoStream != null && _rawVideoStream.CanWrite;

    public void Start(int width, int height, int fps, string rawVideoPath)
    {
        if (_rawVideoStream != null)
            throw new InvalidOperationException("VideoRecorder already started.");

        _stopping = false;
        _width = width;
        _height = height;
        _fps = fps;
        _rawVideoPath = rawVideoPath;

        // Create directory if it doesn't exist
        var directory = Path.GetDirectoryName(rawVideoPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Write raw RGBA frames directly to file
        _rawVideoStream = new FileStream(
            rawVideoPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            4 * 1024 * 1024,
            FileOptions.SequentialScan
        );

        _frameQueue = new System.Collections.Concurrent.ConcurrentQueue<byte[]>();
        _cancellationTokenSource = new CancellationTokenSource();
        _writeTask = Task.Run(() => WriteFramesAsync(_cancellationTokenSource.Token));
    }

    public void WriteFrame(byte[] frameData)
    {
        if (_stopping || _frameQueue == null || frameData == null)
            return;

        try
        {
            // Queue the frame - don't copy, pass reference directly
            _frameQueue.Enqueue(frameData);

            lock (_queueLock)
            {
                _queueSize++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error queueing frame: {ex.Message}");
            Stop();
        }
    }

    private async Task WriteFramesAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_frameQueue != null && _frameQueue.TryDequeue(out var frameData))
                {
                    if (frameData != null)
                    {
                        try
                        {
                            _rawVideoStream?.Write(frameData, 0, frameData.Length);

                            lock (_queueLock)
                            {
                                _queueSize--;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error writing frame: {ex.Message}");
                            break;
                        }
                    }
                }
                else
                {
                    // Small sleep when queue is empty
                    await Task.Delay(1, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown
        }
    }

    public void Stop()
    {
        if (_stopping)
            return;

        _stopping = true;

        // Signal write task to stop
        try
        {
            _cancellationTokenSource?.Cancel();
        }
        catch
        {
        }

        // Wait for remaining frames to be written (up to 30 seconds)
        try
        {
            if (!_writeTask?.Wait(30000) ?? false)
            {
                Console.WriteLine("Warning: Write task did not complete in time");
            }
        }
        catch
        {
        }

        // Close file
        try
        {
            _rawVideoStream?.Flush();
            _rawVideoStream?.Dispose();
            _rawVideoStream = null;
        }
        catch
        {
        }

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }

    public void Dispose() => Stop();
}