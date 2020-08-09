using Shared;

namespace UsersService
{
    public static class ModelHelpers
    {
        public static RavendbOutboxMessageModel ToRavendDb(this OutboxMessageModel baseModel)
        {
            return new RavendbOutboxMessageModel(baseModel);
        }
    }
}