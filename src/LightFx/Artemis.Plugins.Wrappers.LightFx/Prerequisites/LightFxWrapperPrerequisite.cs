using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Artemis.Core;

namespace Artemis.Plugins.Wrappers.LightFx.Prerequisites;

public class LightFxWrapperPrerequisite : PluginPrerequisite
{
    private const string DLL_NAME = "Artemis.Wrappers.LightFx.dll";
    private const string DLL_NAME_SYSTEM = "LightFX.dll";
    private readonly string _dll32;
    private readonly string _dll64;
    private readonly string _systemDll32;
    private readonly string _systemDll64;

    public LightFxWrapperPrerequisite(Plugin plugin)
    {
        _systemDll64 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), DLL_NAME_SYSTEM);
        _systemDll32 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), DLL_NAME_SYSTEM);
        _dll64 = plugin.ResolveRelativePath(Path.Combine("x64", DLL_NAME));
        _dll32 = plugin.ResolveRelativePath(Path.Combine("x86", DLL_NAME));

        InstallActions = new List<PluginPrerequisiteAction>
        {
            new RunInlinePowerShellAction("Copy 64 bit dll", $"Copy-Item -Path {_dll64} -Destination {_systemDll64}",
                true),
            new RunInlinePowerShellAction("Copy 32 bit dll", $"Copy-Item -Path {_dll32} -Destination {_systemDll32}",
                true)
        };
        UninstallActions = new List<PluginPrerequisiteAction>();
    }

    /// <inheritdoc />
    public override string Name => "Place wrapper Dlls";

    /// <inheritdoc />
    public override string Description => "This will copy the needed files to capture LightFX lighting";

    /// <inheritdoc />
    public override List<PluginPrerequisiteAction> InstallActions { get; }

    /// <inheritdoc />
    public override List<PluginPrerequisiteAction> UninstallActions { get; }

    /// <inheritdoc />
    public override bool IsMet()
    {
        return IsDllValid(_systemDll64, _dll64)
               && IsDllValid(_systemDll32, _dll32);
    }

    private static bool IsDllValid(string fileName, string compareTo)
    {
        if (!File.Exists(fileName)) return false;

        var info = FileVersionInfo.GetVersionInfo(fileName);
        var desiredInfo = FileVersionInfo.GetVersionInfo(compareTo);

        return info?.CompanyName == desiredInfo?.CompanyName &&
               info?.ProductName == desiredInfo?.ProductName &&
               info?.ProductVersion == desiredInfo?.ProductVersion;
    }
}