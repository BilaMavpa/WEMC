using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace WEMC;

public partial class ConversionWindow : Window
{
    public ConversionWindow()
    {
        InitializeComponent();
    }

    public Action updateProgress(string message)
    {
        ProgressText.Text = message;
        return Close;
    }

    public Dispatcher getCurrentDispatcher()
    {
        return Dispatcher.CurrentDispatcher;
    }
    private void StopRender(object sender, RoutedEventArgs e)
    {
        ConversionManager.Instance.CancellationTokenSource.Cancel();
        Close();
    }
}