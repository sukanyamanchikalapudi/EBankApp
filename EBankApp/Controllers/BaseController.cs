using EBankApp.DatabaseContext;
using EBankApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EBankApp.Controllers
{
    public partial class BaseController : Controller
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

        public static CurrencyCode GetCurrencyTypeEnum(string currencyCode)
        {
            if (currencyCode == CurrencyCode.GBP.ToString())
            {
                return CurrencyCode.GBP;
            }
            else if (currencyCode == CurrencyCode.USD.ToString())
            {
                return CurrencyCode.USD;
            }
            return CurrencyCode.Unknown;
        }

        [HttpGet]
        public ActionResult Success()
        {
            return View();
        }

        public async Task UpdateSession(string userId = "")
        {
            User currentUser;
            int id;
            if (string.IsNullOrEmpty(userId))
            {
                currentUser = HttpContext.Session["User"] as User;
                id = currentUser.Id;
            }
            else
            {
                id = Convert.ToInt32(userId);
            }
            var accounts = await appDbContext.Accounts.Where(x => x.UserId == id).ToListAsync();
            var user = await appDbContext.Users.FindAsync(id);

            // remove session.
            RemoveFromContext("User");
            RemoveFromContext("Accounts");
            // update
            AttachToContext<List<Account>>("Accounts", accounts);
            AttachToContext<User>("User", user);
        }

        [HttpGet]
        public async Task<ActionResult> Error()
        {
            await LogActivity(UserActivityEnum.ERROR_PAGE);
            return View();
        }
    }
}