using System.Net.Http.Json;
using System.Text.Json;

namespace OliAcessoRemoto.Client;

class Program
{
    private static readonly HttpClient httpClient = new HttpClient();

    static async Task Main(string[] args)
    {
        Console.WriteLine("🖥️  OliAcesso Remoto - Cliente Console");
        Console.WriteLine("=====================================");
        Console.WriteLine();

        // Configurar servidor
        string serverUrl = "http://localhost:7070"; // Altere conforme necessário

        if (args.Length > 0)
        {
            serverUrl = args[0];
        }

        Console.WriteLine($"Servidor: {serverUrl}");
        Console.WriteLine();

        try
        {
            // Testar conectividade
            await TestServerConnection(serverUrl);

            // Menu principal
            await ShowMainMenu(serverUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro: {ex.Message}");
        }
        finally
        {
            httpClient.Dispose();
        }
    }

    static async Task TestServerConnection(string serverUrl)
    {
        Console.WriteLine("🔍 Testando conectividade com o servidor...");

        try
        {
            var response = await httpClient.GetAsync($"{serverUrl}/health");

            if (response.IsSuccessStatusCode)
            {
                var healthData = await response.Content.ReadAsStringAsync();
                Console.WriteLine("✅ Servidor online!");
                Console.WriteLine($"   Status: {healthData}");
            }
            else
            {
                Console.WriteLine($"⚠️  Servidor respondeu com status: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ Erro de conexão: {ex.Message}");
            Console.WriteLine("   Verifique se o servidor está rodando.");
        }

        Console.WriteLine();
    }

    static async Task ShowMainMenu(string serverUrl)
    {
        while (true)
        {
            Console.WriteLine("📋 Menu Principal:");
            Console.WriteLine("1. Testar autenticação");
            Console.WriteLine("2. Obter informações do servidor");
            Console.WriteLine("3. Listar clientes online");
            Console.WriteLine("4. Testar health check");
            Console.WriteLine("5. Sair");
            Console.WriteLine();
            Console.Write("Escolha uma opção: ");

            var choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await TestAuthentication(serverUrl);
                    break;
                case "2":
                    await GetServerInfo(serverUrl);
                    break;
                case "3":
                    await ListOnlineClients(serverUrl);
                    break;
                case "4":
                    await TestServerConnection(serverUrl);
                    break;
                case "5":
                    Console.WriteLine("👋 Até logo!");
                    return;
                default:
                    Console.WriteLine("❌ Opção inválida!");
                    break;
            }

            Console.WriteLine();
            Console.WriteLine("Pressione qualquer tecla para continuar...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    static async Task TestAuthentication(string serverUrl)
    {
        Console.WriteLine("🔐 Testando autenticação...");

        try
        {
            var loginData = new
            {
                clientId = "console-client-" + Environment.MachineName,
                systemInfo = new
                {
                    operatingSystem = Environment.OSVersion.Platform.ToString(),
                    osVersion = Environment.OSVersion.Version.ToString(),
                    architecture = Environment.Is64BitOperatingSystem ? "x64" : "x86",
                    computerName = Environment.MachineName,
                    userName = Environment.UserName,
                    screenResolution = "1920x1080",
                    monitorCount = 1,
                    clientVersion = "1.0.0-console"
                }
            };

            var response = await httpClient.PostAsJsonAsync($"{serverUrl}/api/auth/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine("✅ Autenticação bem-sucedida!");
                Console.WriteLine($"   Resposta: {result}");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Falha na autenticação: {response.StatusCode}");
                Console.WriteLine($"   Erro: {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro na autenticação: {ex.Message}");
        }
    }

    static async Task GetServerInfo(string serverUrl)
    {
        Console.WriteLine("ℹ️  Obtendo informações do servidor...");

        try
        {
            var response = await httpClient.GetAsync($"{serverUrl}/info");

            if (response.IsSuccessStatusCode)
            {
                var info = await response.Content.ReadAsStringAsync();
                Console.WriteLine("✅ Informações do servidor:");

                // Tentar formatar JSON
                try
                {
                    var jsonDoc = JsonDocument.Parse(info);
                    var formatted = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
                    Console.WriteLine(formatted);
                }
                catch
                {
                    Console.WriteLine(info);
                }
            }
            else
            {
                Console.WriteLine($"❌ Erro ao obter informações: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro: {ex.Message}");
        }
    }

    static async Task ListOnlineClients(string serverUrl)
    {
        Console.WriteLine("👥 Listando clientes online...");

        try
        {
            var response = await httpClient.GetAsync($"{serverUrl}/api/connections/clients/online");

            if (response.IsSuccessStatusCode)
            {
                var clients = await response.Content.ReadAsStringAsync();
                Console.WriteLine("✅ Clientes online:");

                // Tentar formatar JSON
                try
                {
                    var jsonDoc = JsonDocument.Parse(clients);
                    var formatted = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
                    Console.WriteLine(formatted);
                }
                catch
                {
                    Console.WriteLine(clients);
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("❌ Não autorizado. É necessário fazer login primeiro.");
            }
            else
            {
                Console.WriteLine($"❌ Erro ao listar clientes: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro: {ex.Message}");
        }
    }
}
