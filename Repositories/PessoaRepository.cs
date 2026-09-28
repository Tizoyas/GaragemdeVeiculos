using System.Text.Encodings.Web;
using System.Text.Json;
using GaragemVeiculos.Models;

namespace GaragemVeiculos.Repositories;

public class PessoaRepository : IPessoaRepository
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

    public PessoaRepository(IWebHostEnvironment ambiente)
    {
        var pasta = Path.Combine(ambiente.ContentRootPath, "Data");
        Directory.CreateDirectory(pasta);
        _caminho = Path.Combine(pasta, "pessoas.json");
    }

    public List<Pessoa> ObterTodas()
    {
        lock (Trava)
        {
            return Ler();
        }
    }

    public Pessoa? ObterPorId(int id)
    {
        return ObterTodas().FirstOrDefault(p => p.Id == id);
    }

    public void Adicionar(Pessoa pessoa)
    {
        lock (Trava)
        {
            var lista = Ler();
            Preparar(pessoa);
            pessoa.Id = lista.Count == 0 ? 1 : lista.Max(p => p.Id) + 1;
            lista.Add(pessoa);
            Gravar(lista);
        }
    }

    public void Atualizar(Pessoa pessoa)
    {
        lock (Trava)
        {
            var lista = Ler();
            var indice = lista.FindIndex(p => p.Id == pessoa.Id);
            if (indice < 0)
                return;

            Preparar(pessoa);
            lista[indice] = pessoa;
            Gravar(lista);
        }
    }

    public void Remover(int id)
    {
        lock (Trava)
        {
            var lista = Ler();
            lista.RemoveAll(p => p.Id == id);
            Gravar(lista);
        }
    }

    public bool ExisteCpf(string cpf, int idIgnorado)
    {
        var digitos = SomenteDigitos(cpf);
        if (digitos.Length == 0)
            return false;

        return ObterTodas().Any(p => p.Id != idIgnorado && SomenteDigitos(p.Cpf) == digitos);
    }

    private List<Pessoa> Ler()
    {
        if (!File.Exists(_caminho))
        {
            File.WriteAllText(_caminho, "[]");
            return new List<Pessoa>();
        }

        var texto = File.ReadAllText(_caminho);
        if (string.IsNullOrWhiteSpace(texto))
            return new List<Pessoa>();

        return JsonSerializer.Deserialize<List<Pessoa>>(texto, _json) ?? new List<Pessoa>();
    }

    private void Gravar(List<Pessoa> pessoas)
    {
        File.WriteAllText(_caminho, JsonSerializer.Serialize(pessoas, _json));
    }

    private static void Preparar(Pessoa pessoa)
    {
        pessoa.Nome = (pessoa.Nome ?? string.Empty).Trim();
        pessoa.Cpf = (pessoa.Cpf ?? string.Empty).Trim();
        pessoa.Email = (pessoa.Email ?? string.Empty).Trim();
        pessoa.Telefone = string.IsNullOrWhiteSpace(pessoa.Telefone) ? null : pessoa.Telefone.Trim();
    }

    private static string SomenteDigitos(string? texto) =>
        new string((texto ?? string.Empty).Where(char.IsDigit).ToArray());
}
