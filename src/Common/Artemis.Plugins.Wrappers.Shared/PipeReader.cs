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
        _listenerTask = Task.Run(ReadLoop);
    }

    private async Task ReadLoop()
    {
        while (!_cancellationTokenSource.IsCancellationRequested && _pipe.IsConnected)
        {
            try
            {
                await ReadAndDispatchPacketAsync();
            }
            catch (Exception e)
            {
                Exception?.Invoke(this, e);
            }
        }

        await _pipe.DisposeAsync();
        Disconnected?.Invoke(this, EventArgs.Empty);
    }
    
    private async Task ReadAndDispatchPacketAsync()
    {
        if (await _pipe.ReadAsync(_headerBuffer, _cancellationTokenSource.Token) != 4)
            throw new Exception("Pipe closed");

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
            var bytesRead = await _pipe.ReadAsync(_buffer.AsMemory(position, dataLength - position),
                _cancellationTokenSource.Token);
            if (bytesRead == 0)
                throw new Exception("Pipe closed");
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