using System.Threading.Tasks;

namespace OTUS_SoftwareArchitect_Client.Helpers
{
    public static class TaskHelpers
    {
        public static void ThrowIfFaulted(this Task task)
        {
            if (task.Status == TaskStatus.Faulted)
                throw task.Exception;
        }
    }
}
