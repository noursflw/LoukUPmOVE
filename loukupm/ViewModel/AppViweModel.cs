using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;


namespace loukupm.ViewModel
{
    public partial class AppViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isCouselLoad;

        [ObservableProperty]
        private bool isWorkTeamLoad;

        [ObservableProperty]
        private bool isServicesLoad;
        [ObservableProperty]
        private bool isCatogory;
        [ObservableProperty]
        private bool isloadday;
        [ObservableProperty]
        private bool isTimework;
        [ObservableProperty]
        private bool invoiceLoad;
        [ObservableProperty]
        private bool isloadBooking;
        [ObservableProperty]
        private bool isLoadNotifiction;

        public AppViewModel()
        {
            LoadData();
        }

        private async void LoadData()
        {
            IsCouselLoad = true;
            IsWorkTeamLoad = true;
            IsServicesLoad = true;
            IsCatogory = true;
            Isloadday = true;
            IsTimework= true;
            InvoiceLoad = true;
            IsloadBooking = true;
            IsLoadNotifiction = true;
            // محاكاة وقت تحميل البيانات


            //IsCouselLoad = false;
            //IsWorkTeamLoad = false;
            //IsServicesLoad = false;
            //IsCatogory = false;
            //Isloadday = false;
        }
    }
}
