using System;
using System.Buffers;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Plugins.Wrappers.Modules.Shared
{
    public class PipeReader : IDisposable
    {
        private readonly NamedPipeServerStream _pipe;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _listenerTask;
        private readonly byte[] _buffer;

        public event EventHandler<ReadOnlyMemory<byte>> CommandReceived;
        public event EventHandler<Exception> Exception;
        public event EventHandler Disconnected;

        public PipeReader(NamedPipeServerStream pipe)
        {
            _pipe = pipe;
            _buffer = new byte[_pipe.InBufferSize];
            _cancellationTokenSource = new();
            _listenerTask = Task.Run(ReadLoop);
        }

        private void ReadLoop()
        {
            while (!_cancellationTokenSource.IsCancellationRequested && _pipe.IsConnected)
            {
                try
                {
                    int position = 0;
                    do
                    {
                        position += _pipe.Read(_buffer, position, _buffer.Length - position);
                    } while (!_pipe.IsMessageComplete);

                    if (position == 0)
                    {
                        continue;
                    }

                    var dataLength = (int)BitConverter.ToUInt32(_buffer, 0);
                    var actualData = _buffer.AsMemory(4, dataLength - 4);

                    CommandReceived?.Invoke(this, actualData);
                }
                catch(Exception e)
                {
                    Exception?.Invoke(this, e);
                }
            }
            _pipe.Dispose();
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            try { _listenerTask.Wait(); } catch { }
            _listenerTask.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }
}
