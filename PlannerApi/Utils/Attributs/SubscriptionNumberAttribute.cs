using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using events_planner.Constants;

namespace events_planner.Utils.Attributs {
    
    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SubscriptionNumberAttribute : ValidationAttribute {
        
        private string Target { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SubscriptionNumberAttribute(string target) {
            Target = target;
        }

        protected override ValidationResult IsValid(object number, ValidationContext validationContext) {
            var property = validationContext.ObjectType.GetProperty(Target);
            if (property == null) {
                return new ValidationResult($"Unknown property {Target}");
            }

            var maxNumber = (int)property.GetValue(validationContext.ObjectInstance, null);

            if ((int)number >= maxNumber) {
                return new ValidationResult(ApiErrors.SubscriptionOverFlow, new [] { validationContext.DisplayName });
            }

            return ValidationResult.Success;
        }
    }
}