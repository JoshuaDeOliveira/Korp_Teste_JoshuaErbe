import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription, finalize } from 'rxjs';
import { NotaFiscal, StatusDaNotaNoFront } from '../nota.model';
import { ServicoDeNotaFiscal } from '../nota.service';
import { ServicoDeNotificacao } from '../../nucleo/servicos/notificacao.service';

@Component({
  selector: 'app-lista-notas',
  templateUrl: './nota-lista.component.html',
  styleUrls: ['./nota-lista.component.css']
})
export class ComponenteListaDeNotas implements OnInit, OnDestroy {

  listaDeNotas: NotaFiscal[] = [];
  estaCarregandoAsNotas = false;

  // controla qual nota especifica ta com o botao de imprimir "girando" no momento
  // (uso um Set pq pode ter mais de uma pessoa/aba imprimindo notas diferentes ao mesmo tempo)
  numerosDeNotaImprimindoAgora = new Set<number>();

  StatusDaNotaNoFront = StatusDaNotaNoFront; // exponho o enum pro template poder usar

  private inscricoesAtivas: Subscription[] = [];

  constructor(
    private servicoDeNota: ServicoDeNotaFiscal,
    private notificacao: ServicoDeNotificacao
  ) { }

  ngOnInit(): void {
    this.recarregarListaDeNotas();
  }

  ngOnDestroy(): void {
    this.inscricoesAtivas.forEach(inscricao => inscricao.unsubscribe());
  }

  recarregarListaDeNotas(): void {
    this.estaCarregandoAsNotas = true;

    const inscricao = this.servicoDeNota.buscarTodasAsNotas().pipe(
      finalize(() => this.estaCarregandoAsNotas = false) // finalize do RxJS: roda sempre, deu certo ou nao
    ).subscribe({
      next: (notasQueVieram) => this.listaDeNotas = notasQueVieram,
      error: () => { /* toast global ja cuida de avisar */ }
    });

    this.inscricoesAtivas.push(inscricao);
  }

  produtoEstaAberta(nota: NotaFiscal): boolean {
    return nota.status === StatusDaNotaNoFront.Aberta;
  }

  imprimirEssaNotaAqui(nota: NotaFiscal): void {
    // trava dupla-tentativa de imprimir a mesma nota clicando 2x rapido
    if (this.numerosDeNotaImprimindoAgora.has(nota.numeroSequencial)) {
      return;
    }

    this.numerosDeNotaImprimindoAgora.add(nota.numeroSequencial);

    const inscricao = this.servicoDeNota.imprimirNota(nota.numeroSequencial).pipe(
      finalize(() => this.numerosDeNotaImprimindoAgora.delete(nota.numeroSequencial))
    ).subscribe({
      next: (notaAtualizada) => {
        this.notificacao.mostrarSucesso(`Nota ${notaAtualizada.numeroSequencial} impressa e fechada com sucesso!`);
        this.recarregarListaDeNotas(); // atualiza a tabela pra refletir o novo status
      },
      error: () => {
        // se o estoque tiver caido (erro 503) o toast global ja mostra a mensagem
        // certinha vinda do backend, so precisamos parar o spinner mesmo (feito no finalize)
      }
    });

    this.inscricoesAtivas.push(inscricao);
  }
}
