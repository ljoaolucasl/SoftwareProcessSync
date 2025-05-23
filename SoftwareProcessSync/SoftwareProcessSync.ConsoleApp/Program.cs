using System.Text.Json;
using SoftwareProcessSync.Softwares;

namespace SoftwareProcessSync;

public static class Program
{
    private static void Main()
    {
        if (OperatingSystem.IsWindows())
        {
            var softwaresWithCounters = SoftwareManager.GetSoftwares();

            var outDir = Path.Combine(AppContext.BaseDirectory, "SoftwareMonitor");
            Directory.CreateDirectory(outDir);

            File.WriteAllText(Path.Combine(outDir, "softwares.json"), JsonSerializer.Serialize(softwaresWithCounters, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}