using System.Configuration;
using System.Data;
using System.Windows;

namespace WEFC;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    void AppStartup(object sender, StartupEventArgs e)
    {
        // Application is running
        // Process command line args
        bool startMinimized = false;
        for (int i = 0; i != e.Args.Length; ++i)
        {
            if (e.Args[i] == "-dir")
            {
                FileInfo.Instance.SetFile(e.Args[i + 1]);
            }
        }

        // Create main application window, starting minimized if specified
        MainWindow mainWindow = new MainWindow();
        if (startMinimized)
        {
            mainWindow.WindowState = WindowState.Minimized;
        }
        mainWindow.Show();
        
    }

    
    
    
}