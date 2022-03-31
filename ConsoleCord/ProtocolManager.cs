using c = System.Console;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;

namespace ConsoleCord
{
    internal class ProtocolManager
    {
        public enum InstallType
        {
            install,
            remove
        }

        public static void Elevate(InstallType type)
        {
            ProcessStartInfo psi = new();
            psi.UseShellExecute = true;
            psi.Verb = "runas";
            psi.FileName = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!}\ConsoleCord.exe";
            psi.Arguments = type == InstallType.install ? "--setupProtocolAdmin" : "--removeProtocolAdmin";
            var protocolProcess = Process.Start(psi) ?? throw new Exception($"Error elevating user permissions. Could not {(type == InstallType.install ? "install" : "uninstall")} protocol.");
            protocolProcess.EnableRaisingEvents = true;
            protocolProcess.Exited += new EventHandler(delegate {
                c.WriteLine($"Successfully {(type == InstallType.install ? "installed" : "uninstalled")} protocol.");
                protocolProcess.Dispose();
                Environment.Exit(0);
            });
            c.ReadLine();
        }

        public static void InstallProtocol()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                c.WriteLine("Attempting to install protocol, please wait...");
                var key = Registry.ClassesRoot.OpenSubKey("cctp");
                if (key is null)
                {
                    key = Registry.ClassesRoot.CreateSubKey("cctp");
                    key.SetValue(string.Empty, "URL: cctp Protocol");
                    key.SetValue("URL Protocol", string.Empty);
                    key = key.CreateSubKey(@"shell\open\command");
                    key.SetValue(string.Empty, $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!}\\ConsoleCord.exe %1");
                }
                else
                { c.WriteLine("Protocol has already been registered. Press any key to continue."); c.ReadLine(); return; }
                key.Close();
            }
            else
            { c.WriteLine("This operation is only valid on Windows devices. Press any key co continue."); c.ReadLine(); return; }
        }

        public static void UninstallProtocol()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                c.WriteLine("Attempting to uninstall protocol, please wait...");
                Registry.ClassesRoot.DeleteSubKeyTree("cctp");
            }
        }
    }
}
