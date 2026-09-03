namespace Servico.Estoque.Modelos;

// classe que representa um produto la no banco
// (usei nome "ProdutoEstoque" pra nao bater de frente com o Produto do outro servico)
public class ProdutoEstoque
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public int SaldoAtual { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.Now;
}

// DTOs bem simples só pra nao expor a entidade inteira do EF pro front
public record ProdutoEstoqueEntradaDto(string Codigo, string Descricao, int SaldoAtual);

public record ProdutoEstoqueBaixaDto(string CodigoProduto, int QuantidadeParaBaixar);
