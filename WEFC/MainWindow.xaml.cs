using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
    public MainWindow()
    {
        InitializeComponent();

        FilePath.Text = "Path: " + FileManager.Instance.filePath;
        FileName.Text = "Name: " + FileManager.Instance.fileName;
        NameInput.Text = Path.GetFileNameWithoutExtension(FileManager.Instance.fileName);
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
            args.Add("-filter_complex \"[0:1][0:2]amerge=inputs=2[a]\" -ac 2 -map 0:v -map \"[a]\"");
        }
        
        args.Add($"-c:v {CodecInput.Text}");
        
        args.Add($"-vf \"fps=fps={FpsInput.Text}\"");
        ca.Args = args.ToArray();
        ca.Name = NameInput.Text;
        ca.OutputFormat = FormatInput.Text;
        ca.Hwaccel = HwaccelComboBox.SelectedValue.ToString();
        ca.UseSize = _isSizeActive;
        ca.Size = float.Parse(SizeInput.Text);
        if (FileManager.Instance.ConvertFile(ca))
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
        }
        DurationCheckBox.IsChecked = true;
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

    // private void CompressionSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    // {
    //     CompressionText.Text = "Compression level: " + e.NewValue;
    // }
}