using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using Dynamo.Wpf.Extensions;
// ReSharper disable UnusedMember.Global
// ReSharper disable SwitchStatementMissingSomeCases

namespace archilabLoader
{
    public class ArchilabLoaderExtension : IViewExtension
    {
        public void Startup(ViewStartupParams p)
        {
            var revitProcess = Process.GetCurrentProcess();
            var revitFilePath = revitProcess.MainModule?.FileName;
            if (string.IsNullOrWhiteSpace(revitFilePath))
                return;

            var binDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrWhiteSpace(binDir))
                return;

            var version = FileVersionInfo.GetVersionInfo(revitFilePath).FileMajorPart;
            var zipPath = Path.Combine(Path.Combine(binDir, @"..\"), $"extra\\20{version}.zip");
            if (!File.Exists(zipPath))
                return;

            var unzippedDir = Path.Combine(Path.Combine(binDir, @"..\"), $"extra\\20{version}");
            if (!Directory.Exists(unzippedDir))
                ZipFile.ExtractToDirectory(zipPath, unzippedDir);
            if (!Directory.Exists(unzippedDir))
                return;

            var path = Path.Combine(Path.Combine(binDir, @"..\"), $"extra\\20{version}\\archilab20{version}.dll");
            var pathUi = Path.Combine(Path.Combine(binDir, @"..\"), $"extra\\20{version}\\archilabUI20{version}.dll");
            if (!File.Exists(path) || !File.Exists(pathUi))
                return;

            p.LibraryLoader.LoadNodeLibrary(Assembly.LoadFrom(path));
            p.LibraryLoader.LoadNodeLibrary(Assembly.LoadFrom(pathUi));

            // (Konrad) Load the View Extension as a Node Library so that the Relay Node Model is loaded.
            var viewExtensionPath = Path.Combine(binDir, "archilabViewExtension.dll");
            if (File.Exists(viewExtensionPath))
                p.LibraryLoader.LoadNodeLibrary(Assembly.LoadFrom(viewExtensionPath));
        }

        public void Loaded(ViewLoadedParams p)
        {
        }

        public void Shutdown()
        {
        }

        public string UniqueId
        {
            get { return "2d25bdf8-6f50-40fb-8111-a8069c15824f"; }
        }

        public string Name
        {
            get { return "archilab"; }
        }

        public void Dispose()
        {
        }
    }
}
