using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProiectDAW_V2.Models;

public class FollowRequest
{
    [Key, Column(Order = 0)]
    public string SenderId { get; set; }
    public virtual ApplicationUser? Sender { get; set; }
    
    
    [Key, Column(Order = 1)]
    public string ReceiverId { get; set; }
    public virtual ApplicationUser? Receiver { get; set; }
    
    
    [Required]
    public DateTime? Date { get; set; }
}