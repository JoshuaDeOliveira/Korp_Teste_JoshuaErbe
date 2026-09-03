namespace Servico.Estoque.Excecoes;

// disparada quando tenta tirar mais do que tem em estoque
public class SaldoInsuficienteExcecao : Exception
{
    public SaldoInsuficienteExcecao(string codigo, int saldoAtual, int quantidadeQuePediram)
        : base($"Saldo insuficiente pro produto {codigo}. Tem {saldoAtual}, pediram {quantidadeQuePediram}.")
    {
    }
}
