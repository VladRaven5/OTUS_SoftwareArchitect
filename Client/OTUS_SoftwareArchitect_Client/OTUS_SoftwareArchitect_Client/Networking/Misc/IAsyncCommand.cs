using System.Threading.Tasks;
using System.Windows.Input;

namespace OTUS_SoftwareArchitect_Client.Networking.Misc
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync();
        bool CanExecute();
    }

    public interface IAsyncCommand<T> : ICommand
    {
        Task ExecuteAsync(T arg);
        bool CanExecute(T arg);
    }
}
