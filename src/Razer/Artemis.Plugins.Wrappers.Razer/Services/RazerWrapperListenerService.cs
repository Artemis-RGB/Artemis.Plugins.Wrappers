using Artemis.Core.Services;
using Artemis.Plugins.Wrappers.Modules.Shared;
using Artemis.Plugins.Wrappers.Razer.Services.Effects;
using Artemis.Plugins.Wrappers.Razer.Services.Native;
using RGB.NET.Core;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Artemis.Plugins.Wrappers.Razer.Services
{
    public sealed class RazerWrapperListenerService : IPluginService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly PipeListener _pipeListener;
        private readonly object _lock;
        private readonly Dictionary<Guid, IRazerChromaEffect> _effects;
        private readonly Dictionary<LedId, SKColor> _colors;

        public ReadOnlyDictionary<LedId, SKColor> Colors { get; }

        public event EventHandler ColorsUpdated;

        public event EventHandler ClientDisconnected;

        public RazerWrapperListenerService(ILogger logger)
        {
            _logger = logger;
            _lock = new();
            _effects = new();
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
                _logger.Verbose("Razer command id: {commandId}.", command);

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
                    default: _logger.Information("Unknown command id: {commandId}.", command); break;
                }
            }
        }

        private void Init(ReadOnlySpan<byte> span)
        {
            _logger.Information("ChromaSDKInit: {name}", Encoding.UTF8.GetString(span));
        }

        private void SetEffect(ReadOnlySpan<byte> span)
        {
            var effectId = new Guid(span.Slice(0, 16));
            if (_effects.TryGetValue(effectId, out var effect))
            {
                effect.Apply(_colors);
            }
            ColorsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void DeleteEffect(ReadOnlySpan<byte> span)
        {
            var effectId = new Guid(span.Slice(0, 16));
            _effects.Remove(effectId);
            //un-render somehow.
        }

        private void CreateEffect(ReadOnlySpan<byte> span)
        {
            var deviceId = new Guid(span[..16]);
            var effectType = (EffectType)BitConverter.ToInt32(span.Slice(16, 4));
            var effectId = new Guid(span[^16..]);
            _logger.Verbose("Razer effect type: {effectType}.", effectType);
            //todo
        }

        private void CreateKeyboardEffect(ReadOnlySpan<byte> span)
        {
            var effectType = (KeyboardEffectType)BitConverter.ToInt32(span.Slice(0, 4));
            var effectData = span[4..^16];
            var effectId = new Guid(span[^16..]);

            _logger.Verbose("Razer effect type: {effectType}.", effectType);
            IRazerChromaEffect effect = effectType switch
            {
                KeyboardEffectType.Custom => new KeyboardCustomEffect(effectData),
                KeyboardEffectType.Custom2 => new KeyboardCustomExtendedEffect(effectData),
                KeyboardEffectType.CustomKey => new KeyboardCustomKeyEffect(effectData),
                _ => null
            };

            if (effect != null)
            {
                _effects[effectId] = effect;
            }
        }

        private void CreateMouseEffect(ReadOnlySpan<byte> span)
        {
            var effectType = (MouseEffectType)BitConverter.ToInt32(span.Slice(0, 4));
            var effectData = span[4..^16];
            var effectId = new Guid(span[^16..]);

            _logger.Verbose("Razer effect type: {effectType}.", effectType);
            IRazerChromaEffect effect = effectType switch
            {
                MouseEffectType.Custom => new MouseCustomEffect(effectData),
                MouseEffectType.Custom2 => new MouseCustomExtendedEffect(effectData),
                MouseEffectType.Static => new MouseStaticEffect(effectData),
                _ => null
            };

            if (effect != null)
            {
                _effects[effectId] = effect;
            }
        }

        private void CreateMousepadEffect(ReadOnlySpan<byte> span)
        {
            var effectType = (MousepadEffectType)BitConverter.ToInt32(span.Slice(0, 4));
            var effectData = span[4..^16];
            var effectId = new Guid(span[^16..]);
            _logger.Verbose("Razer effect type: {effectType}.", effectType);
            IRazerChromaEffect effect = effectType switch
            {
                MousepadEffectType.Custom => new MousepadCustomEffect(effectData),
                MousepadEffectType.Custom2 => new MousepadCustomExtendedEffect(effectData),
                _ => null
            };

            if (effect != null)
            {
                _effects[effectId] = effect;
            }
        }

        private void CreateHeadsetEffect(ReadOnlySpan<byte> span)
        {
            var effectType = (HeadsetEffectType)BitConverter.ToInt32(span[..4]);
            var effectData = span[4..^16];
            var effectId = new Guid(span[^16..]);
            _logger.Verbose("Razer effect type: {effectType}.", effectType);
            IRazerChromaEffect effect = effectType switch
            {
                HeadsetEffectType.Custom => new HeadsetCustomEffect(effectData),
                _ => null
            };

            if (effect != null)
            {
                _effects[effectId] = effect;
            }
        }

        private void CreateKeypadEffect(ReadOnlySpan<byte> span)
        {
            var effectType = (KeypadEffectType)BitConverter.ToInt32(span[..4]);
            var effectData = span[4..^16];
            var effectId = new Guid(span[^16..]);
            _logger.Verbose("Razer effect type: {effectType}.", effectType);
            IRazerChromaEffect effect = effectType switch
            {
                KeypadEffectType.Custom => new KeypadCustomEffect(effectData),
                _ => null
            };

            if (effect != null)
            {
                _effects[effectId] = effect;
            }
        }

        private void CreateChromaLinkEffect(ReadOnlySpan<byte> span)
        {
            var effectType = (ChromaLinkEffectType)BitConverter.ToInt32(span[..4]);
            var effectData = span[4..^16];
            var effectId = new Guid(span[^16..]);
            _logger.Verbose("Razer effect type: {effectType}.", effectType);
            IRazerChromaEffect effect = effectType switch
            {
                ChromaLinkEffectType.Custom => new ChromaLinkCustomEffect(effectData),
                _ => null
            };

            if (effect != null)
            {
                _effects[effectId] = effect;
            }
        }

        #region IDisposable
        private bool disposedValue;
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _pipeListener.ClientConnected -= OnPipeListenerClientConnected;
                    _pipeListener.ClientDisconnected -= OnPipeListenerClientDisconnected;
                    _pipeListener.CommandReceived -= OnPipeListenerCommandReceived;
                    _pipeListener.Exception -= OnPipeListenerException;
                    _pipeListener?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
