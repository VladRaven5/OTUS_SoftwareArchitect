using OTUS_SoftwareArchitect_Client.Models;
using OTUS_SoftwareArchitect_Client.Networking.Misc;
using OTUS_SoftwareArchitect_Client.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.ViewModels
{
    public class NotificationsViewModel : BaseViewModel, IViewLoadingAware
    {
        private readonly NotificationsService _notificationsService;
        private IEnumerable<NotificationModel> _notifications;

        public NotificationsViewModel()
        {
            _notificationsService = DependencyService.Resolve<NotificationsService>();
            RefreshCommand = new AsyncCommand(RefreshNotificationsAsync);
        }

        public void OnViewAppearing()
        {
            RefreshCommand?.Execute(null);
        }

        public IEnumerable<NotificationModel> Notifications
        {
            get => _notifications;
            set
            {
                _notifications = value;
                OnPropertyChanged();
            }
        }


        public ICommand RefreshCommand { get; }

        private async Task RefreshNotificationsAsync()
        {
            IsBusy = true;

            try
            {
                var notificationsResult = await _notificationsService.GetMyNotificationsAsync();
                if (!notificationsResult.IsSuccess)
                {
                    ShowToast(notificationsResult.GetFullMessage());
                    return;
                }

                Notifications = notificationsResult.Result.OrderByDescending(n => n.CreatedDate).ToList();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
