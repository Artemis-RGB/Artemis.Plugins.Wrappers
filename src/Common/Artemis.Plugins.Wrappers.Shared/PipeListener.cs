using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Plugins.Wrappers.Modules.Shared
{
    public class PipeListener : IDisposable
    {
        private readonly string _pipeName;
        private readonly int _initialBufferSize;
        private readonly Task _task;
        private readonly CancellationTokenSource _tokenSource;
        private readonly List<PipeReader> _readers;

        public event EventHandler ClientConnected;
        public event EventHandler ClientDisconnected;
        public event EventHandler<ReadOnlyMemory<byte>> CommandReceived;
        public event EventHandler<Exception> Exception;

        public PipeListener(string pipeName, int initialBufferSize)
        {
            _pipeName = pipeName;
            _initialBufferSize = initialBufferSize;
            _tokenSource = new();
            _readers = new();
            _task = Task.Run(Loop);
        }

        private async Task Loop()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    PipeAccessRule rule = new(new SecurityIdentifier(WellKnownSidType.WorldSid, null), PipeAccessRights.FullControl, AccessControlType.Allow);
                    PipeSecurity pipeSecurity = new();
                    pipeSecurity.SetAccessRule(rule);

                    NamedPipeServerStream pipeStream = NamedPipeServerStreamAcl.Create(
                        _pipeName,
                        PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Message,
                        PipeOptions.Asynchronous,
                        0,
                        0,
                        pipeSecurity);

                    await pipeStream.WaitForConnectionAsync(_tokenSource.Token).ConfigureAwait(false);

                    PipeReader reader = new(pipeStream, _initialBufferSize);
                    reader.CommandReceived += OnReaderCommandReceived;
                    reader.Disconnected += OnReaderDisconnected;
                    reader.Exception += OnReaderException;
                    _readers.Add(reader);

                    ClientConnected?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    Exception?.Invoke(this, e);
                }
            }
        }

        private void OnReaderException(object sender, Exception e)
        {
            Exception?.Invoke(sender, e);
        }

        private void OnReaderCommandReceived(object sender, ReadOnlyMemory<byte> e)
        {
            CommandReceived?.Invoke(this, e);
        }

        private void OnReaderDisconnected(object sender, EventArgs e)
        {
            ClientDisconnected?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
            try { _task.Wait(); } catch { }
            _tokenSource.Dispose();

            for (int i = _readers.Count - 1; i >= 0; i--)
            {
                _readers[i].CommandReceived -= OnReaderCommandReceived;
                _readers[i].Disconnected -= OnReaderDisconnected;
                _readers[i].Dispose();
                _readers.RemoveAt(i);
            }

            _readers.Clear();
        }
    }
}
