using OTUS_SoftwareArchitect_Client.Networking.Misc;
using OTUS_SoftwareArchitect_Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly UsersService _usersService;

        private string _login;
        private string _password;
        private string _username;
        private IEnumerable<UserRegions> _availableRegions;
        private UserRegions _selectedRegion;

        public RegisterViewModel()
        {
            _authService = DependencyService.Resolve<AuthService>();
            _usersService = DependencyService.Resolve<UsersService>();

            RegisterCommand = new AsyncCommand(RegisterAsync);

            AvailableRegions = _usersService.AvailabelRegions;
            SelectedRegion = AvailableRegions.First();
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

        public IEnumerable<UserRegions> AvailableRegions
        {
            get => _availableRegions;
            set
            {
                _availableRegions = value;
                OnPropertyChanged();
            }
        }

        public UserRegions SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                _selectedRegion = value;
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

            IsBusy = true;

            try
            {
                var result = await _authService.RegisterAsync(Username, SelectedRegion.Key, Login, Password);
                if (!result.IsSuccess)
                {
                    ShowToast(result.GetFullMessage());
                    return;
                }

                Registered?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                IsBusy = false;
            }            
        }
    }
}
