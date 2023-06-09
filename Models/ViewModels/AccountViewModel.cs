using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Notatez.Models.ViewModels;

public class AccountViewModel
{
    public int AccountId { get; set; }

    [Display(Name = "Email")]
    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Email address is required")]
    public string? Email { get; set; }

    [Display(Name = "Password")]
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "The password must be between 8 and 20 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "The password must contain at least one lowercase letter, one uppercase letter, one numeric digit, and one special character.")]
    public string? Password { get; set; }

    [Display(Name = "Confirm Password")]
    [Required(ErrorMessage = "Confirm Password is required")]
    [DataType(DataType.Password)]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "The password must be between 8 and 20 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "The password must contain at least one lowercase letter, one uppercase letter, one numeric digit, and one special character.")]
    public string? PasswordConfirm { get; set; }

    [DataType(DataType.Text)]
    [Display(Name = "Name")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 50 characters")]
    public string? Name { get; set; }

    [Display(Name = "Roles")]
    public string? Role { get; set; }

    [Display(Name = "Date Created")]
    [DataType(DataType.DateTime)]
    public DateTime DateCreated { get; set; } = DateTime.Now;

    public string? AlertMessage { get; set; }
    public string? AlertType { get; set; }

    public List<Note>? Notes { get; set; }
}