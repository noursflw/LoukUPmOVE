using loukupm.View;
namespace loukupm
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            MainPage.Dispatcher.Dispatch(async () =>
            {
                await Shell.Current.GoToAsync("MainPage");

            });

        }
    }
}