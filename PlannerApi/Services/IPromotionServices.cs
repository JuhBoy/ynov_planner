using System.Threading.Tasks;
using events_planner.Models;

namespace events_planner.Services {
    public interface IPromotionServices {
        Promotion GetStaffPromotion();
        Promotion GetPromotionById(int id);
    }
}