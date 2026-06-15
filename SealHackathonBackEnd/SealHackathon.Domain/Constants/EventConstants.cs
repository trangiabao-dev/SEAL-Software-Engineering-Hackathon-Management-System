using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Constants;

public static class EventConstants
{
    /// <summary>
    /// Event.Status: Trạng thái Event
    /// </summary>
    public static class Status
    {
        public const string Draft = "Draft";
        public const string Registration = "Registration";
        public const string Active = "Active";
        public const string Completed = "Completed";

        public static readonly HashSet<string> ValidStatuses =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Draft,
                Registration,
                Active,
                Completed
            };
    }
}
