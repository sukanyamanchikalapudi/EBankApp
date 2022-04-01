using EBankApp.Attributes;
using EBankApp.DatabaseContext;
using EBankApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EBankApp.Controllers
{
    [EBankAuthorized]
    public class AccountController : BaseController
    {
        public AccountController()
        {
        }
        // GET: Account
        public async Task<ActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        [EBankAuthorized]
        public async Task<ActionResult> MyAccounts(string userId)
        {
            await LogActivity(UserActivityEnum.MY_ACCOUNTS);
            using (appDbContext)
            {
                var id = Convert.ToInt32(userId);
                var users = appDbContext.Accounts.Where(x => x.UserId == id).ToList();
                return View(users);
            }
            return View();
        }

        [HttpGet]
        [EBankAuthorized]
        public async Task<ActionResult> ManageAccounts()
        {
            await LogActivity(UserActivityEnum.MANAGE_ACCOUNTS);
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> QuickTransfer()
        {
            await LogActivity(UserActivityEnum.QUICK_TRANSFER);
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            await LogActivity(UserActivityEnum.ACCOUNT_CREATE);
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetAccounts()
        {
            var list = await appDbContext.Accounts.ToListAsync();
            var json = JsonConvert.SerializeObject(list);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> Create(string userId, AccountCreateRequest accountCreate)
        {
            await LogActivity(UserActivityEnum.ACCOUNT_CREATE);
            int res = 0;
            using (appDbContext)
            {
                appDbContext.Accounts.Add(new Account
                {
                    UserId = Convert.ToInt32(userId),
                    AccountNumber = EBankHelper.GenerateAccountNumber(11),
                    AccountType = accountCreate.AccountType,
                    AccountBalance = 0
                });

                res = await appDbContext.SaveChangesAsync();
                if (res > 0)
                {
                    return RedirectToAction("Edit", "User", new { userId = userId });
                }
            }

            return View(accountCreate);
        }

        [HttpPost]
        public async Task<ActionResult> QuickTransfer(QuickTransferRequest transferRequest)
        {
            await LogActivity(UserActivityEnum.QUICK_TRANSFER);
            var payer = await appDbContext.Accounts.Where(x => x.AccountNumber == transferRequest.PayerAccountNumber).FirstOrDefaultAsync();
            var payee = await appDbContext.Accounts.Where(x => x.AccountNumber == transferRequest.PayeeAccountNumber).FirstOrDefaultAsync();

            if (payer != null && payee != null)
            {
                if (payer.AccountBalance > transferRequest.Amount)
                {
                    payer.AccountBalance -= payer.AccountBalance - transferRequest.Amount;
                    payee.AccountBalance += payee.AccountBalance + transferRequest.Amount;
                    await appDbContext.SaveChangesAsync();
                    return View("ManageFunds");
                }
                else
                {
                    ModelState.AddModelError("INSUFFICIENT_FUNDS", "Insufficent funds available in your account");
                    return View(transferRequest);
                }
            }
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> ManageFunds()
        {
            await LogActivity(UserActivityEnum.MANAGE_FUNDS);
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> GetAccountDetails(int userId)
        {
            using (appDbContext)
            {
                var user = await appDbContext.Users.Where(x => x.Id == userId).AsNoTracking().SingleOrDefaultAsync();
                if (user != null)
                {
                    var accounts = await appDbContext.Accounts.Where(x => x.UserId == user.Id).AsNoTracking().ToListAsync();
                    return Json(new
                    {
                        Accounts = accounts,
                        user = user
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Message = "User not found" });
        }

        [HttpGet]
        public async Task<ActionResult> GetCurrencies()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44324/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("api/Exchanges");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error");
        }

        [HttpGet]
        public async Task<ActionResult> ExchangeCurrency()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ExchangeCurrency(ExchangeCurrencyRequest exchangeCurrencyRequest)
        {
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:44324/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync($"api/Exchanges?from={exchangeCurrencyRequest.From}&to={exchangeCurrencyRequest.To}");
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var value = JsonConvert.DeserializeObject<ExchangeValue>(result).Value;

                        var amount = Convert.ToDouble(exchangeCurrencyRequest.Amount);
                        var exchagedAmount = amount / Convert.ToDouble(value);

                        var account = appDbContext.Accounts.Where(x => x.AccountNumber == exchangeCurrencyRequest.AccountNumber).FirstOrDefault();

                        if (account.AccountBalance > Convert.ToDouble(exchangeCurrencyRequest.Amount))
                        {
                            account.AccountBalance = exchagedAmount;
                            account.Currency = (int)GetCurrencyTypeEnum(exchangeCurrencyRequest.To);

                            var res = await appDbContext.SaveChangesAsync();
                            if (res > 0)
                            {
                                return View("Success");
                            }
                            else
                            {
                                return View("Error");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("InsufficientAmount", "You do not have enough funds.");
                            return View(exchangeCurrencyRequest);
                        }
                    }
                }
            }

            return View(exchangeCurrencyRequest);
        }

        public class ExchangeValue
        {
            public string Value { get; set; }
        }
    }
}