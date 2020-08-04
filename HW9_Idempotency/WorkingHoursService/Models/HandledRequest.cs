using System;

namespace WorkingHoursService
{
    public class HandledRequest
    {
        public string RequestId { get; set; }
        public DateTimeOffset InvalidateAt { get; set; }
    }
}