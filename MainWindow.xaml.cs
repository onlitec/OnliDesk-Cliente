using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OliAcessoRemoto.Services;

namespace OliAcessoRemoto;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private bool _isServerRunning = false;
    private bool _isConnected = false;
    private string _localId = "000 000 000";
    private string? _authToken;
    private readonly ServerApiService? _serverApi;


    public ObservableCollection<RecentConnection> RecentConnections { get; set; } = new();

    public MainWindow()
    {
        try
        {
            // Inicializar servi�o de API com IP do servidor OnliDesk
            _serverApi = new ServerApiService("172.20.120.40", 7070);

            InitializeComponent();
            InitializeData();
            UpdateUI();

            // Inicializar conexão com servidor
            _ = InitializeServerConnectionAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Erro ao inicializar a aplicação: {ex.Message}\n\nDetalhes: {ex}",
                          "Erro de Inicialização", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void InitializeData()
    {
        // Inicializar conex�es recentes (dados de exemplo)
        RecentConnections = new ObservableCollection<RecentConnection>
        {
            new RecentConnection { Name = "Computador do Escrit�rio", Id = "987 654 321" },
            new RecentConnection { Name = "Laptop Casa", Id = "456 789 123" },
            new RecentConnection { Name = "Servidor Principal", Id = "111 222 333" }
        };

        RecentConnectionsListBox.ItemsSource = RecentConnections;

        // Gerar ID local (normalmente seria obtido do servidor)
        GenerateLocalId();
    }

    private async Task InitializeServerConnectionAsync()
    {
        try
        {
            // Verificar se o servidor está online
            FooterStatusText.Text = "Verificando conexão com servidor OnliDesk...";

            bool serverOnline = await _serverApi.CheckServerHealthAsync();
            if (!serverOnline)
            {
                FooterStatusText.Text = "Servidor OnliDesk offline - Modo offline";
                System.Windows.MessageBox.Show("Não foi possível conectar ao servidor OnliDesk (172.20.120.40:7070).\nVerifique se o servidor está rodando e tente novamente.\n\nO aplicativo funcionará em modo offline.",
                              "Servidor Offline", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Obter informações do servidor
            var serverInfo = await _serverApi.GetServerInfoAsync();
            if (serverInfo != null)
            {
                FooterStatusText.Text = $"Conectado ao {serverInfo.ServerName} v{serverInfo.Version}";
            }

            // Registrar cliente no servidor
            FooterStatusText.Text = "Registrando cliente no servidor OnliDesk...";
            var computerName = Environment.MachineName;
            var userName = Environment.UserName;
            var clientName = $"{computerName} ({userName})";

            var registrationResult = await _serverApi.RegisterClientAsync(clientName);

            if (registrationResult.Success)
            {
                _localId = registrationResult.ClientId;
                _authToken = registrationResult.Token;

                LocalIdTextBlock.Text = _localId;
                FooterStatusText.Text = $"Conectado ao OnliDesk - ID: {_localId}";
                _isConnected = true;

                // TODO: Atualizar indicador de status visual
            }
            else
            {
                FooterStatusText.Text = $"Erro no registro: {registrationResult.Message}";
                System.Windows.MessageBox.Show($"Erro ao registrar no servidor OnliDesk:\n{registrationResult.Message}",
                              "Erro de Registro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            FooterStatusText.Text = "Erro de conexão com servidor OnliDesk";
            System.Windows.MessageBox.Show($"Erro ao conectar com servidor OnliDesk (172.20.120.40:7070):\n{ex.Message}\n\nVerifique sua conexão de rede e se o servidor está rodando.",
                          "Erro de Conexão", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        UpdateUI();
    }

    private void GenerateLocalId()
    {
        // Este m�todo agora � chamado apenas como fallback
        Random random = new Random();
        _localId = $"{random.Next(100, 999)} {random.Next(100, 999)} {random.Next(100, 999)}";
        LocalIdTextBlock.Text = _localId;
    }

    private void UpdateUI()
    {
        // Atualizar status da conexão
        if (_isConnected)
        {
            StatusIndicator.Fill = new SolidColorBrush(Colors.Green);
            StatusText.Text = "Conectado";
        }
        else
        {
            StatusIndicator.Fill = new SolidColorBrush(Colors.Red);
            StatusText.Text = "Desconectado";
        }

        // Atualizar status do servidor
        if (_isServerRunning)
        {
            ServerStatusIndicator.Fill = new SolidColorBrush(Colors.Green);
            ServerStatusText.Text = "Servidor ativo - Aguardando conex�es";
            StartServerButton.Content = "Parar Servidor";
        }
        else
        {
            ServerStatusIndicator.Fill = new SolidColorBrush(Colors.Red);
            ServerStatusText.Text = "Servidor parado";
            StartServerButton.Content = "Iniciar Servidor";
        }
    }

    // Event Handlers
    private void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        string remoteId = RemoteIdTextBox.Text.Trim();

        if (string.IsNullOrEmpty(remoteId))
        {
            System.Windows.MessageBox.Show("Por favor, digite o ID do computador remoto.", "ID Necessário",
                          MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Validar formato do ID (XXX XXX XXX)
        if (!IsValidId(remoteId))
        {
            System.Windows.MessageBox.Show("Formato de ID inválido. Use o formato: XXX XXX XXX", "ID Inválido",
                          MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Simular tentativa de conexão
        FooterStatusText.Text = $"Conectando ao ID {remoteId}...";
        ConnectButton.IsEnabled = false;
        ConnectButton.Content = "Conectando...";

        // Implementar lógica real de conexão
        _ = Task.Run(async () =>
        {
            try
            {
                // Verificar se o cliente de destino est� online
                // Verificar se o cliente de destino está online no servidor OnliDesk
                var targetStatus = await _serverApi.GetClientStatusAsync(remoteId);
                if (targetStatus == null || !targetStatus.IsOnline)
                {
                    Dispatcher.Invoke(() =>
                    {
                        FooterStatusText.Text = "Cliente de destino offline";
                        System.Windows.MessageBox.Show($"O cliente {remoteId} não está online ou não foi encontrado no servidor OnliDesk.",
                                      "Cliente Offline", MessageBoxButton.OK, MessageBoxImage.Warning);
                        ConnectButton.IsEnabled = true;
                        ConnectButton.Content = "Conectar";
                    });
                    return;
                }

                // Solicitar conex�o atrav�s da API
                // Solicitar conexão através da API OnliDesk
                var connectionResult = await _serverApi.RequestConnectionAsync(remoteId, _localId);

                Dispatcher.Invoke(() =>
                {
                    if (connectionResult.Success)
                    {
                        FooterStatusText.Text = $"Solicitação enviada para {remoteId}";
                        System.Windows.MessageBox.Show($"Solicitação de conexão enviada para {remoteId}.\n" +
                                      "Aguardando aprovação do usuário remoto.",
                                      "Solicitação Enviada", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Salvar conex�o se solicitado
                        if (SaveConnectionCheckBox.IsChecked == true)
                        {
                            SaveRecentConnection(remoteId);
                        }
                    }
                    else
                    {
                        FooterStatusText.Text = "Falha na solicitação de conexão";
                        System.Windows.MessageBox.Show($"Erro ao solicitar conexão:\n{connectionResult.Message}",
                                      "Erro de Conexão", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    ConnectButton.IsEnabled = true;
                    ConnectButton.Content = "Conectar";
                    UpdateUI();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    FooterStatusText.Text = "Erro na conexão";
                    System.Windows.MessageBox.Show($"Erro ao conectar:\n{ex.Message}",
                                  "Erro de Conexão", MessageBoxButton.OK, MessageBoxImage.Error);
                    ConnectButton.IsEnabled = true;
                    ConnectButton.Content = "Conectar";
                });
            }
        });
    }

    private void CopyIdButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Windows.Clipboard.SetText(_localId);
            FooterStatusText.Text = "ID copiado para a �rea de transfer�ncia";

            // Feedback visual tempor�rio
            CopyIdButton.Content = "? Copiado!";
            Task.Delay(2000).ContinueWith(t =>
            {
                Dispatcher.Invoke(() =>
                {
                    CopyIdButton.Content = "?? Copiar";
                });
            });
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Erro ao copiar ID: {ex.Message}", "Erro",
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void StartServerButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isServerRunning)
        {
            // Parar servidor
            _isServerRunning = false;
            FooterStatusText.Text = "Servidor parado";
        }
        else
        {
            // Iniciar servidor
            _isServerRunning = true;
            FooterStatusText.Text = "Servidor iniciado - Aguardando conex�es";
        }

        UpdateUI();
    }

    private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implementar salvamento de configurações
        FooterStatusText.Text = "Configurações salvas";
        System.Windows.MessageBox.Show("Configurações salvas com sucesso!", "Configurações",
                      MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void RestoreDefaultsButton_Click(object sender, RoutedEventArgs e)
    {
        var result = System.Windows.MessageBox.Show("Deseja restaurar todas as configurações para os valores padrão?",
                                   "Restaurar Padrões", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // Restaurar configurações padrão
            StartWithWindowsCheckBox.IsChecked = false;
            MinimizeToTrayCheckBox.IsChecked = true;
            ShowNotificationsCheckBox.IsChecked = true;
            ServerAddressTextBox.Text = "servidor.oliacesso.com";
            ServerPortTextBox.Text = "7070";
            QualityComboBox.SelectedIndex = 1;
            AdaptiveQualityCheckBox.IsChecked = true;
            RequirePasswordCheckBox.IsChecked = false;
            AccessPasswordTextBox.Text = "";
            AutoAcceptCheckBox.IsChecked = false;

            FooterStatusText.Text = "Configurações restauradas para os padrões";
        }
    }

    private void AboutButton_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.MessageBox.Show("OliAcesso Remoto - Cliente\nVersão 1.0.0\n\nUm aplicativo de acesso remoto seguro e fácil de usar.\n\n© 2024 OliAcesso",
                      "Sobre", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // Helper Methods
    private bool IsValidId(string id)
    {
        // Validar formato XXX XXX XXX (n�meros com espa�os)
        var parts = id.Split(' ');
        if (parts.Length != 3) return false;

        foreach (var part in parts)
        {
            if (part.Length != 3 || !part.All(char.IsDigit))
                return false;
        }

        return true;
    }

    private void SaveRecentConnection(string remoteId)
    {
        // Verificar se j� existe
        var existing = RecentConnections.FirstOrDefault(c => c.Id == remoteId);
        if (existing != null)
        {
            RecentConnections.Remove(existing);
        }

        // Adicionar no topo
        RecentConnections.Insert(0, new RecentConnection
        {
            Name = $"Computador {remoteId}",
            Id = remoteId
        });

        // Manter apenas os 10 mais recentes
        while (RecentConnections.Count > 10)
        {
            RecentConnections.RemoveAt(RecentConnections.Count - 1);
        }
    }

    private void OpenRemoteControlWindow(string remoteId)
    {
        // TODO: Implementar janela de controle remoto
        System.Windows.MessageBox.Show($"Abrindo sess�o de controle remoto para {remoteId}\n\n" +
                       "Esta funcionalidade ser� implementada na pr�xima fase.",
                       "Controle Remoto", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected override void OnClosed(EventArgs e)
    {
        // Limpar recursos do servidor API
        _serverApi?.Dispose();
        base.OnClosed(e);
    }
}

// Model classes
public class RecentConnection
{
    public string Name { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
}
