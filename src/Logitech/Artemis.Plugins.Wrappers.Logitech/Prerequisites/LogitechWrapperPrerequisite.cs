using Artemis.Core;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;

namespace Artemis.Plugins.Wrappers.Logitech.Prerequisites
{
    internal class LogitechWrapperPrerequisite : PluginPrerequisite
    {
        public override string Name => "Logitech wrapper registry patch";

        public override string Description => "This registry patch makes games send their lighting to Artemis instead of LGS or LGHUB";

        public override List<PluginPrerequisiteAction> InstallActions { get; }

        public override List<PluginPrerequisiteAction> UninstallActions { get; }

        public override bool IsMet()
        {
            using RegistryKey key64 = Registry.LocalMachine.OpenSubKey(REGISTRY_PATH_64);
            using RegistryKey key32 = Registry.LocalMachine.OpenSubKey(REGISTRY_PATH_32);

            bool is64BitKeyPresent = key64?.GetValue(null)?.ToString() == _wrapperPath64;
            bool is32BitKeyPresent = key32?.GetValue(null)?.ToString() == _wrapperPath32;

            return is64BitKeyPresent && is32BitKeyPresent;
        }

        public LogitechWrapperPrerequisite(Plugin plugin)
        {
            _wrapperPath64 = Path.Combine(plugin.Directory.FullName, "x64", DLL_NAME);
            _wrapperPath32 = Path.Combine(plugin.Directory.FullName, "x86", DLL_NAME);

            string patchScript = plugin.ResolveRelativePath("Scripts\\patch-registry.ps1");
            string unpatchScript = plugin.ResolveRelativePath("Scripts\\unpatch-registry.ps1");
            InstallActions = new List<PluginPrerequisiteAction>
            {
                new RunPowerShellAction("Patch 64 bit registry", patchScript, true, $"\"HKLM:{REGISTRY_PATH_64}\" \"{_wrapperPath64}\""),
                new RunPowerShellAction("Patch 32 bit registry", patchScript, true, $"\"HKLM:{REGISTRY_PATH_32}\" \"{_wrapperPath32}\"")
            };

            UninstallActions = new List<PluginPrerequisiteAction>
            {
                new RunPowerShellAction("Patch 64 bit registry", unpatchScript, true, $"\"HKLM:{REGISTRY_PATH_64}\""),
                new RunPowerShellAction("Patch 32 bit registry", unpatchScript, true, $"\"HKLM:{REGISTRY_PATH_32}\"")
            };
        }

        private const string DLL_NAME = "Artemis.Wrappers.Logitech.dll";
        private const string REGISTRY_PATH_64 = "SOFTWARE\\Classes\\CLSID\\{a6519e67-7632-4375-afdf-caa889744403}\\ServerBinary";
        private const string REGISTRY_PATH_32 = "SOFTWARE\\Classes\\WOW6432Node\\CLSID\\{a6519e67-7632-4375-afdf-caa889744403}\\ServerBinary";
        private readonly string _wrapperPath64;
        private readonly string _wrapperPath32;
    }
}