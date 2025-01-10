using Telerik.Maui.Controls.Compatibility;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        Telerik.Maui.Controls.Compatibility.Common.Initializer.Init();
        MainPage = new AppShell();
    }
}
