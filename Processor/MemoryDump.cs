using System.Diagnostics;
using Crestron.SimplSharp;
using Microsoft.Diagnostics.NETCore.Client;
using Serilog;
using Directory = Crestron.SimplSharp.CrestronIO.Directory;
using Interlocked = System.Threading.Interlocked;

namespace AVCoders.Crestron.Processor;

public class MemoryDump
{
    private readonly int _processId = Process.GetCurrentProcess().Id;
    private int _isDumping;

    public MemoryDump()
    {
        CrestronConsole.AddNewConsoleCommand(HandleDumpCommand, "dumpheap",
            "Dump the current heap to the simpl app folder for this slot",
            ConsoleAccessLevelEnum.AccessOperator);
    }

    private void HandleDumpCommand(string args)
    {
        CaptureDump();
    }

    public void CaptureDump()
    {
        if (Interlocked.CompareExchange(ref _isDumping, 1, 0) != 0)
        {
            Log.Warning("A memory dump is already in progress");
            return;
        }

        try
        {
            string appDir = Directory.GetApplicationDirectory();
            string dumpPath = Path.Combine(appDir, $"{DateTime.Now:yyyyMMdd_HHmmss}.dmp");
            var client = new DiagnosticsClient(_processId);

            client.WriteDump(DumpType.WithHeap, dumpPath);

            var fileInfo = new FileInfo(dumpPath);
            Log.Information("Dump saved to {DumpPath} ({SizeMb:F1} MB)", dumpPath, fileInfo.Length / 1048576.0);

            PruneOldDumps(appDir, dumpPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Dump failed");
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
                Log.Information("Pruned old dump {FileName}", file.Name);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to prune old dumps");
        }
    }
}
