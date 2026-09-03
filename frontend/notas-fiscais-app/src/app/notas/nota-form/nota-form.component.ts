import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ProdutoDoEstoque } from '../../produtos/produto.model';
import { ServicoDeProduto } from '../../produtos/produto.service';
import { ServicoDeNotaFiscal } from '../nota.service';
import { ItemParaEnviar } from '../nota.model';
import { ServicoDeNotificacao } from '../../nucleo/servicos/notificacao.service';

@Component({
  selector: 'app-form-nota',
  templateUrl: './nota-form.component.html',
  styleUrls: ['./nota-form.component.css']
})
export class ComponenteFormularioDeNota implements OnInit {

  produtosDisponiveisNoEstoque: ProdutoDoEstoque[] = [];

  // campos "soltos" do mini formulario de adicionar item (nao usei reactive forms
  // aqui pq era so 2 campinhos, ngModel resolveu rapido)
  codigoDoProdutoEscolhido = '';
  quantidadeEscolhida = 1;

  itensJaAdicionadosNaNota: ItemParaEnviar[] = [];

  estaSalvandoANota = false;

  constructor(
    private servicoDeProduto: ServicoDeProduto,
    private servicoDeNota: ServicoDeNotaFiscal,
    private notificacao: ServicoDeNotificacao,
    private roteador: Router
  ) { }

  ngOnInit(): void {
    // busca os produtos cadastrados no OUTRO microsservico pra popular o combo
    this.servicoDeProduto.buscarTodosOsProdutos().subscribe({
      next: (produtos) => this.produtosDisponiveisNoEstoque = produtos,
      error: () => { /* toast global cuida */ }
    });
  }

  adicionarItemNaListaDaNota(): void {
    if (!this.codigoDoProdutoEscolhido || this.quantidadeEscolhida <= 0) {
      this.notificacao.mostrarErro('Escolhe um produto e uma quantidade valida antes de adicionar.');
      return;
    }

    const produtoEscolhido = this.produtosDisponiveisNoEstoque.find(p => p.codigo === this.codigoDoProdutoEscolhido);
    if (!produtoEscolhido) return;

    this.itensJaAdicionadosNaNota.push({
      codigoDoProduto: produtoEscolhido.codigo,
      descricaoDoProdutoNaHoraDaVenda: produtoEscolhido.descricao,
      quantidade: this.quantidadeEscolhida
    });

    // limpa os campos pra adicionar o proximo
    this.codigoDoProdutoEscolhido = '';
    this.quantidadeEscolhida = 1;
  }

  removerItemDaLista(indiceDoItem: number): void {
    this.itensJaAdicionadosNaNota.splice(indiceDoItem, 1);
  }

  salvarNotaFiscal(): void {
    if (this.itensJaAdicionadosNaNota.length === 0) {
      this.notificacao.mostrarErro('Adiciona pelo menos 1 produto na nota antes de salvar.');
      return;
    }

    this.estaSalvandoANota = true;

    this.servicoDeNota.criarNotaComItens(this.itensJaAdicionadosNaNota).subscribe({
      next: (notaCriada) => {
        this.notificacao.mostrarSucesso(`Nota ${notaCriada.numeroSequencial} criada com status Aberta!`);
        this.roteador.navigate(['/notas']);
      },
      error: () => {
        this.estaSalvandoANota = false;
      }
    });
  }
}
