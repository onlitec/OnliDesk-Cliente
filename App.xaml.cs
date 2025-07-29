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
        // Capturar exceções não tratadas
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        base.OnStartup(e);
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        System.Windows.MessageBox.Show($"Erro não tratado: {e.Exception.Message}\n\nDetalhes: {e.Exception}",
                       "Erro da Aplicação", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        System.Windows.MessageBox.Show($"Erro crítico: {ex?.Message}\n\nDetalhes: {ex}",
                       "Erro Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
