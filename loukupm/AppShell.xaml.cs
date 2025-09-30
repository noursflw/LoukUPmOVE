using loukupm.View;
namespace loukupm
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(SinginPage), typeof(SinginPage));
            Routing.RegisterRoute(nameof(TerminbuchenPage), typeof(TerminbuchenPage));
            Routing.RegisterRoute(nameof(Paymentgetway), typeof(Paymentgetway));

        }

    }
}