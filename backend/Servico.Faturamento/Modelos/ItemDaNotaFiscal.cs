using System.Text.Json.Serialization;

namespace Servico.Faturamento.Modelos;

// cada linha de produto dentro da nota
public class ItemDaNotaFiscal
{
    public int Id { get; set; }

    public string CodigoDoProduto { get; set; } = string.Empty;

    public string DescricaoDoProdutoNaHoraDaVenda { get; set; } = string.Empty; // guardei uma "foto" da descricao pra nao depender do outro servico depois

    public int Quantidade { get; set; }

    public int NotaFiscalCabecalhoId { get; set; }

    // [JsonIgnore] pq essa propriedade aponta de volta pra nota (Item -> Nota -> Itens -> Nota -> ...),
    // isso criava um ciclo infinito na hora de virar JSON pra mandar pro Angular. Bug feio que passou
    // batido ate eu testar de verdade rodando (nao pega em compilacao, so em runtime).
    [JsonIgnore]
    public NotaFiscalCabecalho? NotaPai { get; set; }
}
