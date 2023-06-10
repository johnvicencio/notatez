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
    private Dictionary<string, int> slugToIdMap = new Dictionary<string, int>();


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

        // Set account
        int accountId = sessionAccountId; 

        // Get notes for the specific AccountId
        IEnumerable<Note> allNotes = await _noteService.GetByAccountIdAsync(accountId);

        // Apply search query if provided
        if (!string.IsNullOrEmpty(searchQuery))
        {
            allNotes = allNotes.Where(n =>
                (n.Title != null && n.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                (n.Content != null && n.Content.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
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


    public async Task<IActionResult> Details(int id)
    {
        // Set session values
        ViewBag.SessionAccountId = sessionAccountId;
        ViewBag.SessionName = sessionName;

        // Get note by id
        var note = await _noteService.GetByIdAsync(id);
        //var note = await _noteService.GetBySlugAsync(slug);

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
                    Name = viewModel.Account?.Name ?? sessionName
                }
            };

            var result = await _noteService.UpdateAsync(note);
            if (!result)
            {
                return NotFound();
            }

            return RedirectToAction("Details", "Note", new { id = viewModel.Id });
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
            //case "author":
            //    notes = sortOrder == "desc" ? notes.OrderByDescending(n => GetPlainText(n?.Account?.Name ?? "", true)) : notes.OrderBy(n => GetPlainText(n?.Account?.Name ?? "", true));
            //    break;
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


    private int? GetIdFromSlug(string slug)
    {
        if (slugToIdMap.TryGetValue(slug, out int id))
        {
            return id;
        }
        return null; // Slug not found
    }
}


