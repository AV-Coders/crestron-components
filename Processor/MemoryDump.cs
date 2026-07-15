using System.Diagnostics;
using AVCoders.Core;
using Crestron.SimplSharp;
using Microsoft.Diagnostics.NETCore.Client;
using Directory = Crestron.SimplSharp.CrestronIO.Directory;
using Interlocked = System.Threading.Interlocked;

namespace AVCoders.Crestron.Processor;

public class MemoryDump : LogBase
{
    private readonly int _processId = Process.GetCurrentProcess().Id;
    private int _isDumping;

    public MemoryDump() : base("MemoryDump")
    {
        CrestronConsole.AddNewConsoleCommand(HandleDumpCommand, "dumpheap",
            "Dump the current heap to the simpl app folder for this slot",
            ConsoleAccessLevelEnum.AccessAdministrator);
    }

    private void HandleDumpCommand(string args)
    {
        CaptureDump();
    }

    public void CaptureDump()
    {
        if (Interlocked.CompareExchange(ref _isDumping, 1, 0) != 0)
        {
            LogWarning("A memory dump is already in progress");
            return;
        }

        try
        {
            string appDir = Directory.GetApplicationDirectory();
            string dumpPath = Path.Combine(appDir, $"{DateTime.Now:yyyyMMdd_HHmmss}.dmp");
            var client = new DiagnosticsClient(_processId);

            client.WriteDump(DumpType.WithHeap, dumpPath);

            var fileInfo = new FileInfo(dumpPath);
            LogInformation("Dump saved to {DumpPath} ({SizeMb:F1} MB)", dumpPath, fileInfo.Length / 1048576.0);

            PruneOldDumps(appDir, dumpPath);
        }
        catch (Exception ex)
        {
            LogError(ex, "Dump failed");
        }
        finally
        {
            Interlocked.Exchange(ref _isDumping, 0);
        }
    }

    private void PruneOldDumps(string directory, string currentDumpPath)
    {
        try
        {
            var dumpFiles = new DirectoryInfo(directory)
                .GetFiles("*.dmp")
                .Where(f => f.FullName != currentDumpPath);

            foreach (var file in dumpFiles)
            {
                file.Delete();
                LogInformation("Pruned old dump {FileName}", file.Name);
            }
        }
        catch (Exception ex)
        {
            LogWarning("Failed to prune old dumps: {Message}", ex.Message);
        }
    }
}
