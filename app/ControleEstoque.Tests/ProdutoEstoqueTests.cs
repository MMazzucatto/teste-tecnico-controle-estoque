using ControleEstoque.Api.Models;

namespace ControleEstoque.Tests;

public class ProdutoEstoqueTests
{
    [Fact]
    public void Disponivel_DeveCalcularQuantidadeDisponivelCorretamente()
    {
        var produto = CriarProduto(quantidade: 20, reservado: 5, bloqueado: 3);

        Assert.Equal(12, produto.Disponivel);
    }

    [Fact]
    public void Disponivel_DeveSerZeroQuandoTudoEstiverReservadoOuBloqueado()
    {
        var produto = CriarProduto(quantidade: 10, reservado: 7, bloqueado: 3);

        Assert.Equal(0, produto.Disponivel);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    public void ValidarQuantidade_DeveAceitarQuantidadePositiva(int quantidade)
    {
        var excecao = Record.Exception(() => ProdutoEstoque.ValidarQuantidade(quantidade));

        Assert.Null(excecao);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void ValidarQuantidade_DeveRejeitarQuantidadeInvalida(int quantidade)
    {
        var excecao = Assert.Throws<InvalidOperationException>(
            () => ProdutoEstoque.ValidarQuantidade(quantidade));

        Assert.Equal("A quantidade deve ser maior que zero.", excecao.Message);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    public void ValidarDisponibilidade_DeveAceitarQuantidadeDentroDoSaldo(int quantidade)
    {
        var produto = CriarProduto(quantidade: 10, reservado: 3, bloqueado: 2);

        var excecao = Record.Exception(() => produto.ValidarDisponibilidade(quantidade));

        Assert.Null(excecao);
    }

    [Fact]
    public void ValidarDisponibilidade_DeveRejeitarProdutoSemEstoque()
    {
        var produto = CriarProduto(quantidade: 0);

        var excecao = Assert.Throws<InvalidOperationException>(
            () => produto.ValidarDisponibilidade(1));

        Assert.Equal("Estoque indisponível: o produto está sem unidades em estoque.", excecao.Message);
    }

    [Fact]
    public void ValidarDisponibilidade_DeveRejeitarSaldoTotalmenteReservadoOuBloqueado()
    {
        var produto = CriarProduto(quantidade: 10, reservado: 7, bloqueado: 3);

        var excecao = Assert.Throws<InvalidOperationException>(
            () => produto.ValidarDisponibilidade(1));

        Assert.Equal("Estoque indisponível: todas as unidades estão reservadas ou bloqueadas.", excecao.Message);
    }

    [Fact]
    public void ValidarDisponibilidade_DeveRejeitarQuantidadeMaiorQueSaldoDisponivel()
    {
        var produto = CriarProduto(quantidade: 10, reservado: 3, bloqueado: 2);

        var excecao = Assert.Throws<InvalidOperationException>(
            () => produto.ValidarDisponibilidade(6));

        Assert.Equal("Estoque insuficiente.", excecao.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ValidarLiberacao_DeveAceitarQuantidadeReservadaOuBloqueada(bool ehReserva)
    {
        var produto = CriarProduto(quantidade: 10, reservado: 4, bloqueado: 3);

        var excecao = Record.Exception(() => produto.ValidarLiberacao(2, ehReserva));

        Assert.Null(excecao);
    }

    [Theory]
    [InlineData(true, "Não há reserva para liberar neste produto.")]
    [InlineData(false, "Não há bloqueio para liberar neste produto.")]
    public void ValidarLiberacao_DeveRejeitarLiberacaoSemSaldo(bool ehReserva, string mensagem)
    {
        var produto = CriarProduto(quantidade: 10);

        var excecao = Assert.Throws<InvalidOperationException>(
            () => produto.ValidarLiberacao(1, ehReserva));

        Assert.Equal(mensagem, excecao.Message);
    }

    [Theory]
    [InlineData(true, 4, "Quantidade maior que a reservada: reservado 3, solicitado 4.")]
    [InlineData(false, 4, "Quantidade maior que a bloqueada: bloqueado 3, solicitado 4.")]
    public void ValidarLiberacao_DeveRejeitarQuantidadeMaiorQueSaldo(bool ehReserva, int quantidade, string mensagem)
    {
        var produto = CriarProduto(quantidade: 10, reservado: 3, bloqueado: 3);

        var excecao = Assert.Throws<InvalidOperationException>(
            () => produto.ValidarLiberacao(quantidade, ehReserva));

        Assert.Equal(mensagem, excecao.Message);
    }

    private static ProdutoEstoque CriarProduto(int quantidade, int reservado = 0, int bloqueado = 0) =>
        new()
        {
            Id = 1,
            Nome = "Produto de teste",
            Quantidade = quantidade,
            Reservado = reservado,
            Bloqueado = bloqueado
        };
}
