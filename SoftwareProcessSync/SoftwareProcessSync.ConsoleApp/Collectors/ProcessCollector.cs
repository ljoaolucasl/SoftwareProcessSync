using System.Diagnostics;
using System.Management;
using System.Runtime.Versioning;

namespace SoftwareProcessSync.Collectors;

/// <summary>
/// Responsável por coletar processos ativos no sistema, agrupando-os pelo caminho
/// do executável retornado pelo WMI (Win32_Process).
/// </summary>
[SupportedOSPlatform("windows")]
public static class ProcessCollector
{
    /// <summary>
    /// Busca todos os processos via WMI (<c>Win32_Process</c>), obtendo o <c>ProcessId</c>
    /// e o <c>ExecutablePath</c>, e então agrupa as instâncias de <see cref="Process"/>
    /// pelo caminho completo do executável (em lowercase).
    /// </summary>
    /// <returns>
    /// Dicionário onde a chave é o caminho completo do executável (em lowercase) e
    /// o valor é a lista de processos correspondentes que estão em execução.
    /// </returns>
    public static Dictionary<string, List<Process>> Collect()
    {
        var runningGroups = new Dictionary<string, List<Process>>(StringComparer.OrdinalIgnoreCase);

        var wmiSearcher = new ManagementObjectSearcher("SELECT ProcessId, ExecutablePath FROM Win32_Process");
        foreach (var mo in wmiSearcher.Get())
        {
            var execPath = mo["ExecutablePath"] as string;

            if (string.IsNullOrWhiteSpace(execPath))
                continue;

            if (!uint.TryParse(mo["ProcessId"]?.ToString(), out var pid))
                continue;

            var pathKey = execPath.ToLowerInvariant();
            try
            {
                var proc = Process.GetProcessById((int)pid);
                if (!runningGroups.TryGetValue(pathKey, out var list))
                {
                    list = [];
                    runningGroups[pathKey] = list;
                }

                list.Add(proc);
            }
            catch { }
        }

        return runningGroups;
    }
}