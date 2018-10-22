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
    }
}