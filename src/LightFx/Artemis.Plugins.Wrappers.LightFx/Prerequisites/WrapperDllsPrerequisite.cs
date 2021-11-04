using Artemis.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Artemis.Plugins.Wrappers.LightFx
{
    public class WrapperDllsPrerequisite : PluginPrerequisite
    {
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
            return IsDllValid(SYSTEM_DLL_64) && IsDllValid(SYSTEM_DLL_32);
        }

        public WrapperDllsPrerequisite(Plugin plugin)
        {
            string dll64 = plugin.ResolveRelativePath($"x64\\{DLL_NAME}");
            string dll32 = plugin.ResolveRelativePath($"x86\\{DLL_NAME}");

            InstallActions = new()
            {
                new RunInlinePowerShellAction("Copy 64 bit dll", $"Copy-Item -Path {dll64} -Destination {SYSTEM_DLL_64}", true),
                new RunInlinePowerShellAction("Copy 32 bit dll", $"Copy-Item -Path {dll32} -Destination {SYSTEM_DLL_32}", true)
            };
            UninstallActions = new List<PluginPrerequisiteAction>();
        }

        private static bool IsDllValid(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return false;
            }

            FileVersionInfo info = FileVersionInfo.GetVersionInfo(fileName);
            return info?.CompanyName == ARTEMIS_COMPANY_NAME &&
                   info?.FileVersion == ARTEMIS_DLL_VERSION;
        }

        private readonly string SYSTEM_DLL_64 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), DLL_NAME_SYSTEM);
        private readonly string SYSTEM_DLL_32 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), DLL_NAME_SYSTEM);
        private const string DLL_NAME = "Artemis.Wrappers.LightFx.dll";
        private const string DLL_NAME_SYSTEM = "LightFX.dll";
        private const string ARTEMIS_COMPANY_NAME = "Artemis";
        private const string ARTEMIS_DLL_VERSION = "1.3.0.0";
    }
}
