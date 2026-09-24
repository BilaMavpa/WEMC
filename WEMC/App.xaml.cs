using System.Configuration;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Threading;

namespace WEMC;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private void AppStartup(object sender, StartupEventArgs e)
    {
        string jsonText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets.json"));
        
        List<Preset> presets = JsonSerializer.Deserialize<List<Preset>>(jsonText);

        // Process command line args
        for (int i = 0; i != e.Args.Length; ++i)
        {
            if (e.Args[i] == "-dir")
            {
                ConversionManager.Instance.SetFile(e.Args[i + 1]);
            }

            if (e.Args[i] == "-p")
            {
                foreach (Preset preset in presets)
                {
                    if (preset.Name == e.Args[i + 1])
                    {
                        if (ConversionManager.Instance.ConvertFile(preset))
                        {
                            Current.Shutdown();
                        }
                    }
                }
            }
        }
        
        if (e.Args.Length > 0)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
        else
        {
            DefaultWindow defaultWindow = new DefaultWindow();
            defaultWindow.Show();
        }
    }

    
    
    
}