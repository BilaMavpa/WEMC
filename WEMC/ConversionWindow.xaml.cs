using System.Windows;

namespace WEMC;

public partial class ConversionWindow : Window
{
    public bool ConvertingPreset = false;
    public ConversionWindow()
    {
        InitializeComponent();
    }

    public void UpdateProgress(string message,string duration)
    {
        double progress = TimeSpan.Parse(
                message.Substring(message.IndexOf("time=", StringComparison.Ordinal) + 5, 8)).TotalSeconds
            / TimeSpan.Parse(duration).TotalSeconds * 100;
        ProgressBar.Value = progress;

        ProgressText.Text = message;
    }
    
    private void StopRender(object sender, RoutedEventArgs e)
    {
        ConversionManager.Instance.CancellationTokenSource.Cancel();
        if (!ConvertingPreset)
        {
           Close();
        }
    }
    
}