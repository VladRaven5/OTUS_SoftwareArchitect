using OTUS_SoftwareArchitect_Client.DTO;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://xamarin.com"));

            LoginCommand = new Command(Login);
            AuthCommand = new Command(Auth);
        }

        public ICommand OpenWebCommand { get; }

        public ICommand LoginCommand { get; }
        public ICommand AuthCommand { get; set; }

        public void Auth()
        {           
            
        }

        public void Login()
        {            
        }
    }
}