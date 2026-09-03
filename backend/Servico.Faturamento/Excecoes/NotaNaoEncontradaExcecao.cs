namespace Servico.Faturamento.Excecoes;

public class NotaNaoEncontradaExcecao : Exception
{
    public NotaNaoEncontradaExcecao(int numero)
        : base($"Nota numero {numero} nao existe no banco.")
    {
    }
}
