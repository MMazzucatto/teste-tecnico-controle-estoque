public class Produto
{
    public int IdCarrinho { get; set; }
    public int IdProduto { get; set; }
    public string Nome { get; set; } = "";
    public decimal Preco { get; set; }
    public int Quantidade { get; set; }
    public DateTime DataAdicionado { get; set; }

    public decimal Subtotal => Preco * Quantidade;
}