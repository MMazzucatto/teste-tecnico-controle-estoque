using ControleEstoque.Api.Domain;
using ControleEstoque.Api.Dtos;
using ControleEstoque.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ControleEstoque.Api.Services;

public class EstoqueService(EstoqueDbContext db)
{
    public async Task<ProdutoResponse> ConsultarAsync(string id) =>
        Mapear(await ObterAsync(id));

    public async Task<ProdutoResponse> EntradaAsync(string id, int quantidade)
    {
        var produto = await db.Produtos
            .Include(p => p.Reservas).Include(p => p.Bloqueios)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produto is null)
        {
            produto = new Produto(id); // decisão: entrada cria o produto
            db.Produtos.Add(produto);
        }

        produto.Entrada(quantidade);
        await db.SaveChangesAsync();
        return Mapear(produto);
    }

    public async Task<ProdutoResponse> SaidaAsync(string id, int quantidade)
    {
        var produto = await ObterAsync(id);
        produto.Saida(quantidade);
        await db.SaveChangesAsync();
        return Mapear(produto);
    }

    public async Task<ProdutoResponse> ReservarAsync(string id, ReservaRequest req)
    {
        var produto = await ObterAsync(id);
        produto.Reservar(req.SolicitacaoId, req.Quantidade);
        await db.SaveChangesAsync();
        return Mapear(produto);
    }

    public async Task<ProdutoResponse> BloquearAsync(string id, BloqueioRequest req)
    {
        var produto = await ObterAsync(id);
        produto.Bloquear(req.Quantidade, req.Motivo);
        await db.SaveChangesAsync();
        return Mapear(produto);
    }

    public async Task<ProdutoResponse> LiberarReservaAsync(string id, Guid reservaId)
    {
        var produto = await ObterAsync(id);
        produto.LiberarReserva(reservaId);
        await db.SaveChangesAsync();
        return Mapear(produto);
    }

    public async Task<ProdutoResponse> LiberarBloqueioAsync(string id, Guid bloqueioId)
    {
        var produto = await ObterAsync(id);
        produto.LiberarBloqueio(bloqueioId);
        await db.SaveChangesAsync();
        return Mapear(produto);
    }

    private async Task<Produto> ObterAsync(string id) =>
        await db.Produtos
            .Include(p => p.Reservas).Include(p => p.Bloqueios)
            .FirstOrDefaultAsync(p => p.Id == id)
        ?? throw new NaoEncontradoException("Produto não encontrado.");

    private static ProdutoResponse Mapear(Produto p) => new(
        p.Id, p.QuantidadeTotal, p.QuantidadeDisponivel,
        p.QuantidadeReservada, p.QuantidadeBloqueada,
        p.Reservas.Select(r => new ReservaResponse(r.Id, r.SolicitacaoId, r.Quantidade)),
        p.Bloqueios.Select(b => new BloqueioResponse(b.Id, b.Quantidade, b.Motivo)));
}