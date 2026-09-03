namespace Servico.Estoque.Excecoes;

// excecao "caseira" pra quando o produto nao existe no banco
public class ProdutoNaoEncontradoExcecao : Exception
{
    public ProdutoNaoEncontradoExcecao(string codigoProcurado)
        : base($"Nao achei nenhum produto com o codigo '{codigoProcurado}' :/")
    {
    }
}
