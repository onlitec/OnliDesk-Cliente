using System.Configuration;
using System.Data;
using System.Windows;

namespace OliAcessoRemoto;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Capturar exce��es n�o tratadas
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        base.OnStartup(e);
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        System.Windows.MessageBox.Show($"Erro n�o tratado: {e.Exception.Message}\n\nDetalhes: {e.Exception}",
                       "Erro da Aplica��o", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        System.Windows.MessageBox.Show($"Erro cr�tico: {ex?.Message}\n\nDetalhes: {ex}",
                       "Erro Cr�tico", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
