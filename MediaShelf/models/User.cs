using System;
using System.ComponentModel.DataAnnotations;

namespace MediaShelf.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name é obrigatório.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        public string Email { get; set; } = string.Empty;

        public string? Password { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
