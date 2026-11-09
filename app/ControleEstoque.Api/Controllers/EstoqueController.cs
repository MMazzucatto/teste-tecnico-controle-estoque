using ControleEstoque.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ControleEstoque.Api.Controllers;

[AutoValidateAntiforgeryToken]
public class EstoqueController(IConfiguration configuration) : Controller
{
    // ---------- Tela ----------

    [HttpGet]
    public async Task<IActionResult> Entrada()
    {
        try
        {
            var produtos = await ListarProdutos();
            return View("Index", produtos);
        }
        catch (Exception ex)
        {
            ViewBag.Erro = ex.Message;
            return View("Index", new List<ProdutoEstoque>());
        }
    }

    // ---------- Ações (recebem a requisição e devolvem JSON) ----------

    [HttpPost]
    public async Task<IActionResult> IncluirProduto(int id, int quantidade)
    {
        try
        {
            await EntradaProduto(id, quantidade);
            return Json(new { sucesso = true, mensagem = "Entrada registrada." });
        }
        catch (Exception ex)
        {
            return Json(new { sucesso = false, mensagem = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> RetirarProduto(int id, int quantidade)
    {
        try
        {
            await SaidaProduto(id, quantidade);
            return Json(new { sucesso = true, mensagem = "Saída registrada." });
        }
        catch (Exception ex)
        {
            return Json(new { sucesso = false, mensagem = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ReservarProduto(int id, int quantidade)
    {
        try
        {
            await ReservaProduto(id, quantidade);
            return Json(new { sucesso = true, mensagem = "Reserva criada." });
        }
        catch (Exception ex)
        {
            return Json(new { sucesso = false, mensagem = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> BloquearProduto(int id, int quantidade)
    {
        try
        {
            await BloqueioProduto(id, quantidade);
            return Json(new { sucesso = true, mensagem = "Bloqueio criado." });
        }
        catch (Exception ex)
        {
            return Json(new { sucesso = false, mensagem = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> LiberarReserva(int id, int quantidade)
    {
        try
        {
            await LiberarReservaProduto(id, quantidade);
            return Json(new { sucesso = true, mensagem = "Reserva liberada." });
        }
        catch (Exception ex)
        {
            return Json(new { sucesso = false, mensagem = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> LiberarBloqueio(int id, int quantidade)
    {
        try
        {
            await LiberarBloqueioProduto(id, quantidade);
            return Json(new { sucesso = true, mensagem = "Bloqueio liberado." });
        }
        catch (Exception ex)
        {
            return Json(new { sucesso = false, mensagem = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ConsultarProduto(int id)
    {
        try
        {
            var produto = await ConsultaProduto(id);
            return Json(new
            {
                sucesso = true,
                produto = new
                {
                    produto.Id,
                    produto.Nome,
                    produto.Quantidade,
                    produto.Reservado,
                    produto.Bloqueado,
                    produto.Disponivel
                }
            });
        }
        catch (Exception ex)
        {
            return Json(new { sucesso = false, mensagem = ex.Message });
        }
    }

    // ---------- Operações no banco ----------

    private async Task<List<ProdutoEstoque>> ListarProdutos()
    {
        const string sql = @"
            SELECT Id, Nome, Quantidade, Reservado, Bloqueado
            FROM Produto
            ORDER BY Nome";

        var produtos = new List<ProdutoEstoque>();

        await using var connection = new SqlConnection(ObterConnectionString());
        await connection.OpenAsync();

        await using var comm = new SqlCommand(sql, connection);
        await using var reader = await comm.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            produtos.Add(new ProdutoEstoque
            {
                Id = reader.GetInt32(0),
                Nome = reader.GetString(1),
                Quantidade = reader.GetInt32(2),
                Reservado = reader.GetInt32(3),
                Bloqueado = reader.GetInt32(4)
            });
        }

        return produtos;
    }

    private async Task<ProdutoEstoque> ConsultaProduto(int id)
    {
        const string sql = @"
            SELECT Id, Nome, Quantidade, Reservado, Bloqueado
            FROM Produto
            WHERE Id = @id";

        await using var connection = new SqlConnection(ObterConnectionString());
        await connection.OpenAsync();

        await using var comm = new SqlCommand(sql, connection);
        comm.Parameters.Add("@id", SqlDbType.Int).Value = id;

        await using var reader = await comm.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            throw new Exception("Produto não encontrado.");

        return new ProdutoEstoque
        {
            Id = reader.GetInt32(0),
            Nome = reader.GetString(1),
            Quantidade = reader.GetInt32(2),
            Reservado = reader.GetInt32(3),
            Bloqueado = reader.GetInt32(4)
        };
    }

    private async Task EntradaProduto(int id, int quantidade)
    {
        ValidarQuantidade(quantidade);

        const string sql = @"
            UPDATE Produto
            SET Quantidade = Quantidade + @quantidade
            WHERE Id = @id";

        await using var connection = new SqlConnection(ObterConnectionString());
        await connection.OpenAsync();

        await using var comm = new SqlCommand(sql, connection);
        comm.Parameters.Add("@quantidade", SqlDbType.Int).Value = quantidade;
        comm.Parameters.Add("@id", SqlDbType.Int).Value = id;

        var linhas = await comm.ExecuteNonQueryAsync();

        if (linhas == 0)
            throw new Exception("Produto não encontrado.");
    }

    private async Task SaidaProduto(int id, int quantidade)
    {
        ValidarQuantidade(quantidade);

        // Só tira se o disponível (quantidade - reservado - bloqueado) for suficiente
        const string sql = @"
            UPDATE Produto
            SET Quantidade = Quantidade - @quantidade
            WHERE Id = @id
              AND Quantidade - Reservado - Bloqueado >= @quantidade";

        await using var connection = new SqlConnection(ObterConnectionString());
        await connection.OpenAsync();

        await using var comm = new SqlCommand(sql, connection);
        comm.Parameters.Add("@quantidade", SqlDbType.Int).Value = quantidade;
        comm.Parameters.Add("@id", SqlDbType.Int).Value = id;

        var linhas = await comm.ExecuteNonQueryAsync();

        if (linhas == 0)
            await LancarErroSaldoAsync(id, quantidade);
    }

    private async Task ReservaProduto(int id, int quantidade)
    {
        ValidarQuantidade(quantidade);

        const string sql = @"
            UPDATE Produto
            SET Reservado = Reservado + @quantidade
            WHERE Id = @id
              AND Quantidade - Reservado - Bloqueado >= @quantidade";

        await using var connection = new SqlConnection(ObterConnectionString());
        await connection.OpenAsync();

        await using var comm = new SqlCommand(sql, connection);
        comm.Parameters.Add("@quantidade", SqlDbType.Int).Value = quantidade;
        comm.Parameters.Add("@id", SqlDbType.Int).Value = id;

        var linhas = await comm.ExecuteNonQueryAsync();

        if (linhas == 0)
            await LancarErroSaldoAsync(id, quantidade);
    }

    private async Task BloqueioProduto(int id, int quantidade)
    {
        ValidarQuantidade(quantidade);

        const string sql = @"
            UPDATE Produto
            SET Bloqueado = Bloqueado + @quantidade
            WHERE Id = @id
              AND Quantidade - Reservado - Bloqueado >= @quantidade";

        await using var connection = new SqlConnection(ObterConnectionString());
        await connection.OpenAsync();

        await using var comm = new SqlCommand(sql, connection);
        comm.Parameters.Add("@quantidade", SqlDbType.Int).Value = quantidade;
        comm.Parameters.Add("@id", SqlDbType.Int).Value = id;

        var linhas = await comm.ExecuteNonQueryAsync();

        if (linhas == 0)
            await LancarErroSaldoAsync(id, quantidade);
    }

    private async Task LiberarReservaProduto(int id, int quantidade)
    {
        ValidarQuantidade(quantidade);

        const string sql = @"
            UPDATE Produto
            SET Reservado = Reservado - @quantidade
            WHERE Id = @id
              AND Reservado >= @quantidade";

        await using var connection = new SqlConnection(ObterConnectionString());
        await connection.OpenAsync();

        await using var comm = new SqlCommand(sql, connection);
        comm.Parameters.Add("@quantidade", SqlDbType.Int).Value = quantidade;
        comm.Parameters.Add("@id", SqlDbType.Int).Value = id;

        var linhas = await comm.ExecuteNonQueryAsync();

        if (linhas == 0)
            await LancarErroLiberacaoAsync(id, quantidade, ehReserva: true);
    }

    private async Task LiberarBloqueioProduto(int id, int quantidade)
    {
        ValidarQuantidade(quantidade);

        const string sql = @"
            UPDATE Produto
            SET Bloqueado = Bloqueado - @quantidade
            WHERE Id = @id
              AND Bloqueado >= @quantidade";

        await using var connection = new SqlConnection(ObterConnectionString());
        await connection.OpenAsync();

        await using var comm = new SqlCommand(sql, connection);
        comm.Parameters.Add("@quantidade", SqlDbType.Int).Value = quantidade;
        comm.Parameters.Add("@id", SqlDbType.Int).Value = id;

        var linhas = await comm.ExecuteNonQueryAsync();

        if (linhas == 0)
            await LancarErroLiberacaoAsync(id, quantidade, ehReserva: false);
    }

    // ---------- Auxiliares ----------

    private string ObterConnectionString() =>
        configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string 'Default' não encontrada.");

    private static void ValidarQuantidade(int quantidade) =>
        ProdutoEstoque.ValidarQuantidade(quantidade);

 
    private async Task LancarErroSaldoAsync(int id, int quantidade)
    {
        // Se o produto não existir, a própria ConsultaProduto lança "Produto não encontrado."
        var produto = await ConsultaProduto(id);
        produto.ValidarDisponibilidade(quantidade);

        throw new Exception("Não foi possível concluir a operação porque o estoque mudou. Tente novamente.");
    }

    private async Task LancarErroLiberacaoAsync(int id, int quantidade, bool ehReserva)
    {
        var produto = await ConsultaProduto(id);
        produto.ValidarLiberacao(quantidade, ehReserva);

        throw new Exception("Não foi possível concluir a operação porque o estoque mudou. Tente novamente.");
    }
}