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
        private readonly int _initialArraySize;

        public event EventHandler<ReadOnlyMemory<byte>> CommandReceived;
        public event EventHandler<Exception> Exception;
        public event EventHandler Disconnected;

        public PipeReader(NamedPipeServerStream pipe, int initialArraySize)
        {
            _pipe = pipe;
            _cancellationTokenSource = new();
            _listenerTask = Task.Run(ReadLoop);
            _initialArraySize = initialArraySize;
        }

        private async Task ReadLoop()
        {
            while (!_cancellationTokenSource.IsCancellationRequested && _pipe.IsConnected)
            {
                int initialArraySize = _initialArraySize;
                byte[] rented = ArrayPool<byte>.Shared.Rent(initialArraySize);

                try
                {
                    int position = 0;
                    do
                    {
                        if (position >= rented.Length)
                        {
                            //we need to allocate a new buffer and copy over the data.
                            initialArraySize *= 2;
                            var newRented = ArrayPool<byte>.Shared.Rent(initialArraySize);
                            Array.Copy(rented, newRented, rented.Length);
                            ArrayPool<byte>.Shared.Return(rented);
                            rented = newRented;
                        }

                        position += await _pipe.ReadAsync(rented.AsMemory(position), _cancellationTokenSource.Token).ConfigureAwait(false);
                    } while (!_pipe.IsMessageComplete);

                    if (position == 0)
                    {
                        //I don't know why, but this happens when closing the pipe.
                        //return the buffer to the pool and skip the rest
                        ArrayPool<byte>.Shared.Return(rented);
                        continue;
                    }

                    var dataLength = (int)BitConverter.ToUInt32(rented, 0);
                    var actualData = rented.AsMemory(4, dataLength - 4);

                    CommandReceived?.Invoke(this, actualData);
                }
                catch(Exception e)
                {
                    Exception?.Invoke(this, e);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(rented);
                }
            }
            await _pipe.DisposeAsync();
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
