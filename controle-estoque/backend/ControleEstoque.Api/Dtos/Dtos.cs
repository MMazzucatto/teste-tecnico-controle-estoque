namespace ControleEstoque.Api.Dtos;

public record QuantidadeRequest(int Quantidade);
public record ReservaRequest(string SolicitacaoId, int Quantidade);
public record BloqueioRequest(int Quantidade, string? Motivo);

public record ReservaResponse(Guid Id, string SolicitacaoId, int Quantidade);
public record BloqueioResponse(Guid Id, int Quantidade, string? Motivo);
public record ProdutoResponse(
    string Id, int QuantidadeTotal, int QuantidadeDisponivel,
    int QuantidadeReservada, int QuantidadeBloqueada,
    IEnumerable<ReservaResponse> Reservas, IEnumerable<BloqueioResponse> Bloqueios);