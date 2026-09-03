namespace Servico.Faturamento.Modelos;

public enum StatusDaNota
{
    Aberta = 0,
    Fechada = 1
}

// cabecalho da nota fiscal, o "pai" dos itens
public class NotaFiscalCabecalho
{
    public int Id { get; set; }

    // numeracao sequencial da nota (comeca em 1 e vai subindo)
    public int NumeroSequencial { get; set; }

    public StatusDaNota Status { get; set; } = StatusDaNota.Aberta;

    public DateTime CriadaEm { get; set; } = DateTime.Now;

    public DateTime? FechadaEm { get; set; }

    public List<ItemDaNotaFiscal> Itens { get; set; } = new();
}

// DTOs de entrada, bem simplezinhos
public record ItemEntradaDto(string CodigoDoProduto, string DescricaoDoProdutoNaHoraDaVenda, int Quantidade);

public record NotaFiscalEntradaDto(List<ItemEntradaDto> Itens);
