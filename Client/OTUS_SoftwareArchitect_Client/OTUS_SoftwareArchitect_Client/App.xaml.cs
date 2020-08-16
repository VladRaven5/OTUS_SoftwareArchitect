using Xamarin.Forms;
using OTUS_SoftwareArchitect_Client.Services;
using OTUS_SoftwareArchitect_Client.Views;
using OTUS_SoftwareArchitect_Client.Networking;
using OTUS_SoftwareArchitect_Client.Infrastructure;

namespace OTUS_SoftwareArchitect_Client
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            RegisterDependencies();

            bool isUserCredsExists = AuthDataProvider.HasAuthData();
            Page initialPage = isUserCredsExists
                ? (Page)new MainPage()
                : new NavigationPage(new AuthPage());

            MainPage = initialPage;
        }

        private static void RegisterDependencies()
        {
            DependencyService.Register<MockDataStore>();
            DependencyService.Register<WebApiProvider>();
            DependencyService.Register<WebApiClient>();
            DependencyService.Register<AuthService>();
            DependencyService.Register<TasksService>();
            DependencyService.Register<ProjectsService>();
            DependencyService.Register<UsersService>();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
