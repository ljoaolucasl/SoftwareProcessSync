using System.Diagnostics;
using SoftwareProcessSync.Helpers;

namespace SoftwareProcessSync.Resolvers;

/// <summary>
/// Resolve o caminho do executável principal de um software a partir de
/// diferentes fontes: diretório de instalação, atalhos do Menu Iniciar e ícone.
/// </summary>
public static class PathResolver
{
    private static readonly StartMenuScannerHelper _scanner = new();

    /// <summary>
    /// Tenta encontrar o caminho completo do executável de um software
    /// usando, na ordem:
    /// 1. Diretório de instalação (<paramref name="installLoc"/> + heurística de nome/descrição),
    /// 2. Atalhos do Menu Iniciar,
    /// 3. Caminho direto de <paramref name="displayIcon"/>.
    /// </summary>
    /// <param name="displayIcon">
    /// Valor de DisplayIcon do registro, possivelmente no formato "caminho,índice".
    /// </param>
    /// <param name="installLoc">
    /// Valor de InstallLocation do registro, pasta onde o software pode estar instalado.
    /// </param>
    /// <param name="displayName">
    /// Nome amigável do software (usado para matching de nome de arquivo ou descrição).
    /// </param>
    /// <returns>
    /// Caminho completo do executável, ou <c>null</c> se não for possível resolver.
    /// </returns>
    public static string? Resolve(string? displayIcon, string? installLoc, string displayName)
    {
        if (TryResolveFromDirectory(installLoc, displayName, out var path))
            return path;

        if (TryResolveFromScanner(displayName, out path))
            return path;

        if (TryResolveFromIcon(displayIcon, out path))
            return path;

        return null;
    }

    /// <summary>
    /// Tenta resolver o executável a partir do diretório de instalação.
    /// Primeiro busca arquivo com mesmo nome, depois compara
    /// ProductName/FileDescription e por fim escolhe o maior .exe.
    /// </summary>
    /// <param name="installLoc">Pasta onde o software foi instalado.</param>
    /// <param name="displayName">Nome do software para matching.</param>
    /// <param name="path">Recebe o caminho se encontrado.</param>
    /// <returns><c>true</c> se encontrou um executável válido; caso contrário <c>false</c>.</returns>
    private static bool TryResolveFromDirectory(string? installLoc, string displayName, out string? path)
    {
        path = null;
        if (string.IsNullOrWhiteSpace(installLoc) || !Directory.Exists(installLoc))
            return false;

        var executables = Directory.EnumerateFiles(installLoc, "*.exe", SearchOption.TopDirectoryOnly);

        path = executables.FirstOrDefault(f =>
            string.Equals(Path.GetFileNameWithoutExtension(f), displayName, StringComparison.OrdinalIgnoreCase));

        if (path != null)
            return true;

        foreach (var exeLoc in executables)
            try
            {
                var fvi = FileVersionInfo.GetVersionInfo(exeLoc);
                var description = fvi.ProductName ?? fvi.FileDescription;

                if (!string.IsNullOrWhiteSpace(description) &&
                    (description.Contains(displayName, StringComparison.OrdinalIgnoreCase) ||
                     displayName.Contains(description, StringComparison.OrdinalIgnoreCase)))
                {
                    path = exeLoc;
                    return true;
                }
            }
            catch { }

        path = executables
            .Select(f => new FileInfo(f))
            .OrderByDescending(fi => fi.Length)
            .FirstOrDefault()?.FullName;

        return path != null;
    }

    /// <summary>
    /// Tenta resolver o executável usando atalhos do Menu Iniciar
    /// previamente indexados pelo <see cref="StartMenuScannerHelper"/>.
    /// </summary>
    /// <param name="displayName">Nome de exibição do software.</param>
    /// <param name="path">Recebe o caminho se encontrado.</param>
    /// <returns><c>true</c> se encontrou no cache; caso contrário <c>false</c>.</returns>
    private static bool TryResolveFromScanner(string displayName, out string? path)
    {
        path = _scanner.Lookup(displayName).FirstOrDefault();
        return path != null;
    }

    /// <summary>
    /// Tenta resolver o executável a partir de um valor DisplayIcon
    /// que contenha caminho absoluto antes de uma vírgula.
    /// </summary>
    /// <param name="displayIcon">String do registro com o ícone/display path.</param>
    /// <param name="path">Recebe o caminho se o arquivo existir.</param>
    /// <returns><c>true</c> se o arquivo existir; caso contrário <c>false</c>.</returns>
    private static bool TryResolveFromIcon(string? displayIcon, out string? path)
    {
        path = null;
        if (string.IsNullOrWhiteSpace(displayIcon))
            return false;

        var iconPath = displayIcon.Split(',')[0].Trim('"');
        if (File.Exists(iconPath))
        {
            path = iconPath;
            return true;
        }

        return false;
    }
}