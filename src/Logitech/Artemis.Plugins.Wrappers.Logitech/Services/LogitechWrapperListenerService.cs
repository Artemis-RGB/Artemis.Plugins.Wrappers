using Artemis.Core;
using Artemis.Core.Services;
using Artemis.Plugins.Wrappers.Modules.Shared;
using RGB.NET.Core;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Artemis.Plugins.Wrappers.Logitech.Services
{
    public class LogitechWrapperListenerService : IPluginService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly Profiler _profiler;
        private readonly PipeListener _pipeListener;
        private readonly object _lock;
        private readonly Dictionary<LedId, SKColor> _colors;
        private readonly HashSet<LedId> _excluded;

        public event EventHandler ColorsUpdated;
        public event EventHandler ClientDisconnected;

        public IReadOnlyDictionary<LedId, SKColor> Colors => _colors;
        public SKColor BackgroundColor { get; private set; }
        public LogiSetTargetDeviceType DeviceType { get; private set; }
        public int UnknownParameter { get; private set; }

        public LogitechWrapperListenerService(ILogger logger, Plugin plugin)
        {
            _logger = logger;
            _profiler = plugin.GetProfiler("Wrapper Listener Service");
            _lock = new();
            _colors = new();
            _excluded = new();

            //512 fits the bitmap (504 bytes), which is the largest payload
            _pipeListener = new("Artemis\\Logitech", 512);
            _pipeListener.ClientConnected += OnPipeListenerClientConnected;
            _pipeListener.ClientDisconnected += OnPipeListenerClientDisconnected;
            _pipeListener.CommandReceived += OnPipeListenerCommandReceived;
            _pipeListener.Exception += OnPipeListenerException;
        }

        private void OnPipeListenerException(object sender, Exception e)
        {
            _logger.Error("Logitech wrapper reader exception ", e);
        }

        private void OnPipeListenerClientConnected(object sender, EventArgs e)
        {
            _logger.Information("Logitech wrapper reader connected.");
        }

        private void OnPipeListenerClientDisconnected(object sender, EventArgs e)
        {
            _logger.Information("Logitech wrapper reader disconnected.");
            ClearData();
            ClientDisconnected?.Invoke(this, EventArgs.Empty);
        }

        private void OnPipeListenerCommandReceived(object sender, ReadOnlyMemory<byte> e)
        {
            _profiler.StartMeasurement("Process Command");
            lock (_lock)
            {
                var command = (LogitechCommand)BitConverter.ToUInt32(e.Span.Slice(0, 4));
                var span = e.Span.Slice(4);

                switch (command)
                {
                    case LogitechCommand.Init: Init(span); break;
                    case LogitechCommand.Shutdown: Shutdown(span); break;
                    case LogitechCommand.SetTargetDevice: SetTargetDevice(span); break;
                    case LogitechCommand.SetLighting: SetLighting(span); break;
                    case LogitechCommand.SetLightingForKeyWithKeyName: SetLightingForKeyWithKeyName(span); break;
                    case LogitechCommand.SetLightingForKeyWithScanCode: SetLightingForKeyWithScanCode(span); break;
                    case LogitechCommand.SetLightingForKeyWithHidCode: SetLightingForKeyWithHidCode(span); break;
                    case LogitechCommand.SetLightingFromBitmap: SetLightingFromBitmap(span); break;
                    case LogitechCommand.ExcludeKeysFromBitmap: ExcludeKeysFromBitmap(span); break;
                    default: _logger.Information("Unknown command id: {commandId}.", command); break;
                }
            }
            _profiler.StopMeasurement("Process Command");
        }

        private void Init(ReadOnlySpan<byte> span)
        {
            _logger.Information("LogiLedInit: {name}", Encoding.UTF8.GetString(span));
        }

        private void Shutdown(ReadOnlySpan<byte> span)
        {
            ClearData();
            _logger.Information("LogiLedShutdown: {name}", Encoding.UTF8.GetString(span));
            ColorsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void SetTargetDevice(ReadOnlySpan<byte> span)
        {
            DeviceType = (LogiSetTargetDeviceType)BitConverter.ToInt32(span);
            _logger.Verbose("SetTargetDevice: {deviceType} ", DeviceType);
        }

        private void SetLighting(ReadOnlySpan<byte> span)
        {
            SKColor color = FromSpan(span);
            UnknownParameter = BitConverter.ToInt32(span[3..]);
            if (DeviceType == LogiSetTargetDeviceType.PerKeyRgb)
            {
                foreach (LedId key in _colors.Keys)
                {
                    _colors[key] = color;
                }
            }
            else
            {
                BackgroundColor = color;
            }

            _logger.Verbose("SetLighting: {color}", color);
            ColorsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void SetLightingForKeyWithKeyName(ReadOnlySpan<byte> span)
        {
            int keyNameIdx = BitConverter.ToInt32(span);
            SKColor color = FromSpan(span[4..]);
            LogitechLedId keyName = (LogitechLedId)keyNameIdx;

            if (LedMapping.LogitechLedIds.TryGetValue(keyName, out LedId idx))
            {
                _colors[idx] = color;
            }

            _logger.Verbose("SetLightingForKeyWithKeyName: {keyName} ({keyNameIdx}) - {color}", keyName, keyNameIdx, color);
            ColorsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void SetLightingForKeyWithScanCode(ReadOnlySpan<byte> span)
        {
            int scanCodeIdx = BitConverter.ToInt32(span);
            SKColor color = FromSpan(span[4..]);
            DirectInputScanCode scanCode = (DirectInputScanCode)scanCodeIdx;

            if (LedMapping.DirectInputScanCodes.TryGetValue(scanCode, out LedId idx2))
            {
                _colors[idx2] = color;
            }

            _logger.Verbose("SetLightingForKeyWithScanCode: {scanCode} ({scanCodeIdx}) - {color}", scanCode, scanCodeIdx, color);
            ColorsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void SetLightingForKeyWithHidCode(ReadOnlySpan<byte> span)
        {
            int hidCodeIdx = BitConverter.ToInt32(span);
            SKColor color = FromSpan(span[4..]);
            HidCode hidCode = (HidCode)hidCodeIdx;

            if (LedMapping.HidCodes.TryGetValue(hidCode, out LedId idx3))
            {
                _colors[idx3] = color;
            }

            _logger.Verbose("SetLightingForKeyWithHidCode: {hidCode} ({hidCodeIdx}) - {color}", hidCode, hidCodeIdx, color);
            ColorsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void SetLightingFromBitmap(ReadOnlySpan<byte> span)
        {
            const int LOGI_LED_BITMAP_WIDTH = 21;
            const int LOGI_LED_BITMAP_HEIGHT = 6;
            const int LOGI_LED_BITMAP_BYTES_PER_KEY = 4;
            const int LOGI_LED_BITMAP_SIZE = (LOGI_LED_BITMAP_WIDTH * LOGI_LED_BITMAP_HEIGHT * LOGI_LED_BITMAP_BYTES_PER_KEY);

            for (int i = 0; i < LOGI_LED_BITMAP_SIZE; i += LOGI_LED_BITMAP_BYTES_PER_KEY)
            {
                ReadOnlySpan<byte> colorBuff = span.Slice(i, LOGI_LED_BITMAP_BYTES_PER_KEY);
                if (LedMapping.BitmapMap.TryGetValue(i, out LedId l) && !_excluded.Contains(l))
                {
                    //BGRA
                    _colors[l] = new SKColor(colorBuff[2], colorBuff[1], colorBuff[0], colorBuff[3]);
                }
            }
            _logger.Verbose("SetLightingFromBitmap");

            ColorsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void ExcludeKeysFromBitmap(ReadOnlySpan<byte> span)
        {
            for (int nextKeyIndex = 0; nextKeyIndex < span.Length; nextKeyIndex += 4)
            {
                var excludedLogitechLedId = (LogitechLedId)BitConverter.ToInt32(span.Slice(nextKeyIndex, 4));
                if (!LedMapping.LogitechLedIds.TryGetValue(excludedLogitechLedId, out var excludedLedId))
                    continue;

                if (!_excluded.Contains(excludedLedId))
                    _excluded.Add(excludedLedId);
            }
            _logger.Verbose("ExcludeKeysFromBitmap");
        }

        public static SKColor FromSpan(ReadOnlySpan<byte> span) => new(span[0], span[1], span[2]);

        private void ClearData()
        {
            _excluded.Clear();
            _colors.Clear();
            DeviceType = LogiSetTargetDeviceType.All;
            BackgroundColor = SKColors.Empty;
        }

        public void Dispose()
        {
            _colors.Clear();
            _excluded.Clear();

            _pipeListener.ClientConnected -= OnPipeListenerClientConnected;
            _pipeListener.ClientDisconnected -= OnPipeListenerClientDisconnected;
            _pipeListener.CommandReceived -= OnPipeListenerCommandReceived;
            _pipeListener.Exception -= OnPipeListenerException;
            _pipeListener?.Dispose();
        }
    }
}
