using System.Globalization;

namespace SoftwareProcessSync.Extensions;

/// <summary>
/// Extensões de <see cref="string"/> para conversão de formatos de data.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converte uma string no formato <c>yyyyMMdd</c> em um ISO 8601
    /// (<c>o</c>) se o parse for bem-sucedido.
    /// </summary>
    /// <param name="installDate">
    /// String contendo a data no formato <c>yyyyMMdd</c>, ou <c>null</c>.
    /// </param>
    /// <returns>
    /// A data convertida em ISO 8601 (por exemplo, <c>2025-05-23T00:00:00.0000000</c>),
    /// ou <c>null</c> se a string de entrada for <c>null</c> ou não corresponder ao formato esperado.
    /// </returns>
    public static string? ToISOString(this string? installDate)
    {
        return DateTime.TryParseExact(installDate, "yyyyMMdd", null, DateTimeStyles.AssumeLocal, out var dateTime)
            ? dateTime.ToString("o")
            : null;
    }
}