namespace ControleEstoque.Api.Domain;

public class Bloqueio
{
    private Bloqueio() { } // exigido pelo EF

    public Bloqueio(string produtoId, int quantidade, string? motivo)
    {
        Id = Guid.NewGuid();
        ProdutoId = produtoId;
        Quantidade = quantidade;
        Motivo = motivo;
    }

    public Guid Id { get; private set; }
    public string ProdutoId { get; private set; } = default!;
    public int Quantidade { get; private set; }
    public string? Motivo { get; private set; }
}