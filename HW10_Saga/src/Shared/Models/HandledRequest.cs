using System;

namespace Shared
{
    public class HandledRequest
    {
        public string RequestId { get; set; }
        public DateTimeOffset InvalidateAt { get; set; }
    }
}