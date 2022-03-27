using EBankApp.Attributes;
using EBankApp.DatabaseContext;
using EBankApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
    }
}