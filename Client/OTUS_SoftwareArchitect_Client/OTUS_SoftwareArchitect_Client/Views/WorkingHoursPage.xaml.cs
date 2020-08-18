using OTUS_SoftwareArchitect_Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OTUS_SoftwareArchitect_Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WorkingHoursPage : ContentPageBase
    {
        public WorkingHoursPage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var vm = BindingContext as WorkingHoursViewModel;
            vm.CreateRecordRequested += OnCreateRecordRequested;
        }

        private void OnCreateRecordRequested(object sender, EventArgs e)
        {
            Navigation.PushAsync(new CreateWorkingHoursRecordPage());
        }
    }
}