using GaragemVeiculos.Models;

namespace GaragemVeiculos.Repositories;

public interface IPessoaRepository
{
    List<Pessoa> ObterTodas();
    Pessoa? ObterPorId(int id);
    void Adicionar(Pessoa pessoa);
    void Atualizar(Pessoa pessoa);
    void Remover(int id);
    bool ExisteCpf(string cpf, int idIgnorado);
}
