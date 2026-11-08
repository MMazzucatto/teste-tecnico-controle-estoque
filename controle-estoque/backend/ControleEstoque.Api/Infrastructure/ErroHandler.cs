using ControleEstoque.Api.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControleEstoque.Api.Infrastructure;

public class ErroHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (status, titulo) = ex switch
        {
            NaoEncontradoException => (404, "Recurso não encontrado"),
            RegraDeNegocioException => (422, "Regra de negócio violada"),
            DbUpdateConcurrencyException => (409, "Conflito de concorrência, tente novamente"),
            _ => (0, "")
        };

        if (status == 0) return false; // erro inesperado segue como 500

        ctx.Response.StatusCode = status;
        await ctx.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = ex is DbUpdateConcurrencyException ? null : ex.Message
        }, ct);
        return true;
    }
}