using System.ComponentModel.DataAnnotations;

namespace ProiectDAW_V2.Models;

public class Post
{
    [Key] public int Id { get; set; }

    public string? UserId { get; set; }

    public virtual ApplicationUser? User { get; set; }
    
    public enum PostType
    {
        Text,
        Image,
        Video
    }

    [Required] public PostType Type { get; set; }

    [Required (ErrorMessage = "Content is required")]
    public string Content { get; set; }

    [Required] public DateTime Date { get; set; }
    
    public virtual ICollection<Comment>? Comments { get; set; }

    public int? GroupId;
    public virtual Group? Group { get; set; }

}