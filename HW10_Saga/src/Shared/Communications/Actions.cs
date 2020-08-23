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
        public const string MoveTask_PrepareListRequested = "movetask_preparelist_req";
        public const string MoveTask_PrepareListCompleted = "movetask_preparelist_ok";



        public const string MoveTask_Rollback = "movetask_rollback";
        public const string MoveTask_Complete = "movetask_complete";
    }
}