using System.Runtime.Versioning;
using SoftwareProcessSync.Collectors;
using SoftwareProcessSync.Softwares.Domain;
using SoftwareProcessSync.Softwares.Services;

namespace SoftwareProcessSync.Softwares;

/// <summary>
/// Pontos de entrada para obtenção da lista de <see cref="Software"/>,
/// combinando a coleta de softwares instalados e a associação com processos ativos.
/// </summary>
[SupportedOSPlatform("windows")]
public static class SoftwareManager
{
    /// <summary>
    /// Retorna a lista de softwares instalados na máquina,
    /// enriquecida com os IDs dos processos em execução associados a cada um.
    /// </summary>
    /// <returns>
    /// Lista de <see cref="Software"/> contendo Nome, Versão, Publicadora, Path,
    /// e IDs dos processos atualmente em execução que correspondem a cada software.
    /// </returns>
    public static List<Software> GetSoftwares()
    {
        var softwares = SoftwareCollector.Collect();
        var processes = ProcessCollector.Collect();

        return ProcessSoftwareSyncService.Sync(softwares, processes);
    }
}