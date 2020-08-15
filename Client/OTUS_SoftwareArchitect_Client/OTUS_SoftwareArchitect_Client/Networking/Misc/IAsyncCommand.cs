using System.Threading.Tasks;
using System.Windows.Input;

namespace OTUS_SoftwareArchitect_Client.Networking.Misc
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync();
        bool CanExecute();
    }
}
