using System.ComponentModel.DataAnnotations;

namespace Prompteer.Web.Models;

public class LocalLoginViewModel
{
    [Required(ErrorMessage = "E-mail obrigatório")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha obrigatória")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
