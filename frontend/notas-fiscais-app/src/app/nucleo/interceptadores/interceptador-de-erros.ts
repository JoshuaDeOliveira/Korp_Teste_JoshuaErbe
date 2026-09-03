import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ServicoDeNotificacao } from '../servicos/notificacao.service';

// interceptor que pega QUALQUER erro http da aplicacao inteira e mostra um toast.
// assim eu nao preciso ficar fazendo try/catch (ou .subscribe com erro) em cada
// chamadinha, fica tudo centralizado aqui (usei RxJS catchError pra isso)
@Injectable()
export class InterceptadorDeErros implements HttpInterceptor {

  constructor(private notificacao: ServicoDeNotificacao) { }

  intercept(requisicao: HttpRequest<unknown>, proximoNaFila: HttpHandler): Observable<HttpEvent<unknown>> {
    return proximoNaFila.handle(requisicao).pipe(
      catchError((erroCapturado: HttpErrorResponse) => {

        // o backend em C# sempre devolve um json com "mensagem", entao tento usar ele
        const mensagemDoBackend = erroCapturado.error?.mensagem;

        if (erroCapturado.status === 0) {
          this.notificacao.mostrarErro('Nao consegui falar com o servidor. Ele ta rodando?');
        } else if (erroCapturado.status === 503) {
          // caso especifico do cenario de "microsservico fora do ar"
          this.notificacao.mostrarErro(mensagemDoBackend ?? 'Um dos servicos ta indisponivel agora.');
        } else {
          this.notificacao.mostrarErro(mensagemDoBackend ?? 'Aconteceu um erro inesperado.');
        }

        return throwError(() => erroCapturado);
      })
    );
  }
}
