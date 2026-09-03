// interfaces bem diretas, espelhando o DTO que o backend devolve
export interface ProdutoDoEstoque {
  id: number;
  codigo: string;
  descricao: string;
  saldoAtual: number;
  criadoEm: string;
}

export interface ProdutoParaCadastrar {
  codigo: string;
  descricao: string;
  saldoAtual: number;
}
