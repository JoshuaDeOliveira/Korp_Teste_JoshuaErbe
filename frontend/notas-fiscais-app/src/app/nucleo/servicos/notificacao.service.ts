import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

// servico bobinho so pra centralizar as mensagens de toast/snackbar
// (assim nao fico repetindo o mesmo bloco de codigo em cada componente)
@Injectable({ providedIn: 'root' })
export class ServicoDeNotificacao {

  constructor(private snackBarDoAngular: MatSnackBar) { }

  mostrarSucesso(mensagem: string): void {
    this.snackBarDoAngular.open(mensagem, 'fechar', { duration: 3000, panelClass: 'toast-sucesso' });
  }

  mostrarErro(mensagem: string): void {
    this.snackBarDoAngular.open(mensagem, 'fechar', { duration: 5000, panelClass: 'toast-erro' });
  }
}
