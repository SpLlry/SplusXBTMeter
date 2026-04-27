using System;
using System.IO;
using System.Reflection;

namespace SplusXBTMeter
{
    internal static class Program
    {
        [STAThread]
        static void Main1()
        {
            // ✅ 在 WPF 启动前就接管 AssemblyResolve
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        private static Assembly? ResolveAssembly(object sender, ResolveEventArgs args)
        {
            try
            {
                File.AppendAllText(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"),
                    $"[Resolve] {args.Name}\n"
                );
                var name = new AssemblyName(args.Name).Name + ".dll";
                var libsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", name);

                if (File.Exists(libsPath))
                    return Assembly.LoadFrom(libsPath);

                var rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
                if (File.Exists(rootPath))
                    return Assembly.LoadFrom(rootPath);
            }
            catch
            {
                // ignore
            }
            return null;
        }
    }
}
