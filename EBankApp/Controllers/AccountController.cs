using AutoMapper;
using EBankApp.Attributes;
using EBankApp.Controllers.Models;
using EBankApp.DatabaseContext;
using EBankApp.ExtensionMethods;
using EBankApp.Helpers;
using EBankApp.Models;
using Newtonsoft.Json;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
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

        public AccountController(IMapper mapper) : base(mapper)
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
        public async Task<ActionResult> GetMyAccounts(int userId)
        {
            await LogActivity(UserActivityEnum.MY_ACCOUNTS);

            int totalRows = 0;
            int filteredRecords = 0;

            if (userId > 0)
            {
                var options = DataTableOptions();
                List<Account> accounts = new List<Account>();

                using (appDbContext)
                {
                    var user = await appDbContext.Users.FindAsync(userId);

                    if (user == null)
                    {
                        return Json(new { Error = "Invalid userId" });
                    }

                    accounts = await appDbContext.Accounts.Where(x => x.UserId == user.Id).AsNoTracking().ToListAsync();

                    totalRows = accounts.Count;

                    if (!string.IsNullOrEmpty(options.SearchKey))
                        accounts = accounts.Where(x => x.AccountNumber.ToLower().Contains(options.SearchKey)).ToList();

                    filteredRecords = accounts.Count;

                    if (!string.IsNullOrEmpty(options.SortColumn) && !string.IsNullOrEmpty(options.SortBy))
                        accounts = accounts.OrderBy<Account>(options.SortColumn + " " + options.SortBy).ToList();

                    if (options.PageNumber.HasValue && options.PageSize.HasValue)
                        accounts = accounts.Skip(options.PageNumber.Value).Take(options.PageSize.Value).ToList();

                    return Json(new { data = accounts, draw = Request["draw"], recordsTotal = totalRows, recordsFiltered = filteredRecords }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Error = "Invalid userId" });
        }

        [HttpGet]
        [EBankAuthorized]
        public async Task<ActionResult> GetAllAccounts(string userId)
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
                accounts = await appDbContext.Accounts.ToListAsync();
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
            int id = -1;

            if (string.IsNullOrEmpty(userId))
            {
                id = GetCurrentUser.Id;
            }
            else
            {
                id = Convert.ToInt32(userId);
            }

            using (appDbContext)
            {
                appDbContext.Accounts.Add(new Account
                {
                    UserId = id,
                    AccountNumber = EBankHelper.GenerateAccountNumber(11),
                    AccountType = accountCreate.AccountType,
                    AccountBalance = 0,
                    Currency = (int)CurrencyCode.GBP
                });

                res = await appDbContext.SaveChangesAsync();
                if (res > 0)
                {
                    return RedirectToAction("MyAccounts", "Account", new { userId = userId });
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
                    payer.AccountBalance -= transferRequest.Amount;
                    payee.AccountBalance += transferRequest.Amount;
                    await appDbContext.SaveChangesAsync();

                    // Create transaction for payer
                    await CreateTransaction(new Transaction
                    {
                        FromAccount = transferRequest.PayerAccountNumber,
                        ToAccount = transferRequest.PayeeAccountNumber,
                        TransactionType = TransactionTypeEnum.DEBITED,
                        Debited = transferRequest.Amount,
                        AccountId = payer.Id,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = GetCurrentUser.Id
                    });

                    // Create transaction for payee
                    await CreateTransaction(new Transaction
                    {
                        FromAccount = transferRequest.PayerAccountNumber,
                        ToAccount = transferRequest.PayeeAccountNumber,
                        TransactionType = TransactionTypeEnum.CREDITED,
                        Credited = transferRequest.Amount,
                        AccountId = payee.Id,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = GetCurrentUser.Id
                    });

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
        [EBankAuthorized]
        public ActionResult GetTransactions()
        {
            return View();
        }

        [HttpGet]
        [EBankAuthorized]
        public async Task<ActionResult> GetAllTransactions()
        {
            await LogActivity(UserActivityEnum.MY_ACCOUNTS);

            int start = Convert.ToInt32(HttpContext.Request["start"] ?? "1");
            int length = Convert.ToInt32(HttpContext.Request["length"] ?? "10");
            string searchValue = HttpContext.Request["search[value]"];
            string sortColumnName = HttpContext.Request["columns[" + Request["order[0][column]"] + "][name]"] ?? "FromAccount";
            string sortDirection = HttpContext.Request["order[0][dir]"] ?? "asc";

            List<Transaction> transactions;

            using (appDbContext)
            {
                transactions = await appDbContext.Transactions.ToListAsync();
                transactions.ForEach(x => x.CreatedOn.Value.ToUniversalTime());
                int totalrows = transactions.ToList().Count;
                if (!string.IsNullOrEmpty(searchValue))//filter
                {
                    transactions = transactions.Where(x => x.ToAccount.ToLower().Contains(searchValue.ToLower())).ToList();
                }
                int totalrowsafterfiltering = transactions.ToList().Count;

                try
                {
                    if (!string.IsNullOrEmpty(sortColumnName) && !string.IsNullOrEmpty(sortDirection))
                        transactions = transactions.OrderBy<Transaction>(sortColumnName + " " + sortDirection).ToList();
                }
                catch (Exception ex)
                {
                    throw;
                }

                //paging
                transactions = transactions.Skip(start).Take(length).ToList<Transaction>();

                return Json(new { data = transactions, draw = Request["draw"], recordsTotal = totalrows, recordsFiltered = totalrowsafterfiltering }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpGet]
        public async Task<ActionResult> GetCurrencies()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(CurrenyExchangeAPIUrl);
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
        public async Task<ActionResult> PrintTransaction(PrintTransactionRequest request)
        {
            if (string.IsNullOrEmpty(request.Start) && string.IsNullOrEmpty(request.End) && !string.IsNullOrEmpty(request.ReportType))
            {
                using (appDbContext)
                {
                    int reportType = int.Parse(request.ReportType);
                    DateTime period;

                    period = (reportType == (int)TransactionReportTime.THIS_WEEK) ? DateTime.Now.StartOfWeek(DayOfWeek.Monday) : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                    var transactions = await appDbContext.Transactions.Where(x => (x.CreatedOn > period) && (x.FromAccount == request.AccountNumber)).ToListAsync();

                    var result = new TransactionStatementResult
                    {
                        AccountNumber = request.AccountNumber,
                        Transactions = transactions,
                        Date = DateTime.UtcNow
                    };

                    HttpContext.Session["TransactionStatements"] = result;

                    return Json(new { RedirectUrl = Url.Action("TransactionStatment", "Account") });
                }
            }
            else if (!string.IsNullOrEmpty(request.Start) && !string.IsNullOrEmpty(request.End) && string.IsNullOrEmpty(request.ReportType))
            {
                using (appDbContext)
                {
                    var start = DateTime.Parse(request.Start);
                    var end = DateTime.Parse(request.End);

                    try
                    {
                        var transactions = await appDbContext.Transactions.Where(x => x.CreatedOn >= start && x.CreatedOn <= end && (x.FromAccount == request.AccountNumber)).ToListAsync();
                        var result = new TransactionStatementResult
                        {
                            AccountNumber = request.AccountNumber,
                            Transactions = transactions,
                            Date = DateTime.UtcNow
                        };

                        HttpContext.Session["TransactionStatements"] = result;
                    }
                    catch (Exception e)
                    {

                        throw;
                    }
                    return Json(new { RedirectUrl = Url.Action("TransactionStatment", "Account") });
                }
            }
            return Json(new { RedirectUrl = Url.Action("Error") });
        }


        public async Task<ActionResult> TransactionStatment()
        {
            return new ViewAsPdf();
        }

        [HttpGet]
        public ActionResult DownloadTransactionReport()
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
                    client.BaseAddress = new Uri(CurrenyExchangeAPIUrl);
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
                            return Json(new { RedirectUrl = "", ErrorMessages = GetModelErrors(ModelState) });
                        }
                    }
                }
            } 
            else
            {
                return Json(new { RedirectUrl = "", ErrorMessages = GetModelErrors(ModelState) });
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
                            // Create transaction for payer
                            await CreateTransaction(new Transaction
                            {
                                FromAccount = transfer.Source,
                                ToAccount = transfer.Destination,
                                TransactionType = TransactionTypeEnum.DEBITED,
                                Debited = Convert.ToInt32(transfer.Amount),
                                AccountId = sourceAccount.Id,
                                CreatedOn = DateTime.UtcNow,
                                CreatedBy = GetCurrentUser.Id
                            });

                            // Create transaction for payee
                            await CreateTransaction(new Transaction
                            {
                                FromAccount = transfer.Source,
                                ToAccount = transfer.Destination,
                                TransactionType = TransactionTypeEnum.CREDITED,
                                Debited = Convert.ToInt32(transfer.Amount),
                                AccountId = destinationAccount.Id,
                                CreatedOn = DateTime.UtcNow,
                                CreatedBy = GetCurrentUser.Id
                            });
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
    }
}