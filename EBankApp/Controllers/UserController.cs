using AutoMapper;
using EBankApp.Attributes;
using EBankApp.Helpers;
using EBankApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EBankApp.Controllers
{
    public class UserController : BaseController
    {
        public UserController(IMapper mapper) : base(mapper)
        {
        }

        /// <summary>
        ///  GET ACTION
        ///  APP_LOGIN_VIEW
        /// </summary>
        /// <returns>View</returns>
        [HttpGet]
        [NoCache]
        public async Task<ActionResult> Login()
        {
            if (IsAlreadyLoggedIn())
            {
                await LogActivity(UserActivityEnum.USER_LOGIN);
                // Since the user already loggedin so redirecting to dashboard.
                return RedirectToAction("Dashboard");
            }
            return View();
        }

        [HttpGet]
        [NoCache]
        public async Task<ActionResult> Logout()
        {
            await LogActivity(UserActivityEnum.USER_LOGOUT);
            if (IsAlreadyLoggedIn())
            {
                CancelCurrentSession();
                return RedirectToActionPermanent("Login");
            }
            return View("Error");
        }

        [HttpGet]
        [NoCache]
        public async Task<ActionResult> Register()
        {
            if (IsAlreadyLoggedIn())
            {
                // Since the user is already loggedin so redirecting to dashboard.
                await LogActivity(UserActivityEnum.USER_REGISTER);
                return RedirectToActionPermanent("Dashboard");
            }
            return View();
        }

        /// <summary>
        ///  POST Action
        ///  APP_LOGIN_REQUEST
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Login(LoginRequest user)
        {
            if (ModelState.IsValid)
            {
                using (appDbContext)
                {
                    try
                    {
                        var dbUser = await appDbContext.Users
                                                        .SingleOrDefaultAsync(x => (x.UserName.ToLower() == user.UserName.Trim().ToLower() &&
                                                        x.Password.ToLower() == user.Password.Trim().ToLower()) ||
                                                        x.Accounts.Any(a => a.AccountNumber.ToLower() == user.UserName.ToLower() &&
                                                        x.PIN.ToLower() == user.Password.ToLower()));

                        if (dbUser == null)
                        {
                            ModelState.AddModelError("InvalidCredentails", "Invalid username or password");
                            return View(user);
                        }
                        else
                        {
                            var accounts = await appDbContext.Accounts.Where(x => x.UserId == dbUser.Id).AsNoTracking().ToListAsync();

                            // Attach to session
                            AttachToContext<User>(ApplicationKeys.SessionKeys.User, dbUser);

                            if (accounts.Any())
                            {
                                // Attach user account information to session
                                AttachToContext<List<Account>>(ApplicationKeys.SessionKeys.Accounts, accounts);
                            }

                            // Log user activity
                            await LogActivity(UserActivityEnum.USER_LOGIN);

                            // Redirect to dashboard on successfull registration.
                            return RedirectToActionPermanent("MyProfile", "User", new { userId = dbUser.Id });
                        }
                    }
                    catch (Exception e)
                    {
                        // Since there were exception need to clear out the session
                        CancelCurrentSession();
                        return RedirectToAction("Error");
                    }
                }
            }
            return View(user);
        }


        /// <summary>
        ///  
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Register(User model)
        {
            int actualTransactions = 0;
            int expectedTransactions = 2;
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new User
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PIN = model.PIN,
                        UserName = model.UserName,
                        Password = model.Password,
                        RoleId = (int)UserRoleEnum.User
                    };
                    // Register user
                    appDbContext.Users.Add(user);

                    var account = new Account()
                    {
                        AccountBalance = INITIAL_ACCOUNT_BALANCE,
                        AccountNumber = EBankHelper.GenerateAccountNumber(ACCOUNT_NUMBER_LENGTH),
                        AccountType = AccountTypeEnum.SAVINGS,
                        UserId = user.Id,
                        Currency = (int)CurrencyCode.GBP
                    };

                    // Create an account for the user
                    appDbContext.Accounts.Add(account);

                    // Save changes to the DB.
                    actualTransactions = await appDbContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("RegistrationFailed", "Failed to register the user");
                    return View(model);
                }

                if (actualTransactions == expectedTransactions)
                {
                    return RedirectToAction("RegistrationSuccessfull", "User");
                }
                else
                {
                    return RedirectToAction("Error");
                }
            }
            return View(model);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult RegistrationSuccessfull()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> UsernameExists(string username)
        {
            bool available = true;
            if (!string.IsNullOrEmpty(username))
            {
                using (appDbContext)
                {
                    available = await appDbContext.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower()) ? false : true;
                }
            }
            return Json(available);
        }

        [EBankAuthorized]
        public async Task<ActionResult> Dashboard()
        {
            // update session.
            await UpdateSession();
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> MyProfile(string userId)
        {
            await LogActivity(UserActivityEnum.MY_PROFILE);
            using (appDbContext)
            {
                var user = await appDbContext.Users.FindAsync(Convert.ToInt32(userId));

                var accounts = await appDbContext.Accounts.Where(x => x.UserId == user.Id).ToListAsync();

                // update session.
                await UpdateSession(userId);

                var result = _mapper.Map<UpdateProfileRequest>(user);

                return View(result);
            }
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> MyProfile(string userId, UpdateProfileRequest userModel)
        {
            await LogActivity(UserActivityEnum.MY_PROFILE);

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(userId))
                {

                    var id = int.Parse(userId);
                    if(id != GetCurrentUser.Id)
                    {
                        return View("Error");
                    }

                    using (appDbContext)
                    {
                        var user = await appDbContext.Users.FindAsync(Convert.ToInt32(userId));

                        if(user == null)
                        {
                            ClearCurrentSession();
                            CancelCurrentSession();
                        }

                        user.FirstName = userModel.FirstName;
                        user.LastName = userModel.LastName;
                        user.Password = userModel.Password;
                        user.PIN = userModel.PIN;
                        user.UserName = userModel.UserName;

                        if (user.RoleId == (int)UserRoleEnum.Admin)
                        {
                            user.RoleId = userModel.RoleId;
                        }

                        await appDbContext.SaveChangesAsync();

                        var accounts = await appDbContext.Accounts.Where(x => x.UserId == user.Id).ToListAsync();

                        // update session.
                        await UpdateSession(userId);

                        return RedirectToAction("MyProfile", "User", new { userId = user.Id });
                    }
                }
                else
                {
                    return View("Error");
                }
            }
            else
            {
                return View(userModel);
            }
        }

        [HttpGet]
        [EBankAuthorized]
        public async Task<ActionResult> GetUsers()
        {
            await LogActivity(UserActivityEnum.MY_ACCOUNTS);

            int start = Convert.ToInt32(HttpContext.Request["start"] ?? "1");
            int length = Convert.ToInt32(HttpContext.Request["length"] ?? "10");
            string searchValue = HttpContext.Request["search[value]"];
            string sortColumnName = HttpContext.Request["columns[" + Request["order[0][column]"] + "][name]"] ?? "FirstName";
            string sortDirection = HttpContext.Request["order[0][dir]"] ?? "asc";

            List<User> users;

            using (appDbContext)
            {
                users = await appDbContext.Users.AsNoTracking().ToListAsync();
                int totalrows = users.ToList().Count;
                if (!string.IsNullOrEmpty(searchValue))//filter
                {
                    users = users.Where(x => x.FirstName.ToLower().Contains(searchValue.ToLower())).ToList();
                }
                int totalrowsafterfiltering = users.ToList().Count;

                try
                {
                    if (!string.IsNullOrEmpty(sortColumnName) && !string.IsNullOrEmpty(sortDirection))
                        users = users.OrderBy<User>(sortColumnName + " " + sortDirection).ToList();
                }
                catch (Exception ex)
                {
                    throw;
                }

                //paging
                users = users.Skip(start).Take(length).ToList<User>();

                return Json(new { data = users, draw = Request["draw"], recordsTotal = totalrows, recordsFiltered = totalrowsafterfiltering }, JsonRequestBehavior.AllowGet);

            }
        }        

        [HttpGet]
        [EBankAuthorized]
        public async Task<ActionResult> ManageUsers()
        {
            await LogActivity(UserActivityEnum.MANAGE_USERS);
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> DeleteUser(int userId)
        {
            using (appDbContext)
            {
                var user = await appDbContext.Users.Where(x => x.Id == userId).FirstOrDefaultAsync();
                appDbContext.Users.Remove(user);
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