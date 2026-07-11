using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    // Name: allow letters (including accents), spaces, hyphens and apostrophes. Min 2, max 80.
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "O nome deve ter entre {2} e {1} caracteres.")]
    [RegularExpression(@"^[A-Za-zÀ-ÖØ-öø-ÿ\s'\-]+$", ErrorMessage = "O nome contém caracteres inválidos.")]
    public string Name { get; set; }

    // Email: keep EmailAddress and add a stricter regex to reject obvious invalid formats.
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um endereço de e-mail válido.")]
    [RegularExpression(@"^(?=.{5,254}$)[A-Za-z0-9]+[A-Za-z0-9._%+-]*@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$", ErrorMessage = "Formato de e-mail inválido.")]
    public string Email { get; set; }

    // Password: require between 8 and 100 chars, at least one uppercase, one digit and one special char.
    [Required(ErrorMessage = "A senha é obrigatória.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre {2} e {1} caracteres.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,100}$", ErrorMessage = "A senha deve conter ao menos uma letra maiúscula, um número e um caractere especial.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "A confirmação de senha é obrigatória.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "As senhas não coincidem.")]
    public string ConfirmPassword { get; set; }
}
