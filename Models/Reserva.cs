namespace GaragemVeiculos.Models;

public class Reserva
{
    public int Id { get; set; }
    public int VeiculoId { get; set; }
    public int PessoaId { get; set; }
    public DateOnly DataInicio { get; set; }
    public DateOnly DataFim { get; set; }
}
