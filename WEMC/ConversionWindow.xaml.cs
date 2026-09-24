using System.Windows;

namespace WEMC;

public partial class ConversionWindow : Window
{
    public bool convertingPreset = false;
    public ConversionWindow()
    {
        InitializeComponent();
    }

    public void updateProgress(string message,string duration)
    {
        double progress = TimeSpan.Parse(
                message.Substring(message.IndexOf("time=") + 5, 8)).TotalSeconds
            / TimeSpan.Parse(duration).TotalSeconds * 100;
        ProgressBar.Value = progress;

        ProgressText.Text = message;
    }
    
    private void StopRender(object sender, RoutedEventArgs e)
    {
        ConversionManager.Instance.CancellationTokenSource.Cancel();
        if (!convertingPreset)
        {
           Close();
        }
    }
    
}