using EBankApp.Attributes;
using EBankApp.DatabaseContext;
using EBankApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EBankApp.Controllers
{
    [EBankAuthorized]
    public partial class AccountController : BaseController
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
            return View();
        }

        [HttpGet]
        [EBankAuthorized]
        public async Task<ActionResult> GetMyAccounts(string userId)
        {
            await LogActivity(UserActivityEnum.MY_ACCOUNTS);

            var id = Convert.ToInt32(userId);
            int start = Convert.ToInt32(HttpContext.Request["start"] ?? "1");
            int length = Convert.ToInt32(HttpContext.Request["length"] ?? "10");
            string searchValue = HttpContext.Request["search[value]"];
            string sortColumnName = HttpContext.Request["columns[" + Request["order[0][column]"] + "][name]"] ?? "AccountNumber";
            string sortDirection = HttpContext.Request["order[0][dir]"] ?? "asc";

            List<Account> accounts;

            using (appDbContext)
            {
                accounts = await appDbContext.Accounts.Where(x => x.UserId == id).ToListAsync();
                int totalrows = accounts.ToList().Count;
                if (!string.IsNullOrEmpty(searchValue))//filter
                {
                    accounts = accounts.Where(x => x.AccountNumber.ToLower().Contains(searchValue.ToLower())).ToList();
                }
                int totalrowsafterfiltering = accounts.ToList().Count;

                try
                {
                    accounts = accounts.OrderBy<Account>(sortColumnName + " " + sortDirection).ToList();
                }
                catch (Exception ex)
                {
                    throw;
                }

                //paging
                accounts = accounts.Skip(start).Take(length).ToList<Account>();

                return Json(new { data = accounts, draw = Request["draw"], recordsTotal = totalrows, recordsFiltered = totalrowsafterfiltering }, JsonRequestBehavior.AllowGet);

            }
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
                    AccountBalance = 0,
                    Currency = (int)CurrencyCode.GBP
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
                    var accountNumbers = new List<string>();

                    var accountsNum = await appDbContext.Accounts.Where(x => x.UserId == user.Id).GroupBy(x => x.AccountNumber).AsNoTracking().ToListAsync();
                    var accounts = await appDbContext.Accounts.Where(x => x.UserId == user.Id).AsNoTracking().ToListAsync();
                    accountsNum.ForEach(x => accountNumbers.Add(x.Key));

                    return Json(new
                    {
                        AccountNumbers = accountNumbers,
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
                        var exchagedAmount = amount * Convert.ToDouble(value);

                        var sourceAccountType = (int)GetCurrencyTypeEnum(exchangeCurrencyRequest.From);
                        var destinationAccountType = (int)GetCurrencyTypeEnum(exchangeCurrencyRequest.To);

                        var userId = Convert.ToInt32(exchangeCurrencyRequest.UserId);

                        var sourceAccount = appDbContext.Accounts.Where(x => x.UserId == userId && x.AccountNumber == exchangeCurrencyRequest.AccountNumber && x.Currency == sourceAccountType).FirstOrDefault();

                        Account destinationAccount = new Account();

                        if (!appDbContext.Accounts.Any(x => x.UserId == userId && x.AccountNumber == exchangeCurrencyRequest.AccountNumber && x.Currency == destinationAccountType))
                        {
                            destinationAccount = new Account()
                            {
                                AccountNumber = sourceAccount.AccountNumber,
                                Currency = (int)GetCurrencyTypeEnum(exchangeCurrencyRequest.To),
                                AccountType = AccountTypeEnum.SAVINGS,
                                UserId = sourceAccount.UserId
                            };
                        }
                        else
                        {
                            destinationAccount = await appDbContext.Accounts.Where(x => x.UserId == userId && x.AccountNumber == exchangeCurrencyRequest.AccountNumber && x.Currency == destinationAccountType).FirstOrDefaultAsync();
                        }

                        if (sourceAccount.AccountBalance > Convert.ToDouble(exchangeCurrencyRequest.Amount))
                        {
                            sourceAccount.AccountBalance -= exchagedAmount;

                            destinationAccount.AccountBalance += exchagedAmount;

                            appDbContext.Accounts.AddOrUpdate(destinationAccount);

                            var res = await appDbContext.SaveChangesAsync();
                            if (res > 0)
                            {
                                return Json(new { RedirectUrl = Url.Action("Success", "Base") });
                            }
                            else
                            {
                                return Json(new { RedirectUrl = Url.Action("Error", "Base") });
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

        [HttpPost]
        public async Task<ActionResult> InterBankTransfer(InterBankTransfer transfer)
        {
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    var amount = transfer.Amount;

                    var sourceAccountType = (int)GetCurrencyTypeEnum(transfer.Currency);
                    var destinationAccountType = (int)GetCurrencyTypeEnum(transfer.Currency);
                    var userId = Convert.ToInt32(transfer.UserId);

                    var sourceAccount = await appDbContext.Accounts.Where(x => x.UserId == userId && x.AccountNumber == transfer.Source && x.Currency == sourceAccountType).FirstOrDefaultAsync();

                    var destinationAccount = await appDbContext.Accounts.Where(x => x.UserId == userId && x.AccountNumber == transfer.Destination && x.Currency == destinationAccountType).FirstOrDefaultAsync();

                    if (sourceAccount.AccountBalance > Convert.ToDouble(transfer.Amount))
                    {
                        sourceAccount.AccountBalance -= amount;

                        destinationAccount.AccountBalance += amount;

                        var res = await appDbContext.SaveChangesAsync();
                        if (res > 0)
                        {
                            return Json(new { RedirectUrl = Url.Action("Success", "Base") });
                        }
                        else
                        {
                            return Json(new { RedirectUrl = Url.Action("Error", "Base") });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("InsufficientAmount", "You do not have enough funds.");
                        return View(transfer);
                    }
                }
            }
            return View(transfer);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteAccount(DeleteAccountRequest deleteAccountRequest)
        {
            using (appDbContext)
            {
                var account = await appDbContext.Accounts.Where(x => x.AccountNumber == deleteAccountRequest.AccountNumber && x.Currency == deleteAccountRequest.Currency && x.UserId == deleteAccountRequest.UserId).FirstOrDefaultAsync();
                appDbContext.Accounts.Remove(account);
                var result = await appDbContext.SaveChangesAsync();
                if (result > 0)
                {
                    return Json(new { Status = "success" });
                }
            }
            throw new Exception();
        }

        public class ExchangeValue
        {
            public string Value { get; set; }
        }

        public class DeleteAccountRequest
        {
            public int UserId { get; set; }
            public string AccountNumber { get; set; }
            public int Currency { get; set; }
        }
    }
}