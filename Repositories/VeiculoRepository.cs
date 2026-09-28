using System.Text.Encodings.Web;
using System.Text.Json;
using GaragemVeiculos.Models;

namespace GaragemVeiculos.Repositories;

public class VeiculoRepository : IVeiculoRepository
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

    public VeiculoRepository(IWebHostEnvironment ambiente)
    {
        var pasta = Path.Combine(ambiente.ContentRootPath, "Data");
        Directory.CreateDirectory(pasta);
        _caminho = Path.Combine(pasta, "veiculos.json");
    }

    public List<Veiculo> ObterTodas()
    {
        lock (Trava)
        {
            return Ler();
        }
    }

    public Veiculo? ObterPorId(int id)
    {
        return ObterTodas().FirstOrDefault(v => v.Id == id);
    }

    public void Adicionar(Veiculo veiculo)
    {
        lock (Trava)
        {
            var lista = Ler();
            Preparar(veiculo);
            veiculo.Id = lista.Count == 0 ? 1 : lista.Max(v => v.Id) + 1;
            lista.Add(veiculo);
            Gravar(lista);
        }
    }

    public void Atualizar(Veiculo veiculo)
    {
        lock (Trava)
        {
            var lista = Ler();
            var indice = lista.FindIndex(v => v.Id == veiculo.Id);
            if (indice < 0)
                return;

            Preparar(veiculo);
            lista[indice] = veiculo;
            Gravar(lista);
        }
    }

    public void Remover(int id)
    {
        lock (Trava)
        {
            var lista = Ler();
            lista.RemoveAll(v => v.Id == id);
            Gravar(lista);
        }
    }

    public bool ExistePlaca(string placa, int idIgnorado)
    {
        var normalizada = NormalizarPlaca(placa);
        if (normalizada.Length == 0)
            return false;

        return ObterTodas().Any(v => v.Id != idIgnorado && NormalizarPlaca(v.Placa) == normalizada);
    }

    private List<Veiculo> Ler()
    {
        if (!File.Exists(_caminho))
        {
            File.WriteAllText(_caminho, "[]");
            return new List<Veiculo>();
        }

        var texto = File.ReadAllText(_caminho);
        if (string.IsNullOrWhiteSpace(texto))
            return new List<Veiculo>();

        return JsonSerializer.Deserialize<List<Veiculo>>(texto, _json) ?? new List<Veiculo>();
    }

    private void Gravar(List<Veiculo> veiculos)
    {
        File.WriteAllText(_caminho, JsonSerializer.Serialize(veiculos, _json));
    }

    private static void Preparar(Veiculo veiculo)
    {
        veiculo.Placa = (veiculo.Placa ?? string.Empty).Trim().ToUpperInvariant();
        veiculo.Marca = (veiculo.Marca ?? string.Empty).Trim();
        veiculo.Modelo = (veiculo.Modelo ?? string.Empty).Trim();
        veiculo.Cor = string.IsNullOrWhiteSpace(veiculo.Cor) ? null : veiculo.Cor.Trim();
    }

    private static string NormalizarPlaca(string? placa) =>
        new string((placa ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
}
