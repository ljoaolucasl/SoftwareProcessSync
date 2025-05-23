using System.Diagnostics;
using SoftwareProcessSync.Softwares.Domain;

namespace SoftwareProcessSync.Softwares.Services;

/// <summary>
/// Serviço que associa processos em execução às instâncias de <see cref="Software"/>,
/// preenchendo a lista de IDs de processo para cada software conhecido.
/// </summary>
public static class ProcessSoftwareSyncService
{
    /// <summary>
    /// Sincroniza a lista de <see cref="Software"/> com o dicionário de processos ativos,
    /// adicionando em cada software os IDs de todos os processos que correspondem ao seu executável,
    /// ao seu diretório pai ou ao seu nome.
    /// </summary>
    /// <param name="softwares">Lista de softwares instalados a serem atualizados.</param>
    /// <param name="processes">
    /// Dicionário cujo a chave é o caminho completo do executável e o valor
    /// é a lista de instâncias de <see cref="Process"/> em execução.
    /// </param>
    /// <returns>A mesma lista de <paramref name="softwares"/>, com os campos <c>ProcessesIds</c> atualizados.</returns>
    public static List<Software> Sync(List<Software> softwares, Dictionary<string, List<Process>> processes)
    {
        foreach (var software in softwares)
        {
            if (software.Path != null)
            {
                if (processes.TryGetValue(software.Path, out var value))
                {
                    software.ProcessesIds.AddRange(value.Select(x => x.Id));
                    continue;
                }

                var parentDir = Path.GetDirectoryName(software.Path);
                value = FindProcessesByKey(processes, key => !string.IsNullOrEmpty(parentDir) && key.Contains(parentDir, StringComparison.OrdinalIgnoreCase));

                if (value != null)
                {
                    software.ProcessesIds.AddRange(value.Select(x => x.Id));
                    continue;
                }
            }

            if (software.Name != null)
            {
                var value = FindProcessesByKey(processes, key => key.Contains(software.Name, StringComparison.OrdinalIgnoreCase));

                if (value != null)
                {
                    software.ProcessesIds.AddRange(value.Select(x => x.Id));
                    continue;
                }
            }
        }

        return softwares;
    }

    /// <summary>
    /// Procura um grupo de processos cuja chave do dicionário satisfaz o predicado informado.
    /// </summary>
    /// <param name="processes">
    /// Dicionário de processos em que a chave é o caminho do executável.
    /// </param>
    /// <param name="predicate">Função que avalia a chave para encontrar correspondência.</param>
    /// <returns>
    /// A lista de processos cujo caminho de executável atende ao <paramref name="predicate"/>,
    /// ou <c>null</c> se nenhum for encontrado.
    /// </returns>
    private static List<Process>? FindProcessesByKey(Dictionary<string, List<Process>> processes, Func<string, bool> predicate)
    {
        var match = processes.Keys.FirstOrDefault(predicate);
        return match != null ? processes[match] : null;
    }
}