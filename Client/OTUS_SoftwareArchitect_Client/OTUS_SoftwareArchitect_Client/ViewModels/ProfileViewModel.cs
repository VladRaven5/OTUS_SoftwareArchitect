using OTUS_SoftwareArchitect_Client.DTO.ListDtos;
using OTUS_SoftwareArchitect_Client.Infrastructure;
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
    public class ProfileViewModel : BaseViewModel, IViewLoadingAware
    {
        private readonly string _userId;        
        private readonly UsersService _usersService;
        private int _userVersion;
        private string _username;
        private string _phoneNumber;
        private string _email;
        private IEnumerable<UserRegions> _availableRegions;
        private UserRegions _selectedRegion;

        public ProfileViewModel()
        {
            _userId = AuthDataProvider.GetUserId();
            _usersService = DependencyService.Resolve<UsersService>();
            AvailableRegions = _usersService.AvailabelRegions;

            SaveCommand = new AsyncCommand(SaveAsync);
            LogoutCommand = new AsyncCommand(LogoutAsync);
            RefreshCommand = new AsyncCommand(RefreshAsync);
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
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

        public ICommand RefreshCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LogoutCommand { get; }

        public event EventHandler LogoutCompleted;

        public void OnViewAppearing()
        {
            RefreshCommand?.Execute(null);
        }

        private async Task RefreshAsync()
        {
            IsBusy = true;

            try
            {
                var userResult = await  _usersService.GetUserAsync(_userId);
                if(!userResult.IsSuccess)
                {
                    ShowToast(userResult.GetFullMessage());
                    return;
                }
                var user = userResult.Result;
                _userVersion = user.Version;
                Username = user.Username;
                SelectedRegion = AvailableRegions.First(r => r.Key == user.Region);
                PhoneNumber = user.PhoneNumber;
                Email = user.Email;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                if(string.IsNullOrEmpty(Username))
                {
                    ShowToast("Username can't be empty");
                    return;
                }

                var updationDto = new UpdateUserDto
                {
                    Id = _userId,
                    Version = _userVersion,
                    Username = Username,
                    PhoneNumber = PhoneNumber,
                    Email = Email,
                    Region = SelectedRegion.Key
                };

                var updateResult = await _usersService.UpdateUserAsync(updationDto);
                if(!updateResult.IsSuccess)
                {
                    ShowToast(updateResult.GetFullMessage());
                    return;
                }

                _userVersion = updateResult.Result.Version;
            }
            catch (Exception e)
            {

            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _usersService.LogoutAsync();
                AuthDataProvider.ResetAuthData();

                LogoutCompleted?.Invoke(this, EventArgs.Empty);                
            }
            catch(Exception e)
            {

            }
        }
    }
}
