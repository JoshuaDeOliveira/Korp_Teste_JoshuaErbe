export enum StatusDaNotaNoFront {
  Aberta = 0,
  Fechada = 1
}

export interface ItemDaNota {
  id: number;
  codigoDoProduto: string;
  descricaoDoProdutoNaHoraDaVenda: string;
  quantidade: number;
}

export interface NotaFiscal {
  id: number;
  numeroSequencial: number;
  status: StatusDaNotaNoFront;
  criadaEm: string;
  fechadaEm: string | null;
  itens: ItemDaNota[];
}

export interface ItemParaEnviar {
  codigoDoProduto: string;
  descricaoDoProdutoNaHoraDaVenda: string;
  quantidade: number;
}
