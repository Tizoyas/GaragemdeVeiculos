using System.Text.Encodings.Web;
using System.Text.Json;
using GaragemVeiculos.Models;

namespace GaragemVeiculos.Repositories;

public class ReservaRepository : IReservaRepository
{
    private static readonly object Trava = new();
    private readonly string _caminho;
    private readonly JsonSerializerOptions _json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public ReservaRepository(IWebHostEnvironment ambiente)
    {
        var pasta = Path.Combine(ambiente.ContentRootPath, "Data");
        Directory.CreateDirectory(pasta);
        _caminho = Path.Combine(pasta, "reservas.json");
    }

    public List<Reserva> ObterTodas()
    {
        lock (Trava)
        {
            return Ler();
        }
    }

    public Reserva? ObterPorId(int id)
    {
        return ObterTodas().FirstOrDefault(r => r.Id == id);
    }

    public void Adicionar(Reserva reserva)
    {
        lock (Trava)
        {
            var lista = Ler();
            reserva.Id = lista.Count == 0 ? 1 : lista.Max(r => r.Id) + 1;
            lista.Add(reserva);
            Gravar(lista);
        }
    }

    public void Atualizar(Reserva reserva)
    {
        lock (Trava)
        {
            var lista = Ler();
            var indice = lista.FindIndex(r => r.Id == reserva.Id);
            if (indice < 0)
                return;

            lista[indice] = reserva;
            Gravar(lista);
        }
    }

    public void Remover(int id)
    {
        lock (Trava)
        {
            var lista = Ler();
            lista.RemoveAll(r => r.Id == id);
            Gravar(lista);
        }
    }

    public bool ExisteConflito(int veiculoId, DateOnly inicio, DateOnly fim, int idIgnorado)
    {
        return ObterTodas().Any(r =>
            r.Id != idIgnorado &&
            r.VeiculoId == veiculoId &&
            inicio <= r.DataFim &&
            fim >= r.DataInicio);
    }

    public bool PossuiReservaAtivaOuFuturaDePessoa(int pessoaId)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        return ObterTodas().Any(r => r.PessoaId == pessoaId && r.DataFim >= hoje);
    }

    public bool PossuiReservaAtivaOuFuturaDeVeiculo(int veiculoId)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        return ObterTodas().Any(r => r.VeiculoId == veiculoId && r.DataFim >= hoje);
    }

    private List<Reserva> Ler()
    {
        if (!File.Exists(_caminho))
        {
            File.WriteAllText(_caminho, "[]");
            return new List<Reserva>();
        }

        var texto = File.ReadAllText(_caminho);
        if (string.IsNullOrWhiteSpace(texto))
            return new List<Reserva>();

        return JsonSerializer.Deserialize<List<Reserva>>(texto, _json) ?? new List<Reserva>();
    }

    private void Gravar(List<Reserva> reservas)
    {
        File.WriteAllText(_caminho, JsonSerializer.Serialize(reservas, _json));
    }
}
