using Artemis.Core;
using Artemis.Core.Services;
using RGB.NET.Core;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Artemis.Plugins.Wrappers.Shared;

namespace Artemis.Plugins.Wrappers.Logitech.Services
{
    public class LogitechPipeListenerService : IPluginService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly Profiler _profiler;
        private readonly PipeListener _pipeListener;
        private readonly object _lock;
        private readonly Dictionary<LedId, SKColor> _colors;
        private readonly HashSet<LedId> _excluded;
        private Dictionary<LedId, SKColor> _savedColors = new();
        private SKColor _savedBackground = SKColor.Empty;

        public event EventHandler ColorsUpdated;
        public event EventHandler<string> ClientConnected;
        public event EventHandler ClientDisconnected;

        public IReadOnlyDictionary<LedId, SKColor> Colors => _colors;
        public SKColor BackgroundColor { get; private set; } = SKColor.Empty;
        public LogiSetTargetDeviceType DeviceType { get; private set; }

        public LogitechPipeListenerService(ILogger logger, Plugin plugin)
        {
            _logger = logger;
            _profiler = plugin.GetProfiler("Logitech Pipe Listener Service");
            _lock = new();
            _colors = new();
            _excluded = new();

            var id = WTSGetActiveConsoleSessionId();
            var pipeName = "LGS_LED_SDK-" + id.ToString("x8");
            _pipeListener = new(pipeName);
            _pipeListener.ClientConnected += OnPipeListenerClientConnected;
            _pipeListener.ClientDisconnected += OnPipeListenerClientDisconnected;
            _pipeListener.CommandReceived += OnPipeListenerCommandReceived;
            _pipeListener.Exception += OnPipeListenerException;
        }

        [DllImport("kernel32.dll")]
        private static extern uint WTSGetActiveConsoleSessionId();

        private void OnPipeListenerException(object sender, Exception e)
        {
            _logger.Error(e, "Logitech wrapper reader exception ");
        }

        private void OnPipeListenerClientConnected(object sender, EventArgs e)
        {
            _logger.Information("Logitech wrapper reader connected");
        }

        private void OnPipeListenerClientDisconnected(object sender, EventArgs e)
        {
            _logger.Information("Logitech wrapper reader disconnected");
            ClearData();
            ClientDisconnected?.Invoke(this, EventArgs.Empty);
        }

        private void OnPipeListenerCommandReceived(object sender, ReadOnlyMemory<byte> e)
        {
            _profiler.StartMeasurement("Process Command");
            lock (_lock)
            {
                var command = BitConverter.ToInt32(e.Span[..4]);
                var span = e.Span[4..];

                switch ((LogitechPipeCommand)command)
                {
                    case LogitechPipeCommand.Init: Init(span); break;
                    case LogitechPipeCommand.SetTargetDevice: SetTargetDevice(span); break;
                    case LogitechPipeCommand.SetLighting: SetLighting(span); break;
                    case LogitechPipeCommand.SetLightingForKeyWithKeyName: SetLightingForKeyWithKeyName(span); break;
                    case LogitechPipeCommand.SetLightingForKeyWithScanCode: SetLightingForKeyWithScanCode(span); break;
                    case LogitechPipeCommand.SetLightingForKeyWithHidCode: SetLightingForKeyWithHidCode(span); break;
                    case LogitechPipeCommand.SetLightingForKeyWithQuartzCode: SetLightingForKeyWithQuartzCode(span); break;
                    case LogitechPipeCommand.SetLightingFromBitmap: SetLightingFromBitmap(span); break;
                    case LogitechPipeCommand.ExcludeKeysFromBitmap: ExcludeKeysFromBitmap(span); break;
                    case LogitechPipeCommand.SetLightingForTargetZone: SetLightingForTargetZone(span); break;
                    case LogitechPipeCommand.SaveLighting: SaveLighting(span); break;
                    case LogitechPipeCommand.RestoreLighting: RestoreLighting(span); break;
                    case LogitechPipeCommand.SaveLightingForKey: SaveLightingForKey(span); break;
                    case LogitechPipeCommand.RestoreLightingForKey: RestoreLightingForKey(span); break;
                    default: _logger.Debug("Unknown command id: {CommandId}", command); break;
                }
            }
            _profiler.StopMeasurement("Process Command");
        }

        private void SetLightingForKeyWithQuartzCode(ReadOnlySpan<byte> span)
        {
            //_logger.Verbose("SetLightingForKeyWithQuartzCode");
        }

        private void RestoreLightingForKey(ReadOnlySpan<byte> span)
        {
            var key = (LogitechLedId)BitConverter.ToInt32(span);
            if (!LedMapping.LogitechLedIds.TryGetValue(key, out var deviceKey))
                return;

            if (_savedColors.TryGetValue(deviceKey, out var savedColor))
                _colors[deviceKey] = savedColor;
            
            ColorsUpdated?.Invoke(this, EventArgs.Empty);
            //_logger.Verbose("RestoreLightingForKey");
        }

        private void SaveLightingForKey(ReadOnlySpan<byte> span)
        {
            var key = (LogitechLedId)BitConverter.ToInt32(span);
            if (!LedMapping.LogitechLedIds.TryGetValue(key, out var deviceKey))
                return;

            BackgroundColor = _savedBackground;
            if (_colors.TryGetValue(deviceKey, out var savedColor))
                _savedColors[deviceKey] = savedColor;
            
            //_logger.Verbose("SaveLightingForKey");
        }

