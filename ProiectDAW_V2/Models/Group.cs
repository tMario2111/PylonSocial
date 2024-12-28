using System.ComponentModel.DataAnnotations;

namespace ProiectDAW_V2.Models;

public class Group
{
    [Key] public int Id { get; set; }

    public string? ModeratorId { get; set; }

    [Required]
    [MinLength(2, ErrorMessage = "Group name is too short")]
    [MaxLength(50, ErrorMessage = "Group name is too long")]
    public string Name { get; set; }

    [Required]
    [MinLength(3, ErrorMessage = "Description is too short")]
    [MaxLength(100, ErrorMessage = "Description is too long")]
    public string Description { get; set; }

    public virtual ICollection<UserGroup>? UserGroups { get; set; }
}