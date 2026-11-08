namespace ControleEstoque.Api.Domain;

public class Produto
{
    private readonly List<Reserva> _reservas = new();
    private readonly List<Bloqueio> _bloqueios = new();

    private Produto() { } // exigido pelo EF
    public Produto(string id) => Id = id;

    public string Id { get; private set; } = default!;
    public int QuantidadeTotal { get; private set; }
    public int Versao { get; private set; } // controle de concorrência

    public IReadOnlyCollection<Reserva> Reservas => _reservas;
    public IReadOnlyCollection<Bloqueio> Bloqueios => _bloqueios;

    // Calculadas: nunca ficam gravadas, então não há como divergirem
    public int QuantidadeReservada => _reservas.Sum(r => r.Quantidade);
    public int QuantidadeBloqueada => _bloqueios.Sum(b => b.Quantidade);
    public int QuantidadeDisponivel => QuantidadeTotal - QuantidadeReservada - QuantidadeBloqueada;

    public void Entrada(int quantidade)
    {
        ValidarQuantidade(quantidade);
        QuantidadeTotal += quantidade;
        Versao++;
    }

    public void Saida(int quantidade)
    {
        ValidarQuantidade(quantidade);
        GarantirDisponivel(quantidade);
        QuantidadeTotal -= quantidade;
        Versao++;
    }

    public Reserva Reservar(string solicitacaoId, int quantidade)
    {
        if (string.IsNullOrWhiteSpace(solicitacaoId))
            throw new RegraDeNegocioException("A solicitação é obrigatória.");
        ValidarQuantidade(quantidade);
        GarantirDisponivel(quantidade);

        var reserva = new Reserva(Id, solicitacaoId, quantidade);
        _reservas.Add(reserva);
        Versao++;
        return reserva;
    }

    public Bloqueio Bloquear(int quantidade, string? motivo)
    {
        ValidarQuantidade(quantidade);
        GarantirDisponivel(quantidade);

        var bloqueio = new Bloqueio(Id, quantidade, motivo);
        _bloqueios.Add(bloqueio);
        Versao++;
        return bloqueio;
    }

    public void LiberarReserva(Guid reservaId)
    {
        var reserva = _reservas.FirstOrDefault(r => r.Id == reservaId)
            ?? throw new NaoEncontradoException("Reserva não encontrada.");
        _reservas.Remove(reserva); // a quantidade volta ao disponível sozinha
        Versao++;
    }

    public void LiberarBloqueio(Guid bloqueioId)
    {
        var bloqueio = _bloqueios.FirstOrDefault(b => b.Id == bloqueioId)
            ?? throw new NaoEncontradoException("Bloqueio não encontrado.");
        _bloqueios.Remove(bloqueio);
        Versao++;
    }

    private static void ValidarQuantidade(int quantidade)
    {
        if (quantidade <= 0)
            throw new RegraDeNegocioException("A quantidade deve ser maior que zero.");
    }

    private void GarantirDisponivel(int quantidade)
    {
        if (quantidade > QuantidadeDisponivel)
            throw new RegraDeNegocioException(
                $"Quantidade indisponível. Disponível: {QuantidadeDisponivel}.");
    }
}