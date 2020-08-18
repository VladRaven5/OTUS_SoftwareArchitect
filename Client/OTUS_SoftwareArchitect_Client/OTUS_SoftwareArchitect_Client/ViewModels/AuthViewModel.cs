using OTUS_SoftwareArchitect_Client.Networking.Misc;
using OTUS_SoftwareArchitect_Client.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.ViewModels
{
    public class AuthViewModel : BaseViewModel
    {
        private string _login;
        private string _password;
        private AuthService _authService;

        public AuthViewModel()
        {
            _authService = DependencyService.Resolve<AuthService>();

            LoginCommand = new AsyncCommand(LoginAsync);
            RegisterCommand = new Command(() => RegistrationRequested?.Invoke());
        }

        private async Task LoginAsync()
        {
            if(string.IsNullOrWhiteSpace(Login))
            {
                ShowToast("Login can't be empty");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ShowToast("Password can't be empty");
                return;
            }

            IsBusy = true;

            try
            {
                var result = await _authService.LoginAsync(Login, Password);
                if (!result.IsSuccess)
                {
                    ShowToast(result.GetFullMessage());
                    return;
                }

                SuccessLogin?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                IsBusy = false;
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

        public ICommand LoginCommand { get;  }
        public ICommand RegisterCommand { get; set; }

        public event EventHandler SuccessLogin;
        public event Action RegistrationRequested;
    }
}
