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

namespace WEFC;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        FilePath.Text = "Path: " + FileInfo.Instance.filePath;
        FileName.Text = "Name: " + FileInfo.Instance.fileName;
    }

    private void ConvertEvent(object sender, RoutedEventArgs e)
    {
        if (FileInfo.Instance.ConvertFile(FormatInput.Text,[]))
        {
            Application.Current.Shutdown();
        }
        
    }
}