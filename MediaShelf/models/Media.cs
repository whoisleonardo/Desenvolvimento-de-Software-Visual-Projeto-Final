using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaShelf.Models
{
    public class Media
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title é obrigatório.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description é obrigatória.")]
        public string Description { get; set; } = string.Empty;

        public string? CoverImagePath { get; set; }

        [Required(ErrorMessage = "UserId é obrigatório.")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public List<Review> Reviews { get; set; } = new List<Review>();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}