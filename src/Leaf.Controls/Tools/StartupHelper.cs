using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Windows;

namespace Leaf.Controls.Tools
{
    public class StartupHelper
    {
        public static void EnsureProfileOptimization()
        {
            var cachePath = $"{AppDomain.CurrentDomain.BaseDirectory}Cache";
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }
            ProfileOptimization.SetProfileRoot(cachePath);
            ProfileOptimization.StartProfile("Profile");
        }

        public static Mutex EnsureSingleton(Application app)
        {
            var mutex = new Mutex(
                true,
                Assembly.GetExecutingAssembly().GetName().Name,
                out var createdNew
            );

            if (createdNew)
            {
                return mutex;
            }

            var current = Process.GetCurrentProcess();

            foreach (var process in Process.GetProcessesByName(current.ProcessName))
            {
                if (process.Id != current.Id)
                {
                    Win32Helper.SetForegroundWindow(process.MainWindowHandle);
                    break;
                }
            }
            app.Shutdown();
            return mutex;
        }
    }
}
