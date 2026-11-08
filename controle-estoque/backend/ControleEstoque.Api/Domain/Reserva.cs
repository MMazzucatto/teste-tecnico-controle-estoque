namespace ControleEstoque.Api.Domain;

public class Reserva
{
    private Reserva() { } // exigido pelo EF

    public Reserva(string produtoId, string solicitacaoId, int quantidade)
    {
        Id = Guid.NewGuid();
        ProdutoId = produtoId;
        SolicitacaoId = solicitacaoId;
        Quantidade = quantidade;
    }

    public Guid Id { get; private set; }
    public string ProdutoId { get; private set; } = default!;
    public string SolicitacaoId { get; private set; } = default!;
    public int Quantidade { get; private set; }
}