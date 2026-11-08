using ControleEstoque.Api.Dtos;
using ControleEstoque.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ControleEstoque.Api.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController(EstoqueService service) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<ProdutoResponse>> Consultar(string id) =>
        Ok(await service.ConsultarAsync(id));

    [HttpPost("{id}/entrada")]
    public async Task<ActionResult<ProdutoResponse>> Entrada(string id, QuantidadeRequest req) =>
        Ok(await service.EntradaAsync(id, req.Quantidade));

    [HttpPost("{id}/saida")]
    public async Task<ActionResult<ProdutoResponse>> Saida(string id, QuantidadeRequest req) =>
        Ok(await service.SaidaAsync(id, req.Quantidade));

    [HttpPost("{id}/reservas")]
    public async Task<ActionResult<ProdutoResponse>> Reservar(string id, ReservaRequest req) =>
        Ok(await service.ReservarAsync(id, req));

    [HttpPost("{id}/bloqueios")]
    public async Task<ActionResult<ProdutoResponse>> Bloquear(string id, BloqueioRequest req) =>
        Ok(await service.BloquearAsync(id, req));

    [HttpDelete("{id}/reservas/{reservaId:guid}")]
    public async Task<ActionResult<ProdutoResponse>> LiberarReserva(string id, Guid reservaId) =>
        Ok(await service.LiberarReservaAsync(id, reservaId));

    [HttpDelete("{id}/bloqueios/{bloqueioId:guid}")]
    public async Task<ActionResult<ProdutoResponse>> LiberarBloqueio(string id, Guid bloqueioId) =>
        Ok(await service.LiberarBloqueioAsync(id, bloqueioId));
}