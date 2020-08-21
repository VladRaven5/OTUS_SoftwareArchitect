using System.Threading.Tasks;

namespace Shared
{
    public static class TasksHelpers
    {
        public static void CheckAndThrowIfFaulted(this Task task)
        {
            if(task.Status == TaskStatus.Faulted)
                throw task.Exception;
        }
    } 
}