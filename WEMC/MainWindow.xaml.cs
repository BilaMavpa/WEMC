using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Path = System.IO.Path;

namespace WEMC;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private bool _isStartTimeActive = false;
    private bool _isDurationActive = false;
    private bool _isSizeActive = false;
    
    private readonly string _ffprobePath = Environment.GetEnvironmentVariable("PATH")
        ?.Split(Path.PathSeparator)
        .Select(dir => Path.Combine(dir, "ffprobe.exe"))
        .FirstOrDefault(File.Exists);
    public MainWindow()
    {
        InitializeComponent();

        FilePath.Text = "Path: " + ConversionManager.Instance.FilePath;
        FileName.Text = "Name: " + ConversionManager.Instance.FileName;
        NameInput.Text = Path.GetFileNameWithoutExtension(ConversionManager.Instance.FileName);
    }

    private async void ConvertEvent(object sender, RoutedEventArgs e)
    {
        ConversionArgs ca = new ConversionArgs();
        List<string> args = new List<string>();
        ca.StartTime = "";
        ca.Duration = "";
        if (_isStartTimeActive)
        {
            ca.StartTime = StartInput.Text;
        }
        if (_isDurationActive)
        {
            ca.Duration = DurationInput.Text;
        }

        
        switch (AudioComboBox.SelectedValue.ToString())
        {
            case "Merge 1 + 2":
                args.Add("-filter_complex \"[0:1][0:2]amerge=inputs=2[a]\" -ac 2 -map 0:v -map \"[a]\""); 
                break;
            case "Merge all":
                args.Add(ConversionManager.Instance.GetArgsForMergingAllAudio());
                break;
            case "Keep same":
                args.Add(ConversionManager.Instance.GetArgsForKeepingAllAudio());
                break;
            case "None":
                args.Add("-an");
                break;
        }

        
        args.Add($"-c:v {CodecInput.Text}");
        
        args.Add($"-vf \"fps=fps={FpsInput.Text}\"");
        ca.Args = args.ToArray();
        ca.Name = NameInput.Text;
        ca.OutputFormat = FormatInput.Text;
        ca.Hwaccel = HwaccelComboBox.SelectedValue.ToString();
        ca.UseSize = _isSizeActive;
        ca.Size = float.Parse(SizeInput.Text);
        ConversionWindow conversionWindow = new ConversionWindow();
        conversionWindow.Show();
        IProgress<String> progress = new Progress<String>(message =>
        {
            conversionWindow.updateProgress(message);
        });

        bool completionStatus = false;

        try
        {
            await Task.Run(() =>
                completionStatus = ConversionManager.Instance.ConvertFile(ca, progress,
                    ConversionManager.Instance.CancellationTokenSource.Token));
        }
        catch (OperationCanceledException exception)
        {
            Console.WriteLine("cancelled");
        }
        catch (Exception exception)
        {
            conversionWindow.Close();
            Alert alert = new Alert("Conversion Error", exception.Message);
            alert.Show();
        }
        finally
        {
            ConversionManager.Instance.CancellationTokenSource = new CancellationTokenSource();
        }
        
        if (completionStatus)
        {
            Application.Current.Shutdown();
        }
        
    }

    private void StartTimeToggle(object sender, RoutedEventArgs e)
    {
        if (_isStartTimeActive)
        {
            StartInput.IsReadOnly = true;
            StartInput.Background = new SolidColorBrush(Colors.LightGray);
        }
        else
        {
            StartInput.IsReadOnly = false;
            StartInput.Background = new SolidColorBrush(Colors.White);
        }
        _isStartTimeActive = !_isStartTimeActive;
    }
    
    private void DurationToggle(object sender, RoutedEventArgs e)
    {
        if (_isDurationActive)
        {
            DurationInput.IsReadOnly = true;
            DurationInput.Background = new SolidColorBrush(Colors.LightGray);
        }
        else
        {
            DurationInput.IsReadOnly = false;
            DurationInput.Background = new SolidColorBrush(Colors.White);
        }
        _isDurationActive = !_isDurationActive;
    }
    
    private void SizeToggle(object sender, RoutedEventArgs e)
    {
        if (_isSizeActive)
        {
            SizeInput.IsReadOnly = true;
            SizeInput.Background = new SolidColorBrush(Colors.LightGray);
        }
        else
        {
            SizeInput.IsReadOnly = false;
            SizeInput.Background = new SolidColorBrush(Colors.White);
            DurationCheckBox.IsChecked = true;
        }
        
        
        if (DurationInput.Text.Equals("00:00:00"))
        {
            string fileDuration;
            ProcessStartInfo ffprobe = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = _ffprobePath,
                Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 -sexagesimal \"{ConversionManager.Instance.FileName}\"",
                RedirectStandardOutput = true,
                WorkingDirectory = ConversionManager.Instance.FilePath
            };
            
            using (Process process = new Process())
            {
                process.StartInfo = ffprobe;
                process.Start();
                fileDuration = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
            DurationInput.Text =  "0" + fileDuration[..7];
        }
        _isSizeActive = !_isSizeActive;
    }
    
    private void PreviewNumberInput(object sender, TextCompositionEventArgs e)
    {
        Regex numberRegex = new Regex("[^0-9.]+");
        e.Handled = numberRegex.IsMatch(e.Text);
    }
    
    private void PreviewTimeInput(object sender, TextCompositionEventArgs e)
    {
        Regex timeRegex = new Regex("[^0-9:]+");
        e.Handled = timeRegex.IsMatch(e.Text);
    }

}