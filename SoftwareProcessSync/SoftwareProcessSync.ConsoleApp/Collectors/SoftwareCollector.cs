using System.Management;
using System.Runtime.Versioning;
using Microsoft.Win32;
using SoftwareProcessSync.Extensions;
using SoftwareProcessSync.Resolvers;
using SoftwareProcessSync.Softwares.Domain;

namespace SoftwareProcessSync.Collectors;

/// <summary>
/// Coleta informações de softwares instalados na máquina Windows,
/// unindo dados do registro e de WMI (Win32_Product) e mesclando
/// em uma lista única sem duplicações.
/// </summary>
[SupportedOSPlatform("windows")]
public static class SoftwareCollector
{
    /// <summary>
    /// Chaves de registro onde o Windows armazena informações sobre
    /// programas instalados (32 e 64 bits, HKLM e HKCU).
    /// </summary>
    private static readonly (RegistryKey, string)[] _registryKeys =
    [
        (Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
        (Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"),
        (Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
        (Registry.CurrentUser, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall")
    ];

    /// <summary>
    /// Executa a coleta de softwares via registro e via WMI, agrupa
    /// por Nome, Versão e Publisher, e retorna a lista resultante.
    /// </summary>
    /// <returns>
    /// Lista de <see cref="Software"/> representando cada aplicação
    /// instalada, preferindo aquelas entradas que fornecem um caminho
    /// válido (<c>Path</c>).
    /// </returns>
    public static List<Software> Collect()
    {
        var regSofts = CollectFromRegistry();
        var wmiSofts = CollectFromWmi();

        var softwares = regSofts
                .Concat(wmiSofts)
                .GroupBy(s => new { s.Name, s.Version, s.Publisher })
                .Select(g =>
                    g.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s.Path))
                    ?? g.First())
                .ToList();

        return softwares;
    }

    /// <summary>
    /// Lê o registro nas chaves definidas em <c>_registryKeys</c> e
    /// retorna informações básicas de cada software ali listado.
    /// </summary>
    private static IEnumerable<Software> CollectFromRegistry()
    {
        foreach (var (rootKey, keyPath) in _registryKeys)
        {
            using var key = rootKey.OpenSubKey(keyPath);
            if (key is null)
                continue;

            foreach (var software in key.GetSubKeyNames())
            {
                var softwareInfo = GetSoftwareFromRegistryKey(key, software);
                if (softwareInfo is not null)
                    yield return softwareInfo;
            }
        }
    }

    /// <summary>
    /// Extrai dados de uma subchave de registro representando um software:
    /// Nome, Versão, Publisher, Data de Instalação e tenta resolver o caminho
    /// do executável principal.
    /// </summary>
    /// <param name="parentKey">Chave pai onde está a subchave.</param>
    /// <param name="subKeyName">Nome da subchave correspondente ao software.</param>
    /// <returns>
    /// Instância de <see cref="Software"/> se os campos mínimos existirem;
    /// caso contrário, <c>null</c>.
    /// </returns>
    private static Software? GetSoftwareFromRegistryKey(RegistryKey parentKey, string subKeyName)
    {
        using var subKey = parentKey.OpenSubKey(subKeyName);
        if (subKey is null)
            return null;

        var name = subKey.GetValue("DisplayName") as string;
        var version = subKey.GetValue("DisplayVersion") as string;
        var publisher = subKey.GetValue("Publisher") as string;
        var installDate = (subKey.GetValue("InstallDate") as string)?.ToISOString();

        var icon = subKey.GetValue("DisplayIcon") as string;
        var installLoc = subKey.GetValue("InstallLocation") as string;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(version) || string.IsNullOrEmpty(publisher))
            return null;

        var path = PathResolver.Resolve(icon, installLoc, name);

        return new Software(name, version, publisher, installDate, path);
    }

    /// <summary>
    /// Consulta a classe WMI <c>Win32_Product</c> para obter softwares
    /// instalados via MSI, extraindo Nome, Versão, Vendor, Data de Instalação
    /// e pasta de instalação.
    /// </summary>
    private static IEnumerable<Software> CollectFromWmi()
    {
        var query = new
        {
            Table = "Win32_Product",
            Properties = "Name, Version, Vendor, InstallDate, InstallLocation",
            Filter = "Name IS NOT NULL AND Name <> '' AND Version IS NOT NULL AND Version <> '' AND Vendor IS NOT NULL AND Vendor <> ''"
        };

        using var searcher = new ManagementObjectSearcher($"SELECT {query.Properties} FROM {query.Table} WHERE {query.Filter}");

        foreach (var mo in searcher.Get().Cast<ManagementObject>())
        {
            var name = mo["Name"] as string;
            var version = mo["Version"] as string;
            var publisher = mo["Vendor"] as string;
            var installDate = (mo["InstallDate"] as string)?.ToISOString();
            var installLoc = mo["InstallLocation"] as string;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(publisher))
                continue;

            var path = PathResolver.Resolve(null, installLoc, name);

            yield return new Software(name, version, publisher, installDate, path);
        }
    }
}