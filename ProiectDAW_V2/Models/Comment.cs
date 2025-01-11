using System.ComponentModel.DataAnnotations;

namespace ProiectDAW_V2.Models;

public class Comment
{
    [Key] public int Id { get; set; }

    [Required] public int PostId { get; set; }

    public string? AuthorId { get; set; }
    
    public virtual ApplicationUser? Author { get; set; }

    [Required (ErrorMessage = "Content is required")]
    [MinLength(3, ErrorMessage = "Content must be at least 3 characters long")]
    [MaxLength(512, ErrorMessage = "Content must be at most 512 characters long")]
    public string Content { get; set; }

    [Required] public DateTime Date { get; set; }
    
    public virtual Post? Post { get; set; }
}