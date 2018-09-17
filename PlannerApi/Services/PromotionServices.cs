using System.Linq;
using events_planner.Services;
using events_planner.Models;

namespace Microsoft.Extensions.DependencyInjection {
    public class PromotionServices : IPromotionServices {

        private PlannerContext Context { get; set; }

        public PromotionServices(PlannerContext context) {
            Context = context;
        }

        public Promotion GetForeignPromotion() {
            return Context.Promotion
                          .FirstOrDefault((Promotion arg) => arg.Name == "Foreigner");
        }

        public Promotion GetPromotionById(int id) {
            return Context.Promotion.FirstOrDefault((Promotion arg) => arg.Id == id);
        }

        public Promotion GetPromotionByName(string name) {
            return Context.Promotion.FirstOrDefault((Promotion arg) => arg.Name == name);
        }
    }
}