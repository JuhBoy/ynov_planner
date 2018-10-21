namespace events_planner.Constants
{
    internal static class ApiErrors
    {
        /// <summary>
        /// When User is already Booked to a specific event
        /// </summary>
        internal static readonly string AlreadyBooked = "Booking:AlreadyBooked";
        
        /// <summary>
        /// When Event is expired and user try to subscribe
        /// </summary>
        internal static readonly string EventExpired = "Booking:EventExpired";
        
        /// <summary>
        /// When subscription are not open
        /// </summary>
        internal static readonly string SubscriptionNotOpen = "Booking:SubscribeNotOpen";
        
        /// <summary>
        /// When user is not allowed to subscribe
        /// </summary>
        internal static readonly string SubscriptionNotPermitted = "Booking:SubscribeNotPermitted";
        
        /// <summary>
        /// When Subscription are actually at maximum capacity
        /// </summary>
        internal static readonly string SubscriptionOverFlow = "Booking:SubscriptionOverflow";

        /// <summary>
        /// When User model is null
        /// </summary>
        internal static readonly string UserNotFound = "User:NotFound";
        
        /// <summary>
        /// When Event model is null
        /// </summary>
        internal static readonly string EventNotFound = "Event:NotFound";
    }
}