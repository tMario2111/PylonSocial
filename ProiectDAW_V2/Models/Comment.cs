using System.ComponentModel.DataAnnotations;

namespace ProiectDAW_V2.Models;

public class Comment
{
    [Key] public int Id { get; set; }

    [Required] public int PostId { get; set; }

    [Required] public string AuthorId { get; set; }

    [Required]
    [MinLength(3)]
    [MaxLength(512)]
    public string Content { get; set; }

    [Required] public DateTime Date { get; set; }
    
    public virtual Post? Post { get; set; }
}