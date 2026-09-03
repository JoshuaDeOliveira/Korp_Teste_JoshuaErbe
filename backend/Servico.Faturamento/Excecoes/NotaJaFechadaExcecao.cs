namespace Servico.Faturamento.Excecoes;

// dispara quando tenta imprimir nota que ja ta fechada (regra obrigatoria do teste)
public class NotaJaFechadaExcecao : Exception
{
    public NotaJaFechadaExcecao(int numero)
        : base($"A nota {numero} ja foi impressa/fechada antes, nao da pra imprimir de novo.")
    {
    }
}
