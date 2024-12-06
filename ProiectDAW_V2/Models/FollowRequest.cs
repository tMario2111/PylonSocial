using System.ComponentModel.DataAnnotations;

namespace ProiectDAW_V2.Models;

public class FollowRequest
{
    [Key] public int Id { get; set; }
    
    public string? SenderId { get; set; }
    public virtual ApplicationUser? Sender { get; set; }
    
    public string? ReceiverId { get; set; }
    public virtual ApplicationUser? Receiver { get; set; }
    
    [Required]
    DateTime Date { get; set; }
}