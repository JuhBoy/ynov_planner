using System;

namespace events_planner.Utils {

    // Generator Exceptions:
    public class TemplateEmptyException : Exception {
        public TemplateEmptyException(string message) : base(message) { /* */ }
    }

    public class NotFoundUserException : Exception {
        public NotFoundUserException(string message): base(message) {}
    }

}