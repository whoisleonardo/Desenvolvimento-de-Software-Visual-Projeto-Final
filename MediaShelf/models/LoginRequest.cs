using System.ComponentModel.DataAnnotations;

namespace MediaShelf.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password é obrigatória.")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}