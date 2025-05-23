using System.Text.RegularExpressions;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace SoftwareProcessSync.Helpers;

/// <summary>
/// Escaneia os atalhos do Menu Iniciar (comum e do usuário) e
/// mantém um cache de names de atalhos para caminhos de executáveis.
/// </summary>
public sealed partial class StartMenuScannerHelper
{
    private readonly Dictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Regex _exeRegex = ExeFileRegex();

    /// <summary>
    /// Inicializa o scanner e popula o cache de atalhos para executáveis.
    /// </summary>
    public StartMenuScannerHelper() => PopulateCache();

    /// <summary>
    /// Retorna os diretórios de Menu Iniciar para todos os usuários e para o usuário atual.
    /// </summary>
    private static IEnumerable<string> StartMenuDirs()
    {
        yield return Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
        yield return Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
    }

    /// <summary>
    /// Varre recursivamente os atalhos (*.lnk) nos diretórios do Menu Iniciar,
    /// extrai o caminho de destino dos que apontam para executáveis válidos
    /// e armazena no cache para lookup rápido.
    /// </summary>
    private void PopulateCache()
    {
        var shell = new WshShell();
        foreach (var dir in StartMenuDirs().Where(Directory.Exists))
        {
            var opts = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true };
            foreach (var file in Directory.EnumerateFiles(dir, "*.lnk", opts))
            {
                try
                {
                    IWshShortcut s = shell.CreateShortcut(file);
                    if (string.IsNullOrWhiteSpace(s.TargetPath) || !_exeRegex.IsMatch(s.TargetPath) || !File.Exists(s.TargetPath))
                        continue;

                    _cache[Path.GetFileNameWithoutExtension(file)] = s.TargetPath;
                }
                catch { /* skip corrupted */ }
            }
        }
    }

    /// <summary>
    /// Regex gerada para identificar arquivos com extensão .exe (case-insensitive).
    /// </summary>
    [GeneratedRegex("\\.exe$", RegexOptions.IgnoreCase)]
    private static partial Regex ExeFileRegex();

    /// <summary>
    /// Procura no cache um executável cujo atalho tenha o nome informado.
    /// </summary>
    /// <param name="displayName">Nome do atalho (sem extensão) a ser buscado.</param>
    /// <returns>
    /// Caminho completo do executável correspondente, ou sequência vazia se não encontrado.
    /// </returns>
    public IEnumerable<string> Lookup(string displayName) => _cache.TryGetValue(displayName, out var exe) ? [exe] : [];
}