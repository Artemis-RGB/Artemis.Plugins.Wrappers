using Artemis.Core.Services;
using Artemis.Plugins.Wrappers.Modules.Shared;
using RGB.NET.Core;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.Wrappers.LightFx.Services
{
    public class LightFxWrapperListenerService : IPluginService, IDisposable
    {
        private readonly PipeListener _pipeListener;
        private readonly ILogger _logger;
        private readonly object _lock;
        private readonly Dictionary<LedId, SKColor> _colors;

        public event EventHandler Updated;
        public event EventHandler ClientDisconnected;

        public IReadOnlyDictionary<LedId, SKColor> Colors => _colors;

        public SKColor Color { get; private set; }

        public LightFxWrapperListenerService(ILogger logger)
        {
            _logger = logger;
            _lock = new();
            _colors = new();

            //32 because most of the payloads are very small.
            _pipeListener = new("Artemis\\LightFx", 32);
            _pipeListener.ClientConnected += OnPipeListenerClientConnected;
            _pipeListener.ClientDisconnected += OnPipeListenerClientDisconnected;
            _pipeListener.CommandReceived += OnPipeListenerCommandReceived;
            _pipeListener.Exception += OnPipeListenerException;
        }

        private void OnPipeListenerClientConnected(object sender, EventArgs e)
        {
            _logger.Information("LightFx wrapper reader connected.");
        }

        private void OnPipeListenerClientDisconnected(object sender, EventArgs e)
        {
            _logger.Information("LightFx wrapper reader disconnected.");
            ClientDisconnected?.Invoke(this, EventArgs.Empty);
        }

        private void OnPipeListenerException(object sender, Exception e)
        {
            _logger.Error("LightFx wrapper reader exception ", e);
        }

        private void OnPipeListenerCommandReceived(object sender, ReadOnlyMemory<byte> e)
        {
            lock (_lock)
            {
                LightFxCommand command = (LightFxCommand)BitConverter.ToUInt32(e.Span.Slice(0, 4));
                ReadOnlySpan<byte> span = e.Span.Slice(4);

                switch (command)
                {
                    case LightFxCommand.Update: Updated?.Invoke(this, EventArgs.Empty); break;

                    case LightFxCommand.SetLightColor: SetLightColor(span); break;
                    case LightFxCommand.Reset: Reset(); break;
                    default: _logger.Information("Unknown command id: {commandId}.", command); break;
                }
            }
        }

        private void SetLightColor(ReadOnlySpan<byte> span)
        {
            Color = FromSpan(span);
        }

        private void Reset()
        {
            Color = SKColor.Empty;
        }

        public static SKColor FromSpan(ReadOnlySpan<byte> span)
        {
            //colors sent by the lightFX dll are all BGRA
            return new(span[2], span[1], span[0], span[3]);
        }

        public void Dispose()
        {
            _colors.Clear();

            _pipeListener.ClientConnected -= OnPipeListenerClientConnected;
            _pipeListener.ClientDisconnected -= OnPipeListenerClientDisconnected;
            _pipeListener.CommandReceived -= OnPipeListenerCommandReceived;
            _pipeListener.Exception -= OnPipeListenerException;
            _pipeListener?.Dispose();
        }
    }
}
