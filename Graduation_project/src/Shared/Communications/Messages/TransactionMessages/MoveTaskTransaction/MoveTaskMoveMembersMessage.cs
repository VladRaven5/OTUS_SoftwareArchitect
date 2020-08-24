using System.Collections.Generic;

namespace Shared
{
    public class MoveTaskMoveMembersMessage : BaseTransactionMessage
    {
        public string SourceProjectId { get; set; }
        public string TargetProjectId { get; set; }
        public IEnumerable<string> TaskMembers { get; set; }
    }

}