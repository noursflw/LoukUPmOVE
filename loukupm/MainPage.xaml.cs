namespace loukupm
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }


      
        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            Shell.Current.GoToAsync("//LoginPage");
        }
    }

}
