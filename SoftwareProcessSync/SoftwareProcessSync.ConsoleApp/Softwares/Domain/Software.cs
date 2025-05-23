namespace SoftwareProcessSync.Softwares.Domain;

/// <summary>
/// Representa um software instalado, com informações de nome, versão,
/// publicadora, data de instalação, caminho do executável e IDs de processos ativos.
/// </summary>
public sealed class Software
{
    /// <summary>
    /// Construtor que inicializa as propriedades básicas do software.
    /// </summary>
    /// <param name="name">Nome amigável do software.</param>
    /// <param name="version">Versão instalada do software.</param>
    /// <param name="publisher">Nome da publicadora/fabricante.</param>
    /// <param name="installDate">Data de instalação no formato ISO 8601, ou <c>null</c>.</param>
    /// <param name="path">Caminho completo do executável, ou <c>null</c>.</param>
    public Software(string name, string version, string publisher, string? installDate, string? path)
    {
        Name = name;
        Version = version;
        Publisher = publisher;
        InstallDate = installDate;
        Path = path;
        ProcessesIds = [];
    }

    /// <summary>
    /// Nome amigável do software conforme exibido ao usuário.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Versão instalada do software.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Nome da publicadora ou fabricante do software.
    /// </summary>
    public string Publisher { get; set; }

    /// <summary>
    /// Data de instalação em formato ISO 8601 (usando 'o'), ou <c>null</c> se não disponível.
    /// </summary>
    public string? InstallDate { get; set; }

    /// <summary>
    /// Caminho completo para o executável principal do software, ou <c>null</c> se não resolvido.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Lista de IDs de processos (<see cref="Process.Id"/>) atualmente em execução
    /// que correspondem a este software.
    /// </summary>
    public List<int> ProcessesIds { get; set; }
}