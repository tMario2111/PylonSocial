using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using ProiectDAW_V2.Data;

namespace ProiectDAW_V2.Models;

public class Profile
{
    [Key] public int Id { get; set; }

    public string? UserId { get; set; }

    public virtual ApplicationUser? User { get; set; }

    [MinLength(2, ErrorMessage = "First name too short")]
    [MaxLength(50, ErrorMessage = "First name too long")]
    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; }

    [MinLength(2, ErrorMessage = "Last name too short")]
    [MaxLength(50, ErrorMessage = "Last name too long")]
    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; }

    [MinLength(3, ErrorMessage = "Description too short")]
    [MaxLength(100, ErrorMessage = "Description too long")]
    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; }

    public string? ProfilePicture { get; set; }

    public enum VisibilityType
    {
        Public,
        Private
    }

    [Required] public VisibilityType Visibility { get; set; }

    [NotMapped] public bool DeleteProfilePicture { get; set; }
}