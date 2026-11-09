namespace ControleEstoque.Api.Models;

public class ProdutoEstoque
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public int Quantidade { get; set; }
    public int Reservado { get; set; }
    public int Bloqueado { get; set; }

    public int Disponivel => Quantidade - Reservado - Bloqueado;

    public static void ValidarQuantidade(int quantidade)
    {
        if (quantidade <= 0)
            throw new InvalidOperationException("A quantidade deve ser maior que zero.");
    }

    public void ValidarDisponibilidade(int quantidade)
    {
        ValidarQuantidade(quantidade);

        if (Quantidade == 0)
            throw new InvalidOperationException("Estoque indisponível: o produto está sem unidades em estoque.");

        if (Disponivel == 0)
            throw new InvalidOperationException("Estoque indisponível: todas as unidades estão reservadas ou bloqueadas.");

        if (quantidade > Disponivel)
            throw new InvalidOperationException("Estoque insuficiente.");
    }

    public void ValidarLiberacao(int quantidade, bool ehReserva)
    {
        ValidarQuantidade(quantidade);

        var atual = ehReserva ? Reservado : Bloqueado;

        if (atual == 0)
            throw new InvalidOperationException(ehReserva
                ? "Não há reserva para liberar neste produto."
                : "Não há bloqueio para liberar neste produto.");

        if (quantidade > atual)
            throw new InvalidOperationException(ehReserva
                ? $"Quantidade maior que a reservada: reservado {atual}, solicitado {quantidade}."
                : $"Quantidade maior que a bloqueada: bloqueado {atual}, solicitado {quantidade}.");
    }
}