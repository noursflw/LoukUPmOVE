using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace loukupm.Model
{
    public partial class DayItem : ObservableObject
    {
        public DayItem() { }

        [ObservableProperty]
        private string date;

        [ObservableProperty]
        private string day;

        [ObservableProperty]
        private bool isAvailable = false;

        [ObservableProperty]
        private string borderColor = "#444444";

        [ObservableProperty]
        private DateTime fullDate;
    }
}
