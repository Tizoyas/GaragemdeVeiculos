using GaragemVeiculos.Models;
using GaragemVeiculos.Repositories;

namespace GaragemVeiculos.Services;

public class ReservaService
{
    private readonly IReservaRepository _reservas;
    private readonly IPessoaRepository _pessoas;
    private readonly IVeiculoRepository _veiculos;

    public ReservaService(
        IReservaRepository reservas,
        IPessoaRepository pessoas,
        IVeiculoRepository veiculos)
    {
        _reservas = reservas;
        _pessoas = pessoas;
        _veiculos = veiculos;
    }

    public IReadOnlyList<(string Campo, string Mensagem)> Validar(Reserva reserva)
    {
        var erros = new List<(string Campo, string Mensagem)>();

        if (reserva.DataInicio == default)
            erros.Add((nameof(reserva.DataInicio), "Informe a data de início."));

        if (reserva.DataFim == default)
            erros.Add((nameof(reserva.DataFim), "Informe a data de fim."));

        if (reserva.DataInicio != default && reserva.DataFim != default && reserva.DataFim < reserva.DataInicio)
            erros.Add((nameof(reserva.DataFim), "A data final não pode ser anterior à data inicial."));

        if (_pessoas.ObterPorId(reserva.PessoaId) is null)
            erros.Add((nameof(reserva.PessoaId), "Selecione uma pessoa cadastrada."));

        if (_veiculos.ObterPorId(reserva.VeiculoId) is null)
            erros.Add((nameof(reserva.VeiculoId), "Selecione um veículo cadastrado."));

        var periodoValido = reserva.DataInicio != default
            && reserva.DataFim != default
            && reserva.DataFim >= reserva.DataInicio;

        if (periodoValido && _reservas.ExisteConflito(reserva.VeiculoId, reserva.DataInicio, reserva.DataFim, reserva.Id))
            erros.Add((string.Empty, "Este veículo já possui reserva com período sobreposto. Escolha outras datas."));

        return erros;
    }
}
