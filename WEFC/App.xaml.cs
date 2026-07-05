using System.Configuration;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;

namespace WEFC;

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
                FileManager.Instance.SetFile(e.Args[i + 1]);
            }

            if (e.Args[i] == "-p")
            {
                foreach (Preset preset in presets)
                {
                    if (preset.Name == e.Args[i + 1])
                    {
                        if (FileManager.Instance.ConvertFile(preset))
                        {
                            Current.Shutdown();
                        }
                    }
                }
            }
        }

        MainWindow mainWindow = new MainWindow();

        mainWindow.Show();
        
    }

    
    
    
}