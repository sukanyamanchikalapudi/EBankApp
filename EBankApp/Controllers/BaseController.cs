using EBankApp.DatabaseContext;
using EBankApp.Models;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EBankApp.Controllers
{
    public class BaseController : Controller
    {
        public readonly AppDbContext appDbContext;
        public const int ACCOUNT_NUMBER_LENGTH = 11;
        public const int INITIAL_ACCOUNT_BALANCCE = 0;
        public AccountTypeEnum DEFAULT_ACCOUNT_TYPE = AccountTypeEnum.SAVINGS;
        public BaseController()
        {
            appDbContext = new AppDbContext();
        }

        public void AttachToContext<T>(string name, T t)
        {
            HttpContext.Session.Add(name, t);
        }

        public void RemoveFromContext(string name)
        {
            HttpContext.Session.Remove(name);
        }

        public async Task LogActivity(UserActivityEnum userActivityEnum)
        {
            var currentUser = HttpContext.Session["User"] as User ?? null;

            var activity = new UserActivity
            {
                UserId = currentUser.Id,
                Name = userActivityEnum,
                CreatedBy = currentUser.Id,
                CreatedOn = System.DateTime.UtcNow
            };
            appDbContext.UserActivities.Add(activity);
            await appDbContext.SaveChangesAsync();
        }
    }
}