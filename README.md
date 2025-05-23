# Software Process Sync PoC

Este repositório contém uma prova de conceito (PoC) em C# chamada **SoftwareProcessSync**, cujo objetivo principal é **sincronizar softwares instalados** em uma máquina Windows com os **processos em execução**. O resultado é uma lista de objetos `Software` que inclui, além dos dados estáticos de instalação, a lista de IDs de processo associados a cada software.

## 🚀 Features

* **Inventário de Softwares Instalados**

  * Lê chaves de registro (HKLM/HKCU) para 32-bit e 64-bit.
  * Consulta via WMI (`Win32_Product`) para pacotes MSI.
  * Resolve o caminho do executável principal usando heurísticas:

    * Diretório de instalação (`InstallLocation`).
    * Atalhos do Menu Iniciar.
    * `DisplayIcon` do registro.

* **Sincronização com Processos Ativos**

  * Enumera processos via WMI (`Win32_Process`), obtendo `ExecutablePath` e `ProcessId`.
  * Agrupa processos pelo caminho do executável.
  * Para cada software, associa IDs de processo por match exato de path, diretório pai ou nome do software.

* **Modelo de Domínio**

  * Classe `Software` com propriedades:

    * `Name`, `Version`, `Publisher`, `InstallDate`, `Path`.
    * `List<int> ProcessesIds` para IDs dos processos encontrados.

* **Exportação**

  * Gera arquivo `softwares.json` contendo a lista completa de `Software` com processos.

## 📋 Pré-requisitos

* Windows 10 ou superior
* .NET 6 SDK ou superior
* Visual Studio 2022 / VS Code ou equivalente
* Permissão de leitura em registro e WMI (normalmente usuário padrão)

## 🛠️ Instalação e Build

1. Clone este repositório:

   ```bash
   git clone https://github.com/ljoaolucasl/SoftwareProcessSync.git
   ```
2. Acesse a pasta e compile:

   ```bash
   cd SoftwareProcessSync
   dotnet restore
   dotnet build
   ```

## 🚀 Uso

Execute o binário gerado para sincronizar softwares e processos:

```bash
cd bin/Debug/net8.0
SoftwareProcessSync.exe
```

Isto criará a pasta `SoftwareMonitor` (no mesmo diretório) e gerará:

* `softwares.json`: lista de softwares com campos estáticos **e** lista `ProcessesIds` dos processos em execução.

Abra `softwares.json` em seu editor ou importe para sistemas de análise.

## 📂 Estrutura do Projeto

```text
SoftwareProcessSync/
├── Program.cs                          # Ponto de entrada: chama SoftwareManager
├── Collectors/
│   ├── SoftwareCollector.cs            # Coleta dados estáticos do registro e WMI
│   └── ProcessCollector.cs             # Lista processos via WMI e agrupa por path
├── Resolvers/
│   └── PathResolver.cs                 # Heurísticas para achar executável principal
├── Helpers/
│   └── StartMenuScannerHelper.cs       # Indexa atalhos do Menu Iniciar
├── Services/
│   └── ProcessSoftwareSyncService.cs   # Associa processos a softwares
├── Extensions/
│   └── StringExtensions.cs             # Conversão de datas (yyyyMMdd → ISO)
├── Softwares/
│   ├── Domain/
│   │   └── Software.cs                 # Modelo de domínio com ProcessIds
│   └── SoftwareManager.cs              # Orquestra fluxo de sincronização
```

## 📄 Licença

Este projeto está licenciado sob a [MIT License](LICENSE).
