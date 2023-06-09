using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notatez.Models;
using Notatez.Models.Attributes;
using Notatez.Models.Helpers;
using Notatez.Models.Services;
using Notatez.Models.ViewModels;

namespace Notatez.Controllers;

public class AccountController : Controller
{
    private readonly AccountService _accountService;
    private readonly int sessionAccountId;
    private readonly string sessionName;

    public AccountController(AccountService accountService)
    {
        _accountService = accountService;
        sessionAccountId = _accountService.GetSessionAccountId();
        sessionName = _accountService.GetSessionUsername();
    }

    [AuthorizationNeeded]
    [HttpGet]
    public IActionResult Index()
    {
        //var accounts = await _accountService.GetAllAsync();
        return RedirectToAction("Index", "Home");
    }


    [HttpGet]
    public async Task<IActionResult> Login()
    {
        //Set session values
        
        ViewBag.SessionAccountId = sessionAccountId;
        ViewBag.SessionName = sessionName;

        string referrer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referrer))
        {
            AlertHelper.SetAlert(this, "You may need to login to access this.", "warning");
        }
        else
        {
            var sessionId = await _accountService.GetSessionAccountIdAsync();
            if (sessionId > 0)
            {
                return RedirectToAction("Index", "Home");
            }
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(Account account)
    {
        //Set session values
        ViewBag.SessionAccountId = sessionAccountId;
        ViewBag.SessionName = sessionName;

        if (ModelState.IsValid)
        {
            if (!string.IsNullOrEmpty(account.Email) && !string.IsNullOrEmpty(account.Password))
            {
                int accountId = await _accountService.HandleAuthenticateAsync(account.Email, account.Password, HttpContext);

                if (accountId > 0)
                {
                    // Authentication successful, redirect to des   ired page
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Authentication failed, display error message
                    ModelState.AddModelError(string.Empty, "Invalid email or password");
                }

            }
            else
            {
                // Invalid credentials, display error message
                ModelState.AddModelError(string.Empty, "Invalid email or password");
            }
        }


        // Model state is invalid or authentication failed, redisplay the login view with validation errors
        return View(account);
    }
    //public async Task<IActionResult> Login(AccountViewModel viewModel)
    //{
    //    var sessionId = await _accountService.GetSessionAccountIdAsync();

    //    if (ModelState.IsValid)
    //    {
    //        if (!string.IsNullOrEmpty(viewModel.Username) && !string.IsNullOrEmpty(viewModel.Password))
    //        {
    //            int accountId = await _accountService.HandleAuthenticateAsync(viewModel.Username, viewModel.Password, HttpContext);

    //            if (accountId > 0)
    //            {
    //                // User is authenticated, perform necessary actions
    //                return RedirectToAction("Index", "Home");
    //            }
    //            else
    //            {
    //                // Authentication failed, display error message
    //                ModelState.AddModelError(string.Empty, "Invalid username or password");
    //            }
    //        }
    //        else
    //        {
    //            // Invalid credentials, display error message
    //            ModelState.AddModelError(string.Empty, "Invalid username or password");
    //        }
    //    }

    //    // Model state is invalid or authentication failed, redisplay the login view with validation errors
    //    return View(viewModel);
    //}

    public async Task<IActionResult> Logout()
    {
        await _accountService.HandleLogoutAsync();

        // Redirect to the desired page after logout
        return RedirectToAction("Index", "Home");
    }


    public IActionResult Register()
    {
        //Set session values
        ViewBag.SessionAccountId = sessionAccountId;
        ViewBag.SessionName = sessionName;

        var viewModel = new AccountViewModel();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(AccountViewModel viewModel)
    {
        //Set session values
        ViewBag.SessionAccountId = sessionAccountId;
        ViewBag.SessionName = sessionName;

        if (ModelState.IsValid)
        {
            // Check if account exists
            var existingAccounts = await _accountService.GetAllAsync();
            if (existingAccounts != null)
            {
                bool isUsernameAvailable = existingAccounts.All(a => a.Email != viewModel.Email);
                if (!isUsernameAvailable)
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    return View(viewModel);
                }
            }
            var passwordEntered = viewModel?.Password?.Trim();
            var passwordConfirm = viewModel?.PasswordConfirm?.Trim();
            if (!string.IsNullOrWhiteSpace(passwordEntered) && !string.IsNullOrWhiteSpace(passwordConfirm))
            {
                bool isPasswordVerified = passwordEntered == passwordConfirm;
                if (!isPasswordVerified)
                {
                    ModelState.AddModelError("Password", "Password doesn't match.");
                    return View(viewModel);
                }
            }

            var account = new Account();
            account.Email = viewModel?.Email;
            account.Password = viewModel?.Password;
            await _accountService.CreateAsync(account);
            return RedirectToAction("Index", "Account");
        }

        return View(viewModel);
    }

    //public IActionResult Register()
    //{

    //    return View();
    //}

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Register(Account account)
    //{
    //    //Check if account exists
    //    var existingAccounts = await _accountService.GetAllAsync();
    //    if (existingAccounts != null)
    //    {
    //        bool isUsernameAvailable = existingAccounts.All(a => a.Email != account.Email);
    //        if (!isUsernameAvailable)
    //        {
    //            ModelState.AddModelError("Email", "Email already exists");
    //            return View(account);
    //        }
    //    }

    //    if (ModelState.IsValid)
    //    {
    //        await _accountService.CreateAsync(account);
    //        return RedirectToAction("Index", "Account");
    //    }

    //    return View(account);
    //}
}