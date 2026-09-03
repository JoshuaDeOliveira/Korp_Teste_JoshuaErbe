import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { ProdutoDoEstoque } from '../produto.model';
import { ServicoDeProduto } from '../produto.service';
import { ServicoDeNotificacao } from '../../nucleo/servicos/notificacao.service';

@Component({
  selector: 'app-lista-produtos',
  templateUrl: './produto-lista.component.html',
  styleUrls: ['./produto-lista.component.css']
})
export class ComponenteListaDeProdutos implements OnInit, OnDestroy {

  listaDeProdutos: ProdutoDoEstoque[] = [];
  estaCarregandoOsProdutos = false;

  // guardo a inscricao pra poder cancelar la no ngOnDestroy e nao vazar memoria
  private inscricaoDaBuscaDeProdutos?: Subscription;

  constructor(
    private servicoDeProduto: ServicoDeProduto,
    private notificacao: ServicoDeNotificacao
  ) { }

  ngOnInit(): void {
    this.recarregarListaDeProdutos();
  }

  ngOnDestroy(): void {
    // boa pratica que aprendi: sempre cancelar subscription manual pra nao vazar memoria
    this.inscricaoDaBuscaDeProdutos?.unsubscribe();
  }

  recarregarListaDeProdutos(): void {
    this.estaCarregandoOsProdutos = true;

    this.inscricaoDaBuscaDeProdutos = this.servicoDeProduto.buscarTodosOsProdutos().subscribe({
      next: (produtosQueVieram) => {
        this.listaDeProdutos = produtosQueVieram;
        this.estaCarregandoOsProdutos = false;
      },
      error: () => {
        // o interceptador global ja mostra o toast de erro, aqui so paro o loading mesmo
        this.estaCarregandoOsProdutos = false;
      }
    });
  }

  apagarEsseProdutoAqui(produto: ProdutoDoEstoque): void {
    // confirm() nativo do navegador mesmo, sem frescura de modal customizado
    const usuarioTemCertezaMesmo = confirm(`Apagar o produto ${produto.codigo} - ${produto.descricao}?`);

    if (!usuarioTemCertezaMesmo) return;

    this.servicoDeProduto.apagarProduto(produto.codigo).subscribe({
      next: () => {
        this.notificacao.mostrarSucesso('Produto apagado.');
        this.recarregarListaDeProdutos();
      },
      error: () => { /* toast global ja avisa */ }
    });
  }
}
