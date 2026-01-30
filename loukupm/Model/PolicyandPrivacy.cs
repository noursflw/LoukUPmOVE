using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace loukupm.Model
{
   public partial class PolicyandPrivacyS :ObservableObject
    {
        public int id {  get; set; }
        public string LastUpate {  get; set; }
        [ObservableProperty]
        public string description;

    }
}
