using GaragemVeiculos.Models;
using GaragemVeiculos.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GaragemVeiculos.Controllers;

public class VeiculoController : Controller
{
    private readonly IVeiculoRepository _veiculos;
    private readonly IReservaRepository _reservas;

    public VeiculoController(IVeiculoRepository veiculos, IReservaRepository reservas)
    {
        _veiculos = veiculos;
        _reservas = reservas;
    }

    public IActionResult Index()
    {
        return View(_veiculos.ObterTodas().OrderBy(v => v.Placa).ToList());
    }

    public IActionResult Create()
    {
        return View("VeiculoForm", new Veiculo());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Veiculo veiculo)
    {
        Normalizar(veiculo);
        ValidarUnicidade(veiculo);
        if (!ModelState.IsValid)
            return View("VeiculoForm", veiculo);

        _veiculos.Adicionar(veiculo);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var veiculo = _veiculos.ObterPorId(id);
        if (veiculo is null)
            return RedirectToAction(nameof(Index));

        return View("VeiculoForm", veiculo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Veiculo veiculo)
    {
        if (_veiculos.ObterPorId(veiculo.Id) is null)
            return RedirectToAction(nameof(Index));

        Normalizar(veiculo);
        ValidarUnicidade(veiculo);
        if (!ModelState.IsValid)
            return View("VeiculoForm", veiculo);

        _veiculos.Atualizar(veiculo);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        if (_reservas.PossuiReservaAtivaOuFuturaDeVeiculo(id))
        {
            TempData["Erro"] = "Não é possível excluir um veículo com reserva em andamento ou futura.";
            return RedirectToAction(nameof(Index));
        }

        _veiculos.Remover(id);
        return RedirectToAction(nameof(Index));
    }

    private void Normalizar(Veiculo veiculo)
    {
        veiculo.Placa = (veiculo.Placa ?? string.Empty).Trim();
        veiculo.Marca = (veiculo.Marca ?? string.Empty).Trim();
        veiculo.Modelo = (veiculo.Modelo ?? string.Empty).Trim();
        veiculo.Cor = string.IsNullOrWhiteSpace(veiculo.Cor) ? null : veiculo.Cor.Trim();

        if (veiculo.Placa.Length == 0 && ModelState[nameof(veiculo.Placa)]?.Errors.Count == 0)
            ModelState.AddModelError(nameof(veiculo.Placa), "A placa é obrigatória.");

        if (veiculo.Marca.Length == 0 && ModelState[nameof(veiculo.Marca)]?.Errors.Count == 0)
            ModelState.AddModelError(nameof(veiculo.Marca), "A marca é obrigatória.");

        if (veiculo.Modelo.Length == 0 && ModelState[nameof(veiculo.Modelo)]?.Errors.Count == 0)
            ModelState.AddModelError(nameof(veiculo.Modelo), "O modelo é obrigatório.");

        AjustarErroDeAno();
    }

    private void AjustarErroDeAno()
    {
        if (!ModelState.TryGetValue(nameof(Veiculo.Ano), out var estado) || estado.Errors.Count == 0)
            return;

        var tentativa = estado.AttemptedValue;
        if (string.IsNullOrWhiteSpace(tentativa) || !int.TryParse(tentativa, out var ano) || ano < 1900 || ano > 2100)
        {
            estado.Errors.Clear();
            estado.Errors.Add("Informe um ano entre 1900 e 2100.");
        }
    }

    private void ValidarUnicidade(Veiculo veiculo)
    {
        if (veiculo.Placa.Length > 0 && _veiculos.ExistePlaca(veiculo.Placa, veiculo.Id))
            ModelState.AddModelError(nameof(veiculo.Placa), "Já existe um veículo com esta placa.");
    }
}
