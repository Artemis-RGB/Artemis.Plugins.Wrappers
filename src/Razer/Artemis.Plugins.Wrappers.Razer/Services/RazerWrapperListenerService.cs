using Artemis.Core.Services;
using Artemis.Plugins.Wrappers.Modules.Shared;
using RGB.NET.Core;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Artemis.Plugins.Wrappers.Razer.Services
{
    public class RazerWrapperListenerService : IPluginService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly PipeListener _pipeListener;
        private readonly object _lock;
        private readonly Dictionary<LedId, SKColor> _colors;
        public ReadOnlyDictionary<LedId, SKColor> Colors { get; }

        public event EventHandler ColorsUpdated;

        public event EventHandler ClientDisconnected;

        public RazerWrapperListenerService(ILogger logger)
        {
            _logger = logger;
            _lock = new();

            _colors = new();
            Colors = new(_colors);

            _pipeListener = new("Artemis\\Razer");
            _pipeListener.ClientConnected += OnPipeListenerClientConnected;
            _pipeListener.ClientDisconnected += OnPipeListenerClientDisconnected;
            _pipeListener.CommandReceived += OnPipeListenerCommandReceived;
            _pipeListener.Exception += OnPipeListenerException;
        }

        private void OnPipeListenerException(object sender, Exception e)
        {
            _logger.Error(e, "Razer wrapper reader exception ");
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
                default:
                    Debugger.Break();
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
                        var ledId = LedMapping.Keyboard[i];
                        var newColor = customKeyboardEffect[i];

                        if (ledId != LedId.Invalid)
                            _colors[ledId] = newColor;
                    }
                    break;
                case KeyboardEffectType.Custom2:
                    var customKeyboardEffectExtended = MemoryMarshal.Read<KeyboardCustomEffectExtended>(span[4..]);
                    for (int i = 0; i < KeyboardCustomEffectExtended.Size; i++)
                    {
                        var ledId = LedMapping.KeyboardExtended[i];
                        var newColor = customKeyboardEffectExtended[i];

                        if (ledId != LedId.Invalid)
                            _colors[ledId] = newColor;
                    }
                    break;
                case KeyboardEffectType.CustomKey:
                    var keyKeyboardEffect = MemoryMarshal.Read<KeyboardCustomKeyEffect>(span[4..]);
                    var keys = keyKeyboardEffect.GetKeys();
                    for (int i = 0; i < KeyboardCustomKeyEffect.Size; i++)
                    {
                        var ledId = LedMapping.Keyboard[i];
                        var newColor = keyKeyboardEffect[i];

                        if (ledId != LedId.Invalid)
                            _colors[ledId] = newColor;
                    }
                    break;
                default:
                    Debugger.Break();
                    break;
            }
            var effectId = new Guid(span[^16..]);

            ColorsUpdated?.Invoke(this, EventArgs.Empty);
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
                        var ledId = LedMapping.Mouse[i];
                        var newColor = customMouseEffect[i];

                        if (ledId != LedId.Invalid)
                            _colors[ledId] = newColor;
                    }
                    break;
                case MouseEffectType.Custom2:
                    var customMouseEffectExtended = MemoryMarshal.Read<MouseCustomEffectExtended>(span[4..]);
                    for (int i = 0; i < MouseCustomEffectExtended.Size; i++)
                    {
                        var ledId = LedMapping.MouseExtended[i];
                        var newColor = customMouseEffectExtended[i];

                        if (ledId != LedId.Invalid)
                            _colors[ledId] = newColor;
                    }
                    break;
            }
            var effectId = new Guid(span[^16..]);

            ColorsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void CreateMousepadEffect(ReadOnlySpan<byte> span)
        {
            var effectType = (MousepadEffectType)BitConverter.ToInt32(span.Slice(0, 4));
            switch (effectType)
            {
                case MousepadEffectType.Custom:
                    var customMousepadEffect = MemoryMarshal.Read<MousepadCustomEffect>(span[4..]);
                    for (int i = 0; i < MousepadCustomEffect.Size; i++)
                    {
                        var ledId = LedMapping.Mousepad[i];
                        var newColor = customMousepadEffect[i];

                        if (ledId != LedId.Invalid)
                            _colors[ledId] = newColor;
                    }
                    break;
                case MousepadEffectType.Custom2:
                    var customMousepadEffectExtended = MemoryMarshal.Read<MousepadCustomEffectExtended>(span[4..]);
                    for (int i = 0; i < MousepadCustomEffectExtended.Size; i++)
                    {
                        var ledId = LedMapping.MousepadExtended[i];
                        var newColor = customMousepadEffectExtended[i];

                        if (ledId != LedId.Invalid)
                            _colors[ledId] = newColor;
                    }
                    break;
            }
            var effectId = new Guid(span[^16..]);

            ColorsUpdated?.Invoke(this, EventArgs.Empty);
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
                        var ledId = LedMapping.Headset[i];
                        var newColor = headsetCustomEffect[i];

                        if (ledId != LedId.Invalid)
                            _colors[ledId] = newColor;
                    }
                    break;
            }
            var effectId = new Guid(span[^16..]);

            ColorsUpdated?.Invoke(this, EventArgs.Empty);
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
                        var ledId = LedMapping.Keypad[i];
                        var newColor = customKeypadEffect[i];

                        if (ledId != LedId.Invalid)
                            _colors[ledId] = newColor;
                    }
                    break;
            }
            var effectId = new Guid(span[^16..]);

            ColorsUpdated?.Invoke(this, EventArgs.Empty);
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
                        var ledId = LedMapping.ChromaLink[i];
                        var newColor = customChromaLinkEffect[i];

                        if (ledId != LedId.Invalid)
                            _colors[ledId] = newColor;
                    }
                    break;
            }
            var effectId = new Guid(span[^16..]);

            ColorsUpdated?.Invoke(this, EventArgs.Empty);
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
