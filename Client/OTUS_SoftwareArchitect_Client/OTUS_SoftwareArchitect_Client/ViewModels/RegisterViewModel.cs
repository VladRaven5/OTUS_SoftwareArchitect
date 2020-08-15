using OTUS_SoftwareArchitect_Client.Networking.Misc;
using OTUS_SoftwareArchitect_Client.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        private string _login;
        private string _password;
        private string _username;

        public RegisterViewModel()
        {
            _authService = DependencyService.Resolve<AuthService>();
            RegisterCommand = new AsyncCommand(RegisterAsync);
        }        

        public string Username
        {
            get =>_username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }


        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public ICommand RegisterCommand { get; }


        public event EventHandler Registered;


        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                ShowToast("Username can't be empty");
                return;
            }

            if (string.IsNullOrWhiteSpace(Login))
            {
                ShowToast("Login can't be empty");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ShowToast("Password can't be empty");
                return;
            }

            var result = await _authService.RegisterAsync(Username, Login, Password);
            if(!result.IsSuccess)
            {
                ShowToast(result.GetFullMessage());
                return;
            }

            Registered?.Invoke(this, EventArgs.Empty);
        }
    }
}
