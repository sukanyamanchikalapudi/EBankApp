using AutoMapper;
using EBankApp.DatabaseContext;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EBankApp.Models
{
    public partial class BaseController : Controller
    {
        public readonly AppDbContext appDbContext;
        public const int ACCOUNT_NUMBER_LENGTH = 11;
        public const int INITIAL_ACCOUNT_BALANCE = 0;
        public readonly IMapper _mapper;
        public AccountTypeEnum DEFAULT_ACCOUNT_TYPE = AccountTypeEnum.SAVINGS;
        public string CurrenyExchangeAPIUrl = ConfigurationManager.AppSettings.Get("CurrenyExchangeAPI");
        public BaseController(IMapper mapper)
        {
            appDbContext = new AppDbContext();
            _mapper = mapper;
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
            //var currentUser = GetCurrentUser;

            //var activity = new UserActivity
            //{
            //    UserId = currentUser.Id,
            //    Name = userActivityEnum,
            //    CreatedBy = currentUser.Id,
            //    CreatedOn = DateTime.UtcNow
            //};
            //appDbContext.UserActivities.Add(activity);
            //await appDbContext.SaveChangesAsync();
        }

        public async Task CreateTransaction(Transaction transaction)
        {
            using (var appDbContext = new AppDbContext())
            {
                appDbContext.Transactions.Add(transaction);
                await appDbContext.SaveChangesAsync();
            }
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
                currentUser = GetCurrentUser;
                id = currentUser.Id;
            }
            else
            {
                id = Convert.ToInt32(userId);
            }
            var accounts = await appDbContext.Accounts.Where(x => x.UserId == id).ToListAsync();
            var user = await appDbContext.Users.FindAsync(id);

            ClearCurrentSession();
            AttachToContext(ApplicationKeys.SessionKeys.Accounts, accounts);
            AttachToContext(ApplicationKeys.SessionKeys.User, user);
        }

        public User GetCurrentUser
        {
            get
            {
                return HttpContext.Session[ApplicationKeys.SessionKeys.User] as User ?? new User();
            }
        }

        [HttpGet]
        public async Task<ActionResult> Error()
        {
            await LogActivity(UserActivityEnum.ERROR_PAGE);
            return View();
        }

        public void ClearCurrentSession()
        {
            HttpContext.Session.Clear();
        }

        public void CancelCurrentSession()
        {
            HttpContext.Session.Abandon();
        }

        public PagingOptions DataTableOptions()
        {
            string start = HttpContext.Request["start"];
            string length = HttpContext.Request["length"];
            string searchValue = HttpContext.Request["search[value]"];
            string sortColumnName = HttpContext.Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = HttpContext.Request["order[0][dir]"];

            var options = new PagingOptions();

            if (!string.IsNullOrEmpty(start))
            {
                options.PageNumber = int.Parse(start);
            }

            if (!string.IsNullOrEmpty(length))
            {
                options.PageSize = int.Parse(length);
            }

            if (!string.IsNullOrEmpty(searchValue))
            {
                options.SearchKey = searchValue;
            }

            if (!string.IsNullOrEmpty(sortColumnName))
            {
                options.SortColumn = sortColumnName;
            }

            if (!string.IsNullOrEmpty(sortDirection))
            {
                options.SortBy = sortDirection;
            }
            return options;
        }

        public bool IsAlreadyLoggedIn()
        {
            var user = HttpContext.Session[ApplicationKeys.SessionKeys.User] as User;
            if (user != null)
            {
                return true;
            }
            return false;
        }

        public List<string> GetModelErrors(ModelStateDictionary state)
        {
            var str = new List<string>();

            foreach (var item in state.SelectMany(x => x.Value.Errors))
            {
                    str.Add(item.ErrorMessage);
            }
            return str;
        }
    }
}