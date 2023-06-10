using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notatez.Models;
using Notatez.Models.Attributes;
using Notatez.Models.Helpers;
using Notatez.Models.Services;
using Notatez.Models.ViewModels;
using SendGrid;
using SendGrid.Helpers.Mail;
using SendGrid.Helpers.Mail.Model;

namespace Notatez.Controllers;

public class AccountController : Controller
{
    private readonly AccountService _accountService;
    private readonly ISendGridClient _sendGridClient;
    private readonly IConfiguration _configuration;
    private readonly int sessionAccountId;
    private readonly string sessionName;

    public AccountController(AccountService accountService, ISendGridClient sendGridClient, IConfiguration configuration)
    {
        _accountService = accountService;
        sessionAccountId = _accountService.GetSessionAccountId();
        sessionName = _accountService.GetSessionUsername();
        _sendGridClient = sendGridClient;
        _configuration = configuration;
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

        // Check if the request comes from another URL
        string referrer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referrer))
        {
            AlertHelper.SetAlert(this, "You'll need to login first.", "warning");
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

            // After successful registration
            // Email confirmation

            //Set email items
            string fromEmail = _configuration.GetSection("EmailSettings").GetValue<string>("FromEmail") ?? "test@johnvicencio.com";
            string fromName = _configuration.GetSection("SendGridEmailSettings").GetValue<string>("FromName") ?? "Notatez Admin";

            var toName = ExtractNameFromEmail(account?.Email?.Trim() ?? "User");
            var toEmail = viewModel?.Email ?? "jvicencio+notatez@johnvicencio.com";


            var msg = new SendGridMessage()
            {
                From = new EmailAddress(fromEmail, fromName),
                Subject = "Welcome to Notatez - Thank you for Registering!",
                HtmlContent = EmailTemplateHelper.GetRegistrationEmailTemplate(toName, toEmail)
            };
            // Send email
            msg.AddTo(viewModel?.Email ?? "test@johnvicencio.com");
            var response = await _sendGridClient.SendEmailAsync(msg);
            bool sentEmail = response.IsSuccessStatusCode ? true : false;
            if (sentEmail)
            {
                ViewBag.SuccessMessage = "Email Send Successfully";
            }
            else
            {
                ViewBag.ErrorMessage = "Email Sending Failed";
            }

            return RedirectToAction("Index", "Account");
        }

        return View(viewModel);
    }

    private string ReplaceTemplatePlaceholders(string htmlContent, string templateData)
    {
        // Replace placeholders in the HTML template with the provided data
        // For example, if your template has a placeholder "{{Name}}", you can replace it with the actual data

        htmlContent = htmlContent.Replace("{{Name}}", templateData);

        // Add more placeholder replacements as needed

        return htmlContent;
    }


    private string ExtractNameFromEmail(string data)
    {
        return DataHelper.GetUsernameFromEmail(data);
    }
}