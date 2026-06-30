using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuAnTotNghiep.Models;

[Table("chat_messages")]
public class ChatMessage
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Sender")]
    public int SenderId { get; set; }

    [Required]
    [ForeignKey("Receiver")]
    public int ReceiverId { get; set; }

    [Required]
    public string MessageText { get; set; } = null!;

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User Sender { get; set; } = null!;
    public virtual User Receiver { get; set; } = null!;
}
