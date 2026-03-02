using System.ComponentModel.DataAnnotations;

namespace Prompteer.Web.Models;

public class SetupViewModel
{
    [Required(ErrorMessage = "Nome obrigatório")]
    [MaxLength(256)]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha obrigatória")]
    [MinLength(8, ErrorMessage = "Mínimo 8 caracteres")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação obrigatória")]
    [Compare(nameof(Password), ErrorMessage = "As senhas não coincidem")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
