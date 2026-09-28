using System.ComponentModel.DataAnnotations;

namespace GaragemVeiculos.Models;

public class Veiculo
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A placa é obrigatória.")]
    public string Placa { get; set; } = string.Empty;

    [Required(ErrorMessage = "A marca é obrigatória.")]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "O modelo é obrigatório.")]
    public string Modelo { get; set; } = string.Empty;

    [Range(1900, 2100, ErrorMessage = "Informe um ano entre 1900 e 2100.")]
    public int Ano { get; set; }

    public string? Cor { get; set; }
}
