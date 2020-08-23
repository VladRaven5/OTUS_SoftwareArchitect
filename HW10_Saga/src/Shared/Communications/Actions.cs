namespace Shared
{
    public static class MessageActions
    {
        public const string Created = "created";
        public const string Updated = "updated";
        public const string Deleted = "deleted";
    }

    public static class TransactionMessageActions
    {
        public const string MoveTask_PrepareListRequested = "move_task_prepare_list_requested";
    }
}