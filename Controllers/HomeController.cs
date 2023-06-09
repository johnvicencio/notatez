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

        //Get all notes
        IEnumerable<Note> allNotes = await _noteService.GetAllAsync();

        // Apply search query if provided
        if (!string.IsNullOrEmpty(searchQuery))
        {
            allNotes = allNotes.Where(n =>
                (n.Title != null && n.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                (n.Content != null && GetPlainText(n.Content, true).Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                (n?.Account?.Name != null && GetPlainText(n.Account.Name, true).Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
            );
        }

        // Apply sorting if provided
        allNotes = ApplySorting(allNotes, sortBy, sortOrder);

        // Pagination
        var paginatedNotes = allNotes.Skip((page - 1) * pageSize).Take(pageSize);

        // View model
        var noteViewModels = paginatedNotes.Select(n => CreateNoteViewModel(n));

        // Create the Page model and pass it to the view
        var pageModel = new Page<NoteViewModel>
        {
            Items = noteViewModels.ToList(),
            TotalItems = allNotes.Count(),
            CurrentPage = page,
            PageSize = pageSize,
            SearchQuery = searchQuery,
            SortBy = sortBy,
            SortOrder = sortOrder
        };

        return View(pageModel);
    }

    private IEnumerable<Note> ApplySorting(IEnumerable<Note> notes, string sortBy, string sortOrder)
    {
        switch (sortBy)
        {
            case "title":
                notes = sortOrder == "desc" ? notes.OrderByDescending(n => n.Title) : notes.OrderBy(n => n.Title);
                break;
            case "content":
                notes = sortOrder == "desc" ? notes.OrderByDescending(n => GetPlainText(n?.Content ?? "", true)) : notes.OrderBy(n => GetPlainText(n?.Content ?? "", true));
                break;
            case "author":
                notes = sortOrder == "desc" ? notes.OrderByDescending(n => GetPlainText(n?.Account?.Name ?? "", true)) : notes.OrderBy(n => GetPlainText(n?.Account?.Name ?? "", true));
                break;
            case "dateCreated":
                notes = sortOrder == "desc" ? notes.OrderByDescending(n => n.DateCreated) : notes.OrderBy(n => n.DateCreated);
                break;
            case "dateModified":
                notes = sortOrder == "desc" ? notes.OrderByDescending(n => n.DateModified) : notes.OrderBy(n => n.DateModified);
                break;
            default:
                notes = notes.OrderBy(n => n.Id);
                break;
        }

        return notes;
    }

    private NoteViewModel CreateNoteViewModel(Note note)
    {
        return new NoteViewModel
        {
            Id = note.Id,
            Title = note.Title,
            Slug = note.Slug,
            Content = note.Content,
            ShortTitle = note.Title?.Length > 30 ? $"{ShortenText(note.Title, 30, true)}..." : note.Title,
            Description = note.Content != null && GetPlainText(note.Content, true).Length > 100 ? $"{ShortenText(note.Content, 100, true)}..." : note.Content,
            DateCreated = note.DateCreated,
            DateModified = note.DateModified,
            Account = note.Account,
            AccountId = note.AccountId,
            Author = note.AccountId > 0 ? note.Account?.Name : "Unknown"
        };
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

