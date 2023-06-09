using Microsoft.AspNetCore.Mvc;
using Notatez.Models;
using Notatez.Models.Attributes;
using Notatez.Models.Helpers;
using Notatez.Models.Services;
using Notatez.Models.ViewModels;

namespace Notatez.Controllers;

[AuthorizationNeeded]
public class NoteController : Controller
{
    private readonly NoteService _noteService;
    private readonly AccountService _accountService;
    private readonly int sessionAccountId;
    private readonly string sessionName;

    public NoteController(NoteService noteService, AccountService accountService)
    {
        _noteService = noteService;
        _accountService = accountService;
        sessionAccountId = _accountService.GetSessionAccountId();
        sessionName = _accountService.GetSessionUsername();
    }

    public async Task<IActionResult> Index(string searchQuery, string sortBy, string sortOrder, int page = 1, int pageSize = 9)
    {
        //Set session values
        ViewBag.SessionAccountId = sessionAccountId;
        ViewBag.SessionName = sessionName;

        // Get all notes
        IEnumerable<Note> allNotes = await _noteService.GetAllAsync(); 
        //// Get session values to see if user is logged in
        //int accountId = await _accountService.GetSessionAccountIdAsync();
        //string sessionName = await _accountService.GetSessionUsernameAsync();
        //ViewBag.SessionId = accountId;
        //ViewBag.SessionName = sessionName;

        //// Get all notes from accountId
        //allNotes = allNotes.Where(n => n.AccountId == accountId);

        // Apply search query if provided
        if (!string.IsNullOrEmpty(searchQuery))
        {
            allNotes = allNotes.Where(n =>
                (n.Title != null && n.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                (n.Content != null && n.Content.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
            );
        }

        // Apply sorting if provided
        switch (sortBy)
        {
            case "title":
                allNotes = sortOrder == "desc" ? allNotes.OrderByDescending(n => n.Title) : allNotes.OrderBy(n => n.Title);
                break;
            case "content":
                allNotes = sortOrder == "desc" ? allNotes.OrderByDescending(n => n.Content) : allNotes.OrderBy(n => n.Content);
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
                Description = n?.Content != null && GetPlainText(n.Content, true).Length > 100
            ? $"{ShortenText(n.Content, 100, true)}..." : n?.Content,
                DateCreated = n?.DateCreated ?? DateTime.MinValue,
                DateModified = n?.DateModified ?? DateTime.MinValue,
                AccountId = n?.AccountId,
                Author = "Test"
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

    public async Task<IActionResult> Details(int id)
    {
        //Set session values
        ViewBag.SessionAccountId = sessionAccountId;
        ViewBag.SessionName = sessionName;
        // Get session values to see if user is logged in
        //int accountId = await _accountService.GetSessionAccountIdAsync();
        //string sessionName = await _accountService.GetSessionUsernameAsync();
        //ViewBag.SessionId = accountId;
        //ViewBag.SessionName = sessionName;

        var note = await _noteService.GetByIdAsync(id);

        if (note == null)
        {
            return NotFound();
        }

        return View(note);
    }

    public async Task<IActionResult> Create()
    {
        //Set session values
        ViewBag.SessionAccountId = sessionAccountId;
        ViewBag.SessionName = sessionName;

        var accountId = await _accountService.GetSessionAccountIdAsync();
        var name = await _accountService.GetAccountNameByIdAsync(accountId);
        var viewModel = new NoteViewModel
        {
            AccountId = accountId,
            Author = name
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NoteViewModel viewModel)
    {
        //Set session values
        ViewBag.SessionAccountId = sessionAccountId;
        ViewBag.SessionName = sessionName;

        // Get all notes from the database.
        // to check if title entered is already existing
        var existingNotes = await _noteService.GetAllAsync();
        if (existingNotes != null)
        {
            // Comparison of inputted title and existing title if any
            // Also, this makes sure that there's no possible null references on any scenarios
            bool isTitleAvailable = existingNotes.Any(n =>
            {
                // Check if the note is not null.
                if (n != null)
                {
                    // Check if the note's title is not null.
                    if (n.Title != null)
                    {
                        // Check if the note's title is equal to the view model's title, ignoring case.
                        return n.Title.Equals(viewModel.Title, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        // The note's title is null, so return false.
                        return false;
                    }
                }
                else
                {
                    // The note is null, so return false.
                    return false;
                }
            });

            if (isTitleAvailable)
            {
                ModelState.AddModelError("Title", "Title already exists");
                return View(viewModel);
            }
        }

        if (ModelState.IsValid)
        {
            var note = new Note
            {
                Title = viewModel.Title,
                Content = viewModel.Content,
                AccountId = viewModel.AccountId
            };

        await _noteService.CreateAsync(note);
        return RedirectToAction("Index", "Note");
        }

        return View(viewModel);
    }

    public async Task<IActionResult> Update(int id)
    {
        //Set session values
        ViewBag.SessionAccountId = sessionAccountId;
        ViewBag.SessionName = sessionName;

        var note = await _noteService.GetByIdAsync(id);
        if (note == null)
        {
            return NotFound();
        }

        var viewModel = new NoteViewModel
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            AccountId = note.AccountId,
            Account = note.Account != null
                ? new Account
                {
                    AccountId = note.Account.AccountId,
                    Name = note.Account.Name
                }
                : null
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, NoteViewModel viewModel)
    {
        // Set session values
        ViewBag.SessionAccountId = sessionAccountId;
        ViewBag.SessionName = sessionName;

        if (id != viewModel.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var note = new Note
            {
                Id = viewModel.Id,
                Title = viewModel.Title,
                Content = viewModel.Content,
                AccountId = viewModel.AccountId,
                Account = new Account
                {
                    AccountId = viewModel.AccountId ?? 0,
                    Name = viewModel.Account?.Name ?? string.Empty
                }
            };

            var result = await _noteService.UpdateAsync(note);
            if (!result)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        return View(viewModel);
    }


    public async Task<IActionResult> Delete(int id)
    {

        //Set session values
        ViewBag.SessionAccountId = sessionAccountId;
        ViewBag.SessionName = sessionName;

        var note = await _noteService.GetByIdAsync(id);
        if (note == null)
        {
            return NotFound();
        }

        var viewModel = new NoteViewModel
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            DateCreated = note.DateCreated,
            DateModified = note.DateModified
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, NoteViewModel viewModel)
    {

        //Set session values
        ViewBag.SessionAccountId = sessionAccountId;
        ViewBag.SessionName = sessionName;

        var result = await _noteService.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    private string GetPlainText(string data, bool decode)
    {
        return DataHelper.GetPlainText(data, decode);
    }
    private string ShortenText(string data, int length, bool decode = false)
    {
        return DataHelper.GetShortText(data, length, decode);
    }
}


