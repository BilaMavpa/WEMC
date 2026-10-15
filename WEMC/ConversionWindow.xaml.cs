using System.Diagnostics;
using System.Windows;

namespace WEMC;

public partial class ConversionWindow : Window
{
    public bool ConvertingPreset = false;
    private Stopwatch _stopwatch = new Stopwatch();
    private double _lastProgress = 0;
    private int _updateNum = 1;
    private double[] _lastSpeeds = new double[3];
    private int _lastSpeedsIndex = 0;
    public ConversionWindow()
    {
        InitializeComponent();
        _stopwatch.Start();
    }

    public void UpdateProgress(string message,string duration)
    {
        try
        {
            double progress = TimeSpan.Parse(
                    message.Substring(message.IndexOf("time=", StringComparison.Ordinal) + 5, 8)).TotalSeconds
                / TimeSpan.Parse(duration).TotalSeconds * 100;
            ProgressBar.Value = progress;

            if (_updateNum != 1)
            {
                double remainingDuration = TimeSpan.Parse(duration).TotalSeconds - TimeSpan.Parse(duration).TotalSeconds * (progress / 100);
                double speed = TimeSpan.Parse(duration).TotalSeconds * (progress - _lastProgress) / 100;
                
                if (_updateNum == 2)
                {
                    Array.Fill(_lastSpeeds, speed);
                }
                
                _lastSpeeds[_lastSpeedsIndex] = speed;
                
                if (_lastSpeedsIndex == _lastSpeeds.Length - 1)
                {
                    _lastSpeedsIndex = 0;
                }
                else
                {
                    _lastSpeedsIndex++;
                }
                
                double remainingTime = remainingDuration / _lastSpeeds.Max();
                EtaTimeText.Text = $"ETA: " + (remainingTime < 3600 ? 
                    TimeSpan.FromSeconds(remainingTime).ToString(@"mm\:ss") :
                    TimeSpan.FromSeconds(remainingTime).ToString(@"hh\:mm\:ss")) ;
            }
            _updateNum++;
            _lastProgress = progress;
            
            ElapsedTimeText.Text = (_stopwatch.Elapsed.TotalSeconds < 3600 ? 
                _stopwatch.Elapsed.ToString(@"mm\:ss") : 
                _stopwatch.Elapsed.ToString(@"hh\:mm\:ss"));

            ProgressText.Text = message;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
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