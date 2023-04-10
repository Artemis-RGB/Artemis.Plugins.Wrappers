using System;
using System.Buffers;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Plugins.Wrappers.Modules.Shared;

public sealed class PipeReader
{
    private readonly NamedPipeServerStream _pipe;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _listenerTask;
    private readonly byte[] _buffer;
    private readonly byte[] _headerBuffer;
    
    public event EventHandler<ReadOnlyMemory<byte>> CommandReceived;
    public event EventHandler<Exception> Exception;
    public event EventHandler Disconnected;

    public PipeReader(NamedPipeServerStream pipe)
    {
        _pipe = pipe;
        _buffer = new byte[_pipe.InBufferSize];
        _headerBuffer = new byte[4];
        _cancellationTokenSource = new();
        _listenerTask = Task.Run(ReadLoop, _cancellationTokenSource.Token);
    }

    private void ReadLoop()
    {
        while (!_cancellationTokenSource.IsCancellationRequested && _pipe.IsConnected)
        {
            try
            {
                ReadAndDispatchPacketAsync();
            }
            catch (Exception e)
            {
                Exception?.Invoke(this, e);
            }
        }

        _pipe.Dispose();
        Disconnected?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Note for future self: you'll want to make this async.
    /// Don't because that will consume lots of cpu.
    /// This is fine since it will only be running when the game is running.
    /// The listener is still async since that is going to be sitting idle most of the time.
    /// </summary>
    private void ReadAndDispatchPacketAsync()
    {
        //if we don't read anything, return and let the loop handle the disconnect
        if (_pipe.Read(_headerBuffer) != 4)
            return;

        var dataLength = BitConverter.ToInt32(_headerBuffer);

        if (dataLength == 0)
        {
            CommandReceived?.Invoke(this, ReadOnlyMemory<byte>.Empty);
            return;
        }

        //we already read the first 4 bytes
        dataLength -= 4;

        int position = 0;
        do
        {
            var bytesRead = _pipe.Read(_buffer.AsSpan(position, dataLength - position));
            if (bytesRead == 0)
                return;
            position += bytesRead;
                
        } while (position < dataLength);

        var actualData = _buffer.AsMemory(0, dataLength);
        CommandReceived?.Invoke(this, actualData);
    }

    #region IDisposable

    private bool _disposedValue;

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _cancellationTokenSource.Cancel();
                try
                {
                    _listenerTask.Wait();
                }
                catch
                {
                    /*we tried*/
                }
                _listenerTask.Dispose();
                _cancellationTokenSource.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}