        private void RestoreLighting(ReadOnlySpan<byte> span)
        {
            BackgroundColor = _savedBackground;
            foreach (var (key, value) in _savedColors)
            {
                _colors[key] = value;
            }
            ColorsUpdated?.Invoke(this, EventArgs.Empty);
            //_logger.Verbose("RestoreLighting");
        }

        private void SaveLighting(ReadOnlySpan<byte> span)
        {
            _savedBackground = BackgroundColor;
            _savedColors = new Dictionary<LedId, SKColor>(_colors);
            //_logger.Verbose("SaveLighting");
        }

        private void SetLightingForTargetZone(ReadOnlySpan<byte> span)
        {
            //zonetype
            //zoneIdx
            //r percentage
            //g percentage
            //b percentage
            //_logger.Verbose("SetLightingForTargetZone");
        }

        private void Init(ReadOnlySpan<byte> span)
        {
            var name = ReadNullTerminatedUnicodeString(span);
            ClientConnected?.Invoke(this, name);
            _logger.Information("LogiLedInit: {Name}", name);
        }

        private void SetTargetDevice(ReadOnlySpan<byte> span)
        {
            DeviceType = (LogiSetTargetDeviceType)BitConverter.ToInt32(span);
            //_logger.Verbose("SetTargetDevice: {DeviceType} ", DeviceType);
        }

        private void SetLighting(ReadOnlySpan<byte> span)
        {
            SKColor color = ReadPercentageColor(span);

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
            //todo: other device zone types

            //_logger.Verbose("SetLighting: {color}", color);
            ColorsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void SetLightingForKeyWithKeyName(ReadOnlySpan<byte> span)
        {
            int keyNameIdx = BitConverter.ToInt32(span);
            SKColor color = ReadPercentageColor(span[4..]);
            LogitechLedId keyName = (LogitechLedId)keyNameIdx;

            if (LedMapping.LogitechLedIds.TryGetValue(keyName, out LedId idx))
            {
                _colors[idx] = color;
            }

            //_logger.Verbose("SetLightingForKeyWithKeyName: {keyName} ({keyNameIdx}) - {color}", keyName, keyNameIdx, color);
            ColorsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void SetLightingForKeyWithScanCode(ReadOnlySpan<byte> span)
        {
            int scanCodeIdx = BitConverter.ToInt32(span);
            SKColor color = ReadPercentageColor(span[4..]);
            DirectInputScanCode scanCode = (DirectInputScanCode)scanCodeIdx;

            if (LedMapping.DirectInputScanCodes.TryGetValue(scanCode, out LedId idx2))
            {
                _colors[idx2] = color;
            }

            //_logger.Verbose("SetLightingForKeyWithScanCode: {scanCode} ({scanCodeIdx}) - {color}", scanCode, scanCodeIdx, color);
            ColorsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void SetLightingForKeyWithHidCode(ReadOnlySpan<byte> span)
        {
            int hidCodeIdx = BitConverter.ToInt32(span);
            SKColor color = ReadPercentageColor(span[4..]);
            HidCode hidCode = (HidCode)hidCodeIdx;

            if (LedMapping.HidCodes.TryGetValue(hidCode, out LedId idx3))
            {
                _colors[idx3] = color;
            }

            ////_logger.Verbose("SetLightingForKeyWithHidCode: {hidCode} ({hidCodeIdx}) - {color}", hidCode, hidCodeIdx, color);
            ColorsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void SetLightingFromBitmap(ReadOnlySpan<byte> span)
        {
            var colors = MemoryMarshal.Cast<byte, LogitechColor>(span);
            for (int clr = 0; clr < colors.Length; clr++)
            {
                if (LedMapping.BitmapMap.TryGetValue(clr * 4, out var ledId) && !_excluded.Contains(ledId))
                {
                    _colors[ledId] = colors[clr].ToSkColor();
                }
            }

            //_logger.Verbose("SetLightingFromBitmap");
            ColorsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void ExcludeKeysFromBitmap(ReadOnlySpan<byte> span)
        {
            var excludedLedIds = MemoryMarshal.Cast<byte, int>(span);
            foreach (var excludedLogitechLedId in excludedLedIds)
            {
                if (!LedMapping.LogitechLedIds.TryGetValue((LogitechLedId)excludedLogitechLedId, out var excludedLedId))
                    continue;

                _excluded.Add(excludedLedId);
            }
            //_logger.Verbose("ExcludeKeysFromBitmap");
        }

        public static string ReadNullTerminatedUnicodeString(ReadOnlySpan<byte> bytes)
        {
            ReadOnlySpan<byte> unicodeNullTerminator = stackalloc byte[] { 0, 0 };
            
            var nullTerminatorIndex = bytes.IndexOf(unicodeNullTerminator);

            if (nullTerminatorIndex == -1)
                return "";
            
            return Encoding.Unicode.GetString(bytes.Slice(0, nullTerminatorIndex + 1));
        }

        public static SKColor ReadPercentageColor(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length != 12)
                return SKColors.Empty;
            
            var r = BitConverter.ToInt32(bytes.Slice(0, 4));
            var g = BitConverter.ToInt32(bytes.Slice(4, 4));
            var b = BitConverter.ToInt32(bytes.Slice(8, 4));
            
            return new SKColor((byte)(r / 100d * 255d), (byte)(g / 100d * 255d), (byte)(b / 100d * 255d));
        }
        
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