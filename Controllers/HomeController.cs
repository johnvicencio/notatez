using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Notatez.Models;
using Notatez.Models.Helpers;
using Notatez.Models.Services;
using Notatez.Models.ViewModels;

namespace Notatez.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AccountService _accountService;
    private readonly NoteService _noteService;
    private readonly int sessionAccountId;
    private readonly string sessionName;

    public HomeController(ILogger<HomeController> logger, AccountService accountService, NoteService noteService)
    {
        _logger = logger;
        _accountService = accountService;
        _noteService = noteService;
        sessionAccountId = _accountService.GetSessionAccountId();
        sessionName = _accountService.GetSessionUsername();
    }

    public async Task<IActionResult> Index(string searchQuery, string sortBy, string sortOrder, int page = 1, int pageSize = 8)
    {
        //Set session values
        ViewBag.SessionAccountId = sessionAccountId;
        ViewBag.SessionName = sessionName;
        IEnumerable<Note> allNotes = await _noteService.GetAllAsync();
        //int accountId = 0;
        //IEnumerable<Account> allAccounts = await _accountService.GetAccountNameByIdAsync(accountId);
        //allAccounts.Where(a => a.AccountId == accountId);
        //allAccounts.FirstOrDefault(a => a.Name);

        // Apply search query if provided
        if (!string.IsNullOrEmpty(searchQuery))
        {
            allNotes = allNotes.Where(n =>
                (n.Title != null && n.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                (n.Content != null && GetPlainText(n.Content, true).Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
            );
        }

        // Apply sorting if provided
        switch (sortBy)
        {
            case "title":
                allNotes = sortOrder == "desc" ? allNotes.OrderByDescending(n => n.Title) : allNotes.OrderBy(n => n.Title);
                break;
            case "content":
                allNotes = sortOrder == "desc" ? allNotes.OrderByDescending(n => GetPlainText(n?.Content ?? "", true)) : allNotes.OrderBy(n => GetPlainText(n?.Content ?? "", true));
                break;
            case "dateCreated":
                allNotes = sortOrder == "desc" ? allNotes.OrderByDescending(n => n.DateCreated) : allNotes.OrderBy(n => n.DateCreated);
                break;
            case "dateModified":
                allNotes = sortOrder == "desc" ? allNotes.OrderByDescending(n => n.DateModified) : allNotes.OrderBy(n => n.DateModified);
                break;
            default:
                allNotes = allNotes.OrderBy(n => n.Id);
                break;
        }

        // Apply pagination
        var notes = allNotes
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NoteViewModel
            {
                Id = n.Id,
                Title = n.Title,
                Slug = n.Slug,
                Content = n.Content,
                ShortTitle = n?.Title?.Length > 30 ? $"{ShortenText(n.Title, 30, true)}..." : n?.Title,
                Description = n?.Content != null && GetPlainText(n.Content, true).Length > 100 ? $"{ShortenText(n.Content, 100, true)}..." : n?.Content,
                DateCreated = n?.DateCreated ?? DateTime.MinValue,
                DateModified = n?.DateModified ?? DateTime.MinValue,
                Account = n?.Account,
                AccountId = n?.AccountId,
                Author = n.AccountId > 0 ? n?.Account?.Name : "Unknown"
            })
            .ToList();

        // Create the Page model and pass it to the view
        var pageModel = new Page<NoteViewModel>
        {
            Items = notes,
            TotalItems = allNotes.Count(), // Get the total count after filtering
            CurrentPage = page,
            PageSize = pageSize,
            SearchQuery = searchQuery,
            SortBy = sortBy,
            SortOrder = sortOrder
        };

        return View(pageModel);
    }

    private string GetPlainText(string data, bool decode)
    {
        return DataHelper.GetPlainText(data, decode);
    }
    private string ShortenText(string data, int length, bool decode = false)
    {
        return DataHelper.GetShortText(data, length, decode);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

