namespace Servico.Faturamento.Excecoes;

// usada quando o servico de estoque ta fora do ar / deu erro na chamada http
public class EstoqueIndisponivelExcecao : Exception
{
    public EstoqueIndisponivelExcecao(string mensagemOriginal)
        : base($"O servico de estoque nao respondeu direito agora. Tenta imprimir de novo daqui a pouco. (detalhe tecnico: {mensagemOriginal})")
    {
    }
}
