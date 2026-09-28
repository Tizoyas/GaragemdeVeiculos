using System.ComponentModel.DataAnnotations;

namespace GaragemVeiculos.Models;

public class Pessoa
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CPF é obrigatório.")]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = string.Empty;

    public string? Telefone { get; set; }
}
