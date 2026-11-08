namespace ControleEstoque.Api.Domain;

public class RegraDeNegocioException(string mensagem) : Exception(mensagem);
public class NaoEncontradoException(string mensagem) : Exception(mensagem);