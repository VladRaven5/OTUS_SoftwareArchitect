using OTUS_SoftwareArchitect_Client.ViewModels;
using Xamarin.Forms.Xaml;

namespace OTUS_SoftwareArchitect_Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProjectListsPage : ContentPageBase
    {
        public ProjectListsPage()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var vm = BindingContext as ProjectListsViewModel;

            vm.DisplayPromptAsync = DisplayCustomPromptAsync;
        }        
    }
}