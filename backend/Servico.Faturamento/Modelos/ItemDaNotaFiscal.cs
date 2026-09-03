namespace Servico.Faturamento.Modelos;

// cada linha de produto dentro da nota
public class ItemDaNotaFiscal
{
    public int Id { get; set; }

    public string CodigoDoProduto { get; set; } = string.Empty;

    public string DescricaoDoProdutoNaHoraDaVenda { get; set; } = string.Empty; // guardei uma "foto" da descricao pra nao depender do outro servico depois

    public int Quantidade { get; set; }

    public int NotaFiscalCabecalhoId { get; set; }

    public NotaFiscalCabecalho? NotaPai { get; set; }
}
