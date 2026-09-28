using GaragemVeiculos.Models;
using GaragemVeiculos.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GaragemVeiculos.Controllers;

public class PessoaController : Controller
{
    private readonly IPessoaRepository _pessoas;
    private readonly IReservaRepository _reservas;

    public PessoaController(IPessoaRepository pessoas, IReservaRepository reservas)
    {
        _pessoas = pessoas;
        _reservas = reservas;
    }

    public IActionResult Index()
    {
        return View(_pessoas.ObterTodas().OrderBy(p => p.Nome).ToList());
    }

    public IActionResult Create()
    {
        return View("PessoaForm", new Pessoa());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Pessoa pessoa)
    {
        Normalizar(pessoa);
        ValidarUnicidade(pessoa);
        if (!ModelState.IsValid)
            return View("PessoaForm", pessoa);

        _pessoas.Adicionar(pessoa);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var pessoa = _pessoas.ObterPorId(id);
        if (pessoa is null)
            return RedirectToAction(nameof(Index));

        return View("PessoaForm", pessoa);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Pessoa pessoa)
    {
        if (_pessoas.ObterPorId(pessoa.Id) is null)
            return RedirectToAction(nameof(Index));

        Normalizar(pessoa);
        ValidarUnicidade(pessoa);
        if (!ModelState.IsValid)
            return View("PessoaForm", pessoa);

        _pessoas.Atualizar(pessoa);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        if (_reservas.PossuiReservaAtivaOuFuturaDePessoa(id))
        {
            TempData["Erro"] = "Não é possível excluir uma pessoa com reserva em andamento ou futura.";
            return RedirectToAction(nameof(Index));
        }

        _pessoas.Remover(id);
        return RedirectToAction(nameof(Index));
    }

    private void Normalizar(Pessoa pessoa)
    {
        pessoa.Nome = (pessoa.Nome ?? string.Empty).Trim();
        pessoa.Cpf = (pessoa.Cpf ?? string.Empty).Trim();
        pessoa.Email = (pessoa.Email ?? string.Empty).Trim();
        pessoa.Telefone = string.IsNullOrWhiteSpace(pessoa.Telefone) ? null : pessoa.Telefone.Trim();

        if (pessoa.Nome.Length == 0 && ModelState[nameof(pessoa.Nome)]?.Errors.Count == 0)
            ModelState.AddModelError(nameof(pessoa.Nome), "O nome é obrigatório.");

        if (pessoa.Cpf.Length == 0 && ModelState[nameof(pessoa.Cpf)]?.Errors.Count == 0)
            ModelState.AddModelError(nameof(pessoa.Cpf), "O CPF é obrigatório.");
    }

    private void ValidarUnicidade(Pessoa pessoa)
    {
        if (pessoa.Cpf.Length > 0 && _pessoas.ExisteCpf(pessoa.Cpf, pessoa.Id))
            ModelState.AddModelError(nameof(pessoa.Cpf), "Já existe uma pessoa com este CPF.");
    }
}
