using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using Dynamo.Extensions;
// ReSharper disable UnusedMember.Global

namespace archilabExtension
{
    public class ArchilabExtension : IExtension
    {
        public void Startup(StartupParams sp)
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
            using (var zipToOpen = new FileStream(zipPath, FileMode.Open))
            {
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    archive.ExtractToDirectory(unzippedDir, true);
                }
            }

            if (!Directory.Exists(unzippedDir))
                return;

            var pathUi = Path.Combine(Path.Combine(binDir, @"..\"), $"extra\\20{version}\\archilabUI20{version}.dll");
            var path = Path.Combine(Path.Combine(binDir, @"..\"), $"extra\\20{version}\\archilab20{version}.dll");
            if (!File.Exists(path) || !File.Exists(pathUi))
                return;

            sp.LibraryLoader.LoadNodeLibrary(Assembly.LoadFrom(path));
            sp.LibraryLoader.LoadNodeLibrary(Assembly.LoadFrom(pathUi));

            // (Konrad) Load the View Extension as a Node Library so that the Relay Node Model is loaded.
            var viewExtensionPath = Path.Combine(binDir, "archilabViewExtension.dll");
            if (File.Exists(viewExtensionPath))
                sp.LibraryLoader.LoadNodeLibrary(Assembly.LoadFrom(viewExtensionPath));
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

        public void Ready(ReadyParams sp)
        {
        }

        public void Shutdown()
        {
        }
    }

    public static class ZipArchiveExtensions
    {
        public static void ExtractToDirectory(this ZipArchive za, string dir, bool overwrite)
        {
            if (!overwrite)
            {
                za.ExtractToDirectory(dir);
                return;
            }

            var di = Directory.CreateDirectory(dir);
            var dirFullPath = di.FullName;

            foreach (var file in za.Entries)
            {
                var completeFileName = Path.GetFullPath(Path.Combine(dirFullPath, file.FullName));

                if (!completeFileName.StartsWith(dirFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new IOException("Trying to extract file outside of destination directory.");
                }

                if (file.Name == "")
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                    continue;
                }

                file.ExtractToFile(completeFileName, true);
            }
        }
    }
}
