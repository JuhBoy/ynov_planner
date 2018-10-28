namespace events_planner.Constants
{
    public static class ApiErrors
    {
        /// <summary>
        /// When User is already Booked to a specific event
        /// </summary>
        public static readonly string AlreadyBooked = "Booking:AlreadyBooked";
        
        /// <summary>
        /// When Event is expired and user try to subscribe
        /// </summary>
        public static readonly string EventExpired = "Booking:EventExpired";
        
        /// <summary>
        /// When subscription are not open
        /// </summary>
        public static readonly string SubscriptionNotOpen = "Booking:SubscribeNotOpen";
        
        /// <summary>
        /// When user is not allowed to subscribe
        /// </summary>
        public static readonly string SubscriptionNotPermitted = "Booking:SubscribeNotPermitted";
        
        /// <summary>
        /// When Subscription are actually at maximum capacity
        /// </summary>
        public static readonly string SubscriptionOverFlow = "Booking:SubscriptionOverflow";

        /// <summary>
        /// When User model is null
        /// </summary>
        public static readonly string UserNotFound = "User:NotFound";
        
        /// <summary>
        /// When Event model is null
        /// </summary>
        public static readonly string EventNotFound = "Event:NotFound";

        /// <summary>
        /// When Ids sent to csv export are empty
        /// </summary>
        public static readonly string CSVEmptySet = "CsvExport:EmptyIds";

        /// <summary>
        /// Unknown Internal error
        /// </summary>
        public static readonly string CsvErrorInternal = "CsvExport:InternalError";

        /// <summary>
        /// Error on saving data to DB
        /// </summary>
        public static readonly string JuryPointDbError = "JuryPoint:DatabaseInternalError";

        /// <summary>
        /// Jury point not found
        /// </summary>
        public static readonly string JuryPointNotFound = "JuryPoint:NotFound";

        /// <summary>
        /// When Deletion has been asked for a JuryPoint that hold an event reference
        /// </summary>
        public static readonly string JuryPointInvalidDelete = "JuryPoint:InvalidDeleteRequest";
    }
}