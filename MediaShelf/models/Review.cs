using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaShelf.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Range(1, 5, ErrorMessage = "Rating deve ser entre 1 e 5.")]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        [Required]
        public int MediaId { get; set; }

        [ForeignKey("MediaId")]
        public Media Media { get; set; } = null!;

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}