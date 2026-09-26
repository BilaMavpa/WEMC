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
        bool presetDetected = false;
        // Process command line args
        for (int i = 0; i != e.Args.Length; ++i)
        {
            if (e.Args[i] == "-dir")
            {
                ConversionManager.Instance.SetFile(e.Args[i + 1]);
            }

            if (e.Args[i] == "-p")
            {
                Console.WriteLine("Preset Detected");
                ConvertPreset(e,i);
                presetDetected = true;
                
            }
        }

        if (presetDetected) return;
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

    private async void ConvertPreset(StartupEventArgs e, int i)
    {
        string jsonText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets.json"));
        List<Preset> presets = JsonSerializer.Deserialize<List<Preset>>(jsonText);
        
        foreach (Preset preset in presets)
        {
            if (preset.Name == e.Args[i + 1])
            {
                
                bool showProgress = preset.ShowProgress;
                ConversionWindow conversionWindow = new ConversionWindow();
                if (showProgress)
                {
                    conversionWindow.Show();
                    conversionWindow.ConvertingPreset = true;
                }
                IProgress<String> progress = new Progress<String>(message =>
                {
                    conversionWindow.UpdateProgress(message,ConversionManager.Instance.GetDuration());
                });
                
                bool completionStatus = false;

                try
                {
                    await Task.Run(() =>
                        completionStatus = ConversionManager.Instance.ConvertFile(
                            ConversionManager.Instance.CreateArgs(preset), progress,
                            ConversionManager.Instance.CancellationTokenSource.Token,showProgress));
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
                    Application.Current.Shutdown();
                }
            }
        }
    }
    
    
}