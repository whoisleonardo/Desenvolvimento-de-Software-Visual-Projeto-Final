using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaShelf.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Rating é obrigatório.")]
        [Range(0, 5, ErrorMessage = "Rating deve ser entre 0 e 5.")]
        public double Rating { get; set; }

        [Required(ErrorMessage = "Comment é obrigatório.")]
        [MaxLength(1000, ErrorMessage = "O comentário não pode exceder 1000 caracteres.")]
        public string Comment { get; set; } = string.Empty;

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