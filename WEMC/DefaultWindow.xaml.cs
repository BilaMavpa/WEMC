using System.IO;
using System.Security.Principal;
using System.Text.Json;
using System.Windows;
using Microsoft.Win32;

namespace WEMC;

public partial class DefaultWindow : Window
{
    public DefaultWindow()
    {
        InitializeComponent();
        if (IsRunningAsAdmin())
        {
            ApplyRegistryButton.IsEnabled = true;
            RemoveRegistryButton.IsEnabled = true;
            TextBlock.Text = "";
        }
        else
        {
            TextBlock.Text = "You have to launch the app as admin to make registry changes";
        }
    }
    
    private static bool IsRunningAsAdmin()
    {
        // Get the identity of the current user
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        // Create a principal object representing the current user
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        // Check if the user is in the Administrator role
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private void ApplyButtonAction(object sender, RoutedEventArgs routedEventArgs)
    {
        try
        {
            ApplyRegistry();
        }
        catch (Exception e)
        {
            MessageBox.Show("Failed to apply registry entries","Error",
                MessageBoxButton.OK,MessageBoxImage.None);
        }
    }
    private void RemoveButtonAction(object sender, RoutedEventArgs routedEventArgs)
    {
        RemoveRegistry();
    }

    private void RemoveRegistry()
    {
        RegistryKey rootKey = Registry.ClassesRoot.OpenSubKey(@"\*\shell", true);
        rootKey.DeleteSubKeyTree("WEMC");
        rootKey.Close();
        
        MessageBox.Show("Registry entries successfully removed","Success",MessageBoxButton.OK,MessageBoxImage.None);
    }
    private void ApplyRegistry()
    {
        RegistryKey rootKey = Registry.ClassesRoot.OpenSubKey(@"\*\shell", true);
        rootKey.CreateSubKey("WEMC");
        rootKey.Close();
        
        RegistryKey appKey = Registry.ClassesRoot.OpenSubKey(@"\*\shell\WEMC", true);
        appKey.SetValue("MUIVerb","Media Converter");
        appKey.SetValue("SubCommands","");
        appKey.CreateSubKey("shell");
        appKey.Close();
        
        RegistryKey shellKey = Registry.ClassesRoot.OpenSubKey(@"\*\shell\WEMC\shell", true);
        shellKey.CreateSubKey("mainAction");
        shellKey.Close();
        
        RegistryKey mainActionKey = Registry.ClassesRoot.OpenSubKey(@"\*\shell\WEMC\shell\mainAction", true);
        mainActionKey.SetValue("","Convert File");
        mainActionKey.CreateSubKey("command");
        mainActionKey.Close();
        
        RegistryKey mainActionCommandKey = Registry.ClassesRoot.OpenSubKey(@"\*\shell\WEMC\shell\mainAction\command", true);
        mainActionCommandKey.SetValue("", $"\"{System.Environment.ProcessPath}\" -dir \"%1\"");
        
        ApplyPresets();
        
        MessageBox.Show("Registry entries successfully created","Success",MessageBoxButton.OK,MessageBoxImage.None);
    }
    private void ApplyPresets()
    {
        string jsonText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets.json"));
        
        List<Preset> presets = JsonSerializer.Deserialize<List<Preset>>(jsonText);
        
        RegistryKey appKey = Registry.ClassesRoot.OpenSubKey(@"\*\shell\WEMC", true);
        appKey.DeleteSubKeyTree("shell");
        appKey.CreateSubKey("shell");
        appKey.Close();
        
        RegistryKey shellKey = Registry.ClassesRoot.OpenSubKey(@"\*\shell\WEMC\shell", true);
        shellKey.CreateSubKey("mainAction");
        foreach (Preset preset in presets)
        {
            shellKey.CreateSubKey(preset.Name);
            RegistryKey presetKey = Registry.ClassesRoot.OpenSubKey(@"\*\shell\WEMC\shell\" + preset.Name, true);
            presetKey.SetValue("", "Convert to " + preset.Name);
            presetKey.CreateSubKey("command");
            presetKey.Close();
            RegistryKey presetCommandKey = Registry.ClassesRoot.OpenSubKey(@"\*\shell\WEMC\shell\" + preset.Name + @"\command", true);
            presetCommandKey.SetValue("", $"\"{System.Environment.ProcessPath}\" -dir \"%1\" -p \"{preset.Name}\"");
        }
        shellKey.Close();
        
        RegistryKey mainActionKey = Registry.ClassesRoot.OpenSubKey(@"\*\shell\WEMC\shell\mainAction", true);
        mainActionKey.SetValue("","Convert File");
        mainActionKey.CreateSubKey("command");
        mainActionKey.Close();
        
        RegistryKey mainActionCommandKey = Registry.ClassesRoot.OpenSubKey(@"\*\shell\WEMC\shell\mainAction\command", true);
        mainActionCommandKey.SetValue("", $"\"{System.Environment.ProcessPath}\" -dir \"%1\"");
        
    }
}