using System.Windows;

namespace WEMC;

public partial class Alert : Window
{
    public Alert(string title, string message)
    {
        InitializeComponent();
        Title = title;
        TextBlock.Text = message;
    }


    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        Close();
    }
}