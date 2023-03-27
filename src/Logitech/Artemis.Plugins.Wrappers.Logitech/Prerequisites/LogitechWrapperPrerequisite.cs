using System.Collections.Generic;
using System.IO;
using Artemis.Core;
using Microsoft.Win32;

namespace Artemis.Plugins.Wrappers.Logitech.Prerequisites;

internal class LogitechWrapperPrerequisite : PluginPrerequisite
{
    private const string DLL_NAME = "LogitechLed.dll";

    private const string REGISTRY_PATH_64 =
        "SOFTWARE\\Classes\\CLSID\\{a6519e67-7632-4375-afdf-caa889744403}\\ServerBinary";

    private const string REGISTRY_PATH_32 =
        "SOFTWARE\\Classes\\WOW6432Node\\CLSID\\{a6519e67-7632-4375-afdf-caa889744403}\\ServerBinary";

    private readonly string _dllPath32;
    private readonly string _dllPath64;

    public LogitechWrapperPrerequisite(Plugin plugin)
    {
        _dllPath64 = Path.Combine(plugin.Directory.FullName, "x64", DLL_NAME);
        _dllPath32 = Path.Combine(plugin.Directory.FullName, "x86", DLL_NAME);

        var patchScript = plugin.ResolveRelativePath("Scripts\\patch-registry.ps1");
        var unpatchScript = plugin.ResolveRelativePath("Scripts\\unpatch-registry.ps1");

        InstallActions = new List<PluginPrerequisiteAction>
        {
            new RunPowerShellAction("Patch 64 bit registry", patchScript, true,
                $"\"HKLM:{REGISTRY_PATH_64}\" \"{_dllPath64}\""),
            new RunPowerShellAction("Patch 32 bit registry", patchScript, true,
                $"\"HKLM:{REGISTRY_PATH_32}\" \"{_dllPath32}\"")
        };

        UninstallActions = new List<PluginPrerequisiteAction>
        {
            new RunPowerShellAction("Unpatch 64 bit registry", unpatchScript, true, $"\"HKLM:{REGISTRY_PATH_64}\""),
            new RunPowerShellAction("Unpatch 32 bit registry", unpatchScript, true, $"\"HKLM:{REGISTRY_PATH_32}\"")
        };
    }

    public override string Name => "Logitech wrapper registry patch";

    public override string Description => "This registry patch makes games send their lighting to Artemis instead of LGS or LGHUB";

    public override List<PluginPrerequisiteAction> InstallActions { get; }

    public override List<PluginPrerequisiteAction> UninstallActions { get; }

    public override bool IsMet()
    {
        using var key64 = Registry.LocalMachine.OpenSubKey(REGISTRY_PATH_64);
        using var key32 = Registry.LocalMachine.OpenSubKey(REGISTRY_PATH_32);

        var is64BitKeyPresent = key64?.GetValue(null)?.ToString() == _dllPath64;
        var is32BitKeyPresent = key32?.GetValue(null)?.ToString() == _dllPath32;

        return is64BitKeyPresent && is32BitKeyPresent;
    }
}