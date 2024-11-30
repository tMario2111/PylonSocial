using System.ComponentModel.DataAnnotations;

namespace ProiectDAW_V2.Models;

public class Profile
{
    [Key] public int Id { get; set; }

    [Required] public int UserId { get; set; }

    [Required] public string FirstName { get; set; }

    [Required] public string LastName { get; set; }

    [Required] public string Description { get; set; }

    [Required] public string ProfilePicture { get; set; }

    public enum VisibilityType
    {
        Public,
        Private
    }

    [Required] public VisibilityType Visibility { get; set; }
}