using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace OnliDeskWrapper
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                // Configurar para aplicação Windows Forms
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                // Verificar se dotnet está disponível
                if (!IsDotNetAvailable())
                {
                    ShowDotNetInstallDialog();
                    return;
                }
                
                // Tentar executar o OnliDesk
                if (!LaunchOnliDesk())
                {
                    MessageBox.Show(
                        "Erro ao executar OnliDesk!\n\n" +
                        "Verifique se os arquivos estão na mesma pasta:\n" +
                        "• OnliDesk.exe (este arquivo)\n" +
                        "• OliAcessoRemoto.exe ou OliAcessoRemoto.csproj\n\n" +
                        "Se o problema persistir, execute OnliDesk.bat",
                        "OnliDesk - Erro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro inesperado: {ex.Message}\n\n" +
                    "Tente executar OnliDesk.bat como alternativa.",
                    "OnliDesk - Erro Crítico",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        
        static bool IsDotNetAvailable()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = "--list-runtimes",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                // Verificar se tem .NET 8 Desktop Runtime
                return output.Contains("Microsoft.WindowsDesktop.App 8.");
            }
            catch
            {
                return false;
            }
        }
        
        static void ShowDotNetInstallDialog()
        {
            var result = MessageBox.Show(
                "OnliDesk requer o .NET 8 Desktop Runtime para funcionar.\n\n" +
                "Deseja abrir a página de download oficial?\n\n" +
                "• Tamanho: ~55MB\n" +
                "• Tempo: 2-3 minutos\n" +
                "• Necessário apenas uma vez",
                "OnliDesk - .NET Runtime Necessário",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://dotnet.microsoft.com/download/dotnet/8.0",
                        UseShellExecute = true
                    });
                    
                    MessageBox.Show(
                        "Instruções de instalação:\n\n" +
                        "1. Na página que abriu, procure por '.NET Desktop Runtime 8.0'\n" +
                        "2. Clique em 'Download x64' (para Windows 64-bit)\n" +
                        "3. Execute o arquivo baixado\n" +
                        "4. Siga as instruções do instalador\n" +
                        "5. Após a instalação, execute este OnliDesk novamente",
                        "OnliDesk - Instruções de Instalação",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                catch
                {
                    MessageBox.Show(
                        "Não foi possível abrir o navegador automaticamente.\n\n" +
                        "Acesse manualmente: https://dotnet.microsoft.com/download/dotnet/8.0\n" +
                        "Baixe '.NET Desktop Runtime 8.0' para Windows x64",
                        "OnliDesk - Download Manual",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
        }
        
        static bool LaunchOnliDesk()
        {
            string currentDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // Mostrar mensagem de debug
            MessageBox.Show($"Tentando executar OnliDesk...\nDiretório: {currentDir}", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Opção 1: Tentar executável direto (mais simples)
            string exePath = Path.Combine(currentDir, "OliAcessoRemoto.exe");
            if (File.Exists(exePath))
            {
                try
                {
                    MessageBox.Show($"Encontrado: {exePath}\nTentando executar...", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = exePath,
                        WorkingDirectory = currentDir,
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Normal
                    });

                    if (process != null)
                    {
                        MessageBox.Show("OnliDesk executado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao executar {exePath}:\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show($"Arquivo não encontrado: {exePath}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Opção 2: Tentar via dotnet run
            string csprojPath = Path.Combine(currentDir, "OliAcessoRemoto.csproj");
            if (File.Exists(csprojPath))
            {
                try
                {
                    MessageBox.Show($"Tentando via dotnet run: {csprojPath}", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "dotnet",
                            Arguments = $"run --project \"{csprojPath}\"",
                            WorkingDirectory = currentDir,
                            UseShellExecute = false,
                            CreateNoWindow = false,
                            WindowStyle = ProcessWindowStyle.Normal
                        }
                    };

                    process.Start();

                    // Aguardar um pouco para ver se o processo inicia
                    Thread.Sleep(3000);

                    if (!process.HasExited)
                    {
                        MessageBox.Show("OnliDesk iniciado via dotnet run!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show($"Processo dotnet terminou com código: {process.ExitCode}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao executar via dotnet run:\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            MessageBox.Show("Todas as opções falharam!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }
}
