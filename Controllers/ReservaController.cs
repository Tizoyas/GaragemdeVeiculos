using GaragemVeiculos.Models;
using GaragemVeiculos.Repositories;
using GaragemVeiculos.Services;
using Microsoft.AspNetCore.Mvc;

namespace GaragemVeiculos.Controllers;

public class ReservaController : Controller
{
    private readonly IReservaRepository _reservas;
    private readonly IPessoaRepository _pessoas;
    private readonly IVeiculoRepository _veiculos;
    private readonly ReservaService _servico;

    public ReservaController(
        IReservaRepository reservas,
        IPessoaRepository pessoas,
        IVeiculoRepository veiculos,
        ReservaService servico)
    {
        _reservas = reservas;
        _pessoas = pessoas;
        _veiculos = veiculos;
        _servico = servico;
    }

    public IActionResult Index(int? pessoaId, DateOnly? de, DateOnly? ate)
    {
        return View(Montar(filtroPessoaId: pessoaId, filtroDe: de, filtroAte: ate));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Reserva reserva)
    {
        if (!AplicarValidacao(reserva))
            return View("Index", Montar(reserva));

        _reservas.Adicionar(reserva);
        TempData["Ok"] = "Reserva registrada.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var reserva = _reservas.ObterPorId(id);
        if (reserva is null)
            return RedirectToAction(nameof(Index));

        return View("Index", Montar(reserva));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Reserva reserva)
    {
        if (_reservas.ObterPorId(reserva.Id) is null)
            return RedirectToAction(nameof(Index));

        if (!AplicarValidacao(reserva))
            return View("Index", Montar(reserva));

        _reservas.Atualizar(reserva);
        TempData["Ok"] = "Período da reserva atualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        _reservas.Remover(id);
        TempData["Ok"] = "Reserva cancelada.";
        return RedirectToAction(nameof(Index));
    }

    private bool AplicarValidacao(Reserva reserva)
    {
        RemoverErroDeConversao(nameof(reserva.VeiculoId));
        RemoverErroDeConversao(nameof(reserva.PessoaId));
        RemoverErroDeConversao(nameof(reserva.DataInicio));
        RemoverErroDeConversao(nameof(reserva.DataFim));

        foreach (var erro in _servico.Validar(reserva))
            ModelState.AddModelError(erro.Campo, erro.Mensagem);

        return ModelState.IsValid;
    }

    private void RemoverErroDeConversao(string campo)
    {
        if (!ModelState.TryGetValue(campo, out var estado))
            return;

        var mensagens = estado.Errors
            .Select(e => e.ErrorMessage)
            .Where(mensagem => !string.IsNullOrEmpty(mensagem)
                && !mensagem.Contains("invalid", StringComparison.OrdinalIgnoreCase)
                && !mensagem.Contains("is not valid", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (mensagens.Count == estado.Errors.Count)
            return;

        estado.Errors.Clear();
        foreach (var mensagem in mensagens)
            estado.Errors.Add(mensagem);

        if (estado.Errors.Count == 0)
            ModelState.Remove(campo);
    }

    private ReservaPagina Montar(
        Reserva? formulario = null,
        int? filtroPessoaId = null,
        DateOnly? filtroDe = null,
        DateOnly? filtroAte = null)
    {
        var pessoas = _pessoas.ObterTodas().OrderBy(p => p.Nome).ToList();
        var veiculos = _veiculos.ObterTodas().OrderBy(v => v.Placa).ToList();
        var reservas = _reservas.ObterTodas();
        var hoje = DateOnly.FromDateTime(DateTime.Today);

        var consulta = reservas.AsEnumerable();
        if (filtroPessoaId is > 0)
            consulta = consulta.Where(r => r.PessoaId == filtroPessoaId);
        if (filtroDe is not null)
            consulta = consulta.Where(r => r.DataFim >= filtroDe);
        if (filtroAte is not null)
            consulta = consulta.Where(r => r.DataInicio <= filtroAte);

        return new ReservaPagina
        {
            Pessoas = pessoas,
            FiltroPessoaId = filtroPessoaId,
            FiltroDe = filtroDe,
            FiltroAte = filtroAte,
            Formulario = formulario ?? new Reserva { DataInicio = hoje, DataFim = hoje },
            Veiculos = veiculos.Select(v => new VeiculoDoDia
            {
                Id = v.Id,
                Placa = v.Placa,
                Descricao = $"{v.Marca} {v.Modelo}".Trim(),
                Reservado = reservas.Any(r => r.VeiculoId == v.Id && r.DataInicio <= hoje && hoje <= r.DataFim)
            }).ToList(),
            Reservas = consulta
                .OrderByDescending(r => r.DataInicio)
                .Select(r =>
                {
                    var veiculo = veiculos.FirstOrDefault(v => v.Id == r.VeiculoId);
                    var pessoa = pessoas.FirstOrDefault(p => p.Id == r.PessoaId);
                    return new ReservaLinha
                    {
                        Id = r.Id,
                        Placa = veiculo?.Placa ?? "—",
                        Marca = veiculo?.Marca ?? string.Empty,
                        Modelo = veiculo?.Modelo ?? "veículo removido",
                        Pessoa = pessoa?.Nome ?? "pessoa removida",
                        DataInicio = r.DataInicio,
                        DataFim = r.DataFim
                    };
                })
                .ToList()
        };
    }
}
