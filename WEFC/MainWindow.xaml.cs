using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Path = System.IO.Path;

namespace WEFC;

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

    private void ConvertEvent(object sender, RoutedEventArgs e)
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
        if ((bool)MergeAudioCheckbox.IsChecked!)
        {
            switch (AudioComboBox.SelectedValue.ToString())
            {
                case "1 + 2":
                    args.Add("-filter_complex \"[0:1][0:2]amerge=inputs=2[a]\" -ac 2 -map 0:v -map \"[a]\""); 
                    break;
                case "All":
                    string tracks = "";
                    ProcessStartInfo ffmpeg = new ProcessStartInfo
                    {
                        CreateNoWindow = false,
                        UseShellExecute = false,
                        FileName = _ffprobePath,
                        Arguments = $"-v error -select_streams a -show_entries stream=index -of csv=p=0 \"{ConversionManager.Instance.FileName}\"",
                        RedirectStandardOutput = true,
                        WorkingDirectory = ConversionManager.Instance.FilePath
                    };
            
                    using (Process process = new Process())
                    {
                        process.StartInfo = ffmpeg;
                        process.Start();
                        tracks = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                    }
                    int amount = tracks.Count(char.IsDigit);
                    
                    if (amount == 0)
                    {
                        break;
                    }

                    string arg = "-filter_complex \"";
                    for (int i = 1; i < amount+1; i++)
                    {
                        arg += $"[0:{i}]";
                    }
                    
                    arg += $"amerge=inputs={amount}[a]\" -ac 2 -map 0:v -map \"[a]\"";
                    args.Add(arg);
                    break;
            }
        }
        
        args.Add($"-c:v {CodecInput.Text}");
        
        args.Add($"-vf \"fps=fps={FpsInput.Text}\"");
        ca.Args = args.ToArray();
        ca.Name = NameInput.Text;
        ca.OutputFormat = FormatInput.Text;
        ca.Hwaccel = HwaccelComboBox.SelectedValue.ToString();
        ca.UseSize = _isSizeActive;
        ca.Size = float.Parse(SizeInput.Text);
        if (ConversionManager.Instance.ConvertFile(ca))
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
            ProcessStartInfo ffmpeg = new ProcessStartInfo
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
                process.StartInfo = ffmpeg;
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