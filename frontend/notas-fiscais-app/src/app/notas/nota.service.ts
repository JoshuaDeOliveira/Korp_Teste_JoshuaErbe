import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NotaFiscal, ItemParaEnviar } from './nota.model';

// esse servico conversa com o microsservico de Faturamento
@Injectable({ providedIn: 'root' })
export class ServicoDeNotaFiscal {

  // porta do servico de faturamento (diferente da porta do estoque, cada um no seu quadrado)
  private enderecoBaseDaApi = 'http://localhost:5090/api/notas';

  constructor(private http: HttpClient) { }

  buscarTodasAsNotas(): Observable<NotaFiscal[]> {
    return this.http.get<NotaFiscal[]>(this.enderecoBaseDaApi);
  }

  criarNotaComItens(itens: ItemParaEnviar[]): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(this.enderecoBaseDaApi, { itens });
  }

  imprimirNota(numeroDaNota: number): Observable<NotaFiscal> {
    // gero uma chave de idempotencia aleatoria a cada clique novo no botao,
    // assim se o angular reenviar a MESMA requisicao (tipo um duplo clique
    // acidental) o backend sabe que é repetido e nao baixa o estoque 2x
    const chaveDeIdempotenciaDessaVez = 'imp-' + numeroDaNota + '-' + Date.now();

    const cabecalhosDaRequisicao = new HttpHeaders({
      'X-Chave-Idempotencia': chaveDeIdempotenciaDessaVez
    });

    return this.http.post<NotaFiscal>(`${this.enderecoBaseDaApi}/${numeroDaNota}/imprimir`, {}, { headers: cabecalhosDaRequisicao });
  }
}
