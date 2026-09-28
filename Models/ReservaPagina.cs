namespace GaragemVeiculos.Models;

public class ReservaPagina
{
    public List<ReservaLinha> Reservas { get; set; } = new();
    public List<VeiculoDoDia> Veiculos { get; set; } = new();
    public List<Pessoa> Pessoas { get; set; } = new();
    public Reserva Formulario { get; set; } = new();
    public int? FiltroPessoaId { get; set; }
    public DateOnly? FiltroDe { get; set; }
    public DateOnly? FiltroAte { get; set; }
}

public class ReservaLinha
{
    public int Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Pessoa { get; set; } = string.Empty;
    public DateOnly DataInicio { get; set; }
    public DateOnly DataFim { get; set; }
}

public class VeiculoDoDia
{
    public int Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool Reservado { get; set; }
}
