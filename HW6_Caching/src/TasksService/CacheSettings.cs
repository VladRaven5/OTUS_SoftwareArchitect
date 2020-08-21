using System;
using System.Linq;
using Shared;

namespace TasksService
{
    public static class CacheSettings
    {
        public static readonly string TaskIdCacheKeyPattern = "task-{0}"; // 0 - task id
        public static readonly TimeSpan TaskIdCacheLifetime = TimeSpan.FromMinutes(2);  

        public static readonly string UserTasksCacheKeyPattern = "user-tasks-{0}"; //0 - user id
        public static readonly TimeSpan UserTasksCacheLifetime = TimeSpan.FromMinutes(2);

        private static readonly string _tasksFilterCacheKeyPattern = "tasks-filter-{0}";
        public static readonly TimeSpan TasksFilterCacheLifetime = TimeSpan.FromMinutes(2);
        public static string GetFilterArgsCachekey(FilterTaskArgs args)
        {
            string AddArg(string sourceString, string arg)
            {
                if(string.IsNullOrWhiteSpace(sourceString))
                    return arg;

                return sourceString + "&" + arg;
            }

            string argsString = string.Empty;

            if(args.Title != null)
                argsString = AddArg(argsString, $"title={args.Title}");
            
            if(!args.LabelsIds.IsNullOrEmpty())
                argsString = AddArg(argsString, $"labels={string.Join(",",args.LabelsIds)}");

            if(!args.ListsIds.IsNullOrEmpty())
                argsString = AddArg(argsString, $"lists={string.Join(",",args.ListsIds)}");
            
            if(!args.UsersIds.IsNullOrEmpty())
                argsString = AddArg(argsString, $"users={string.Join(",",args.UsersIds)}");

            if(!args.States.IsNullOrEmpty())
                argsString = AddArg(argsString, $"states={string.Join(",",args.States.Select(s => (int)s))}");
            
            return string.Format(_tasksFilterCacheKeyPattern, argsString);
        }
    }
}