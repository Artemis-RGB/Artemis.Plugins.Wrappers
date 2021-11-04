using Artemis.Core.Services;
using Artemis.Plugins.Wrappers.Modules.Shared;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Artemis.Plugins.Wrappers.Razer.Services
{
    public class RazerWrapperListenerService : IPluginService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly PipeListener _pipeListener;
        private readonly object _lock;
        private readonly Dictionary<Guid, object> _effects;
        public readonly SKColor[] _keyboardCustom = new SKColor[KeyboardCustomEffect.Size];
        public readonly SKColor[] _keyboardCustomExtended = new SKColor[KeyboardCustomEffectExtended.Size];
        public readonly SKColor[] _mouseCustom = new SKColor[MouseCustomEffect.Size];
        public readonly SKColor[] _mouseCustomExtended = new SKColor[MouseCustomEffectExtended.Size];
        public readonly SKColor[] _mousepadCustom = new SKColor[MousepadCustomEffect.Size];
        public readonly SKColor[] _mousepadCustomExtended = new SKColor[MousepadCustomEffectExtended.Size];
        public readonly SKColor[] _headsetCustom = new SKColor[HeadsetCustomEffect.Size];
        public readonly SKColor[] _keypadCustom = new SKColor[KeypadCustomEffect.Size];
        public readonly SKColor[] _chromaLinkCustom = new SKColor[ChromaLinkCustomEffect.Size];

        public event EventHandler ColorsUpdated;
        public event EventHandler ClientDisconnected;

        public RazerWrapperListenerService(ILogger logger)
        {
            _logger = logger;
            _lock = new();
            _effects = new();

            //the custom2 payload is ~1300 bytes
            _pipeListener = new("Artemis\\Razer", 2048);
            _pipeListener.ClientConnected += OnPipeListenerClientConnected;
            _pipeListener.ClientDisconnected += OnPipeListenerClientDisconnected;
            _pipeListener.CommandReceived += OnPipeListenerCommandReceived;
            _pipeListener.Exception += OnPipeListenerException;
        }

        private void OnPipeListenerException(object sender, Exception e)
        {
            _logger.Error("Razer wrapper reader exception ", e);
        }

        private void OnPipeListenerClientConnected(object sender, EventArgs e)
        {
            _logger.Information("Razer wrapper reader connected.");
        }

        private void OnPipeListenerClientDisconnected(object sender, EventArgs e)
        {
            _logger.Information("Razer wrapper reader disconnected.");
            ClientDisconnected?.Invoke(this, EventArgs.Empty);
        }

        private void OnPipeListenerCommandReceived(object sender, ReadOnlyMemory<byte> e)
        {
            lock (_lock)
            {
                var command = (RazerCommand)BitConverter.ToUInt32(e.Span.Slice(0, 4));
                var span = e.Span[4..];
                switch (command)
                {
                    case RazerCommand.Init: Init(span); break;
                    case RazerCommand.SetEffect: SetEffect(span); break;
                    case RazerCommand.DeleteEffect: DeleteEffect(span); break;
                    case RazerCommand.CreateEffect: CreateEffect(span); break;
                    case RazerCommand.CreateKeyboardEffect: CreateKeyboardEffect(span); break;
                    case RazerCommand.CreateMouseEffect: CreateMouseEffect(span); break;
                    case RazerCommand.CreateMousepadEffect: CreateMousepadEffect(span); break;
                    case RazerCommand.CreateHeadsetEffect: CreateHeadsetEffect(span); break;
                    case RazerCommand.CreateKeypadEffect: CreateKeypadEffect(span); break;
                    case RazerCommand.CreateChromaLinkEffect: CreateChromaLinkEffect(span); break;
                    default: _logger.Verbose("Unknown command id: {commandId}.", command); break;
                }
                //_logger.Information("Razer command id: {commandId}.", command);
            }
        }

        private void Init(ReadOnlySpan<byte> span)
        {
            _logger.Information("ChromaSDKInit: {name}", Encoding.UTF8.GetString(span));
        }

        private void SetEffect(ReadOnlySpan<byte> span)
        {
            var effectId = new Guid(span.Slice(0, 16));
        }

        private void DeleteEffect(ReadOnlySpan<byte> span)
        {
            var effectId = new Guid(span.Slice(0, 16));
        }

        private void CreateEffect(ReadOnlySpan<byte> span)
        {
            var deviceId = new Guid(span.Slice(0, 16));
            var effectType = (EffectType)BitConverter.ToInt32(span.Slice(16, 4));
            switch (effectType)
            {
                case EffectType.Custom:
                    var s = MemoryMarshal.Read<CustomEffect>(span[20..]);
                    break;
            }
            var effectId = new Guid(span[^16..]);
        }

        private void CreateKeyboardEffect(ReadOnlySpan<byte> span)
        {
            var effectType = (KeyboardEffectType)BitConverter.ToInt32(span.Slice(0, 4));
            switch (effectType)
            {
                case KeyboardEffectType.Custom:
                    var customKeyboardEffect = MemoryMarshal.Read<KeyboardCustomEffect>(span[4..]);
                    for (int i = 0; i < KeyboardCustomEffect.Size; i++)
                    {
                        _keyboardCustom[i] = customKeyboardEffect[i];
                    }
                    break;
                case KeyboardEffectType.Custom2:
                    var customKeyboardEffectExtended = MemoryMarshal.Read<KeyboardCustomEffectExtended>(span[4..]);
                    for (int i = 0; i < KeyboardCustomEffectExtended.Size; i++)
                    {
                        _keyboardCustomExtended[i] = customKeyboardEffectExtended[i];
                    }
                    break;
                case KeyboardEffectType.CustomKey:
                    var keyKeyboardEffect = MemoryMarshal.Read<KeyboardCustomKeyEffect>(span[4..]);
                    for (int i = 0; i < KeyboardCustomKeyEffect.Size; i++)
                    {
                        _keyboardCustom[i] = keyKeyboardEffect[i];
                    }
                    break;
            }
            ColorsUpdated?.Invoke(this, EventArgs.Empty);
            var effectId = new Guid(span[^16..]);
        }

        private void CreateMouseEffect(ReadOnlySpan<byte> span)
        {
            var effectType = (MouseEffectType)BitConverter.ToInt32(span.Slice(0, 4));
            switch (effectType)
            {
                case MouseEffectType.Custom:
                    var customMouseEffect = MemoryMarshal.Read<MouseCustomEffect>(span[4..]);
                    for (int i = 0; i < MouseCustomEffect.Size; i++)
                    {
                        _mouseCustom[i] = customMouseEffect[i];
                    }
                    break;
                case MouseEffectType.Custom2:
                    var customMouseEffectExtended = MemoryMarshal.Read<MouseCustomEffectExtended>(span[4..]);
                    for (int i = 0; i < MouseCustomEffectExtended.Size; i++)
                    {
                        _mouseCustomExtended[i] = customMouseEffectExtended[i];
                    }
                    break;
            }
            var effectId = new Guid(span[^16..]);
        }

        private void CreateMousepadEffect(ReadOnlySpan<byte> span)
        {
            var effectType = (MousepadEffectType)BitConverter.ToInt32(span.Slice(0, 4));
            switch (effectType)
            {
                case MousepadEffectType.Custom:
                    var customMouseEffect = MemoryMarshal.Read<MousepadCustomEffect>(span[4..]);
                    for (int i = 0; i < MousepadCustomEffect.Size; i++)
                    {
                        _mousepadCustom[i] = customMouseEffect[i];
                    }
                    break;
                case MousepadEffectType.Custom2:
                    var customMouseEffectExtended = MemoryMarshal.Read<MousepadCustomEffectExtended>(span[4..]);
                    for (int i = 0; i < MousepadCustomEffectExtended.Size; i++)
                    {
                        _mousepadCustomExtended[i] = customMouseEffectExtended[i];
                    }
                    break;
            }
            var effectId = new Guid(span[^16..]);
        }

        private void CreateHeadsetEffect(ReadOnlySpan<byte> span)
        {
            var effectType = (HeadsetEffectType)BitConverter.ToInt32(span.Slice(0, 4));
            switch (effectType)
            {
                case HeadsetEffectType.Custom:
                    var headsetCustomEffect = MemoryMarshal.Read<HeadsetCustomEffect>(span[20..]);
                    for (int i = 0; i < HeadsetCustomEffect.Size; i++)
                    {
                        _headsetCustom[i] = headsetCustomEffect[i];
                    }
                    break;
            }
            var effectId = new Guid(span[^16..]);
        }

        private void CreateKeypadEffect(ReadOnlySpan<byte> span)
        {
            var effectType = (KeypadEffectType)BitConverter.ToInt32(span.Slice(0, 4));
            switch (effectType)
            {
                case KeypadEffectType.Custom:
                    var customKeypadEffect = MemoryMarshal.Read<KeypadCustomEffect>(span[4..]);
                    for (int i = 0; i < KeypadCustomEffect.Size; i++)
                    {
                        _keypadCustom[i] = customKeypadEffect[i];
                    }
                    break;
            }
            var effectId = new Guid(span[^16..]);
        }

        private void CreateChromaLinkEffect(ReadOnlySpan<byte> span)
        {
            var effectType = (ChromaLinkEffectType)BitConverter.ToInt32(span.Slice(0, 4));
            switch (effectType)
            {
                case ChromaLinkEffectType.Custom:
                    var customChromaLinkEffect = MemoryMarshal.Read<ChromaLinkCustomEffect>(span[4..]);
                    for (int i = 0; i < ChromaLinkCustomEffect.Size; i++)
                    {
                        _chromaLinkCustom[i] = customChromaLinkEffect[i];
                    }
                    break;
            }
            var effectId = new Guid(span[^16..]);
        }

        public void Dispose()
        {
            _pipeListener.ClientConnected -= OnPipeListenerClientConnected;
            _pipeListener.ClientDisconnected -= OnPipeListenerClientDisconnected;
            _pipeListener.CommandReceived -= OnPipeListenerCommandReceived;
            _pipeListener.Exception -= OnPipeListenerException;
            _pipeListener?.Dispose();
        }
    }
}
