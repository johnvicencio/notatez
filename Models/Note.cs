using System;
using System.ComponentModel.DataAnnotations;

namespace Notatez.Models;

public class Note
{
    public int Id { get; set; }
    public string? Slug { get; set; }

    [DataType(DataType.Text)]
    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, ErrorMessage = "Title must be at most 100 characters")]
    public string? Title { get; set; }

    [Required(ErrorMessage = "Content is required")]
    [DataType(DataType.MultilineText)]
    public string? Content { get; set; }

    [Display(Name = "Date Created")]
    [DataType(DataType.DateTime)]
    public DateTime DateCreated { get; set; }

    [Display(Name = "Date Modified")]
    [DataType(DataType.DateTime)]
    public DateTime DateModified { get; set; }

    public string? Description { get; set; }
    public string? ShortTitle { get; set; }

    public int? AccountId { get; set; }
    public Account? Account { get; set; }
}

