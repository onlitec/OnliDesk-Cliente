using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace OliAcessoRemoto.Services;

/// <summary>
/// Serviço para comunicação com a API do servidor OnliDesk
/// </summary>
public class ServerApiService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private string? _authToken;

    public ServerApiService(string serverIp = "172.20.120.40", int port = 7070)
    {
        _baseUrl = $"http://{serverIp}:{port}";
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        // Configurar headers padrão
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "OliAcesso-Cliente/1.0.0");
    }

    /// <summary>
    /// Verificar se o servidor está online
    /// </summary>
    public async Task<bool> CheckServerHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Obter informações do servidor
    /// </summary>
    public async Task<ServerInfo?> GetServerInfoAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/info");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ServerInfo>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao obter info do servidor: {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// Registrar cliente no servidor
    /// </summary>
    public async Task<RegistrationResult> RegisterClientAsync(string clientName)
    {
        try
        {
            var systemInfo = new SystemInfo
            {
                OperatingSystem = Environment.OSVersion.Platform.ToString(),
                OSVersion = Environment.OSVersion.Version.ToString(),
                Architecture = Environment.Is64BitOperatingSystem ? "x64" : "x86",
                ComputerName = Environment.MachineName,
                UserName = Environment.UserName,
                ScreenResolution = GetScreenResolution(),
                MonitorCount = GetMonitorCount(),
                ClientVersion = "1.0.0"
            };

            var request = new LoginRequest
            {
                ClientId = GenerateClientId(),
                SystemInfo = systemInfo
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loginResponse != null)
                {
                    _authToken = loginResponse.Token;
                    SetAuthToken(_authToken);
                    
                    return new RegistrationResult
                    {
                        Success = true,
                        ClientId = loginResponse.ClientId,
                        Token = loginResponse.Token,
                        Message = "Registrado com sucesso"
                    };
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new RegistrationResult
                {
                    Success = false,
                    Message = $"Erro HTTP {response.StatusCode}: {errorContent}"
                };
            }
        }
        catch (Exception ex)
        {
            return new RegistrationResult
            {
                Success = false,
                Message = $"Erro de conexão: {ex.Message}"
            };
        }

        return new RegistrationResult
        {
            Success = false,
            Message = "Erro desconhecido"
        };
    }

    /// <summary>
    /// Configurar token de autenticação
    /// </summary>
    public void SetAuthToken(string token)
    {
        _authToken = token;
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Obter status de um cliente
    /// </summary>
    public async Task<ClientStatus?> GetClientStatusAsync(string clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/connections/clients/{clientId}/status");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ClientStatus>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao obter status do cliente: {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// Solicitar conexão com outro cliente
    /// </summary>
    public async Task<ConnectionResult> RequestConnectionAsync(string targetClientId, string sourceClientId)
    {
        try
        {
            var request = new
            {
                TargetClientId = targetClientId,
                SourceClientId = sourceClientId
            };

            var response = await _httpClient.PostAsJsonAsync("/api/connections/request", request);
            
            if (response.IsSuccessStatusCode)
            {
                return new ConnectionResult
                {
                    Success = true,
                    Message = "Solicitação de conexão enviada"
                };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new ConnectionResult
                {
                    Success = false,
                    Message = $"Erro: {errorContent}"
                };
            }
        }
        catch (Exception ex)
        {
            return new ConnectionResult
            {
                Success = false,
                Message = $"Erro de conexão: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Gerar ID único do cliente
    /// </summary>
    private string GenerateClientId()
    {
        var random = new Random();
        return $"{random.Next(100, 999)} {random.Next(100, 999)} {random.Next(100, 999)}";
    }

    /// <summary>
    /// Obter resolução da tela
    /// </summary>
    private string GetScreenResolution()
    {
        try
        {
            return "1920x1080"; // Placeholder - implementar com WPF APIs
        }
        catch
        {
            return "1920x1080";
        }
    }

    /// <summary>
    /// Obter número de monitores
    /// </summary>
    private int GetMonitorCount()
    {
        try
        {
            return 1; // Placeholder - implementar com WPF APIs
        }
        catch
        {
            return 1;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

// Modelos de dados
public class ServerInfo
{
    public string ServerName { get; set; } = "";
    public string Version { get; set; } = "";
    public string Framework { get; set; } = "";
    public string[] Features { get; set; } = Array.Empty<string>();
    public string[] Endpoints { get; set; } = Array.Empty<string>();
}

public class SystemInfo
{
    public string OperatingSystem { get; set; } = "";
    public string OSVersion { get; set; } = "";
    public string Architecture { get; set; } = "";
    public string ComputerName { get; set; } = "";
    public string UserName { get; set; } = "";
    public string ScreenResolution { get; set; } = "";
    public int MonitorCount { get; set; } = 1;
    public string ClientVersion { get; set; } = "";
}

public class LoginRequest
{
    public string ClientId { get; set; } = "";
    public SystemInfo SystemInfo { get; set; } = new();
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public string ClientId { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
}

public class RegistrationResult
{
    public bool Success { get; set; }
    public string ClientId { get; set; } = "";
    public string Token { get; set; } = "";
    public string Message { get; set; } = "";
}

public class ClientStatus
{
    public string ClientId { get; set; } = "";
    public bool IsOnline { get; set; }
    public DateTime? LastSeen { get; set; }
}

public class ConnectionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
}
