import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ServicoDeProduto } from '../produto.service';
import { ServicoDeNotificacao } from '../../nucleo/servicos/notificacao.service';

@Component({
  selector: 'app-form-produto',
  templateUrl: './produto-form.component.html',
  styleUrls: ['./produto-form.component.css']
})
export class ComponenteFormularioDeProduto {

  formularioDoProduto: FormGroup;
  estaSalvando = false;

  constructor(
    private construtorDeFormulario: FormBuilder,
    private servicoDeProduto: ServicoDeProduto,
    private notificacao: ServicoDeNotificacao,
    private roteador: Router
  ) {
    this.formularioDoProduto = this.construtorDeFormulario.group({
      codigo: ['', Validators.required],
      descricao: ['', Validators.required],
      saldoAtual: [0, [Validators.required, Validators.min(0)]]
    });
  }

  salvarProduto(): void {
    if (this.formularioDoProduto.invalid) {
      this.formularioDoProduto.markAllAsTouched();
      return;
    }

    this.estaSalvando = true;

    this.servicoDeProduto.cadastrarNovoProduto(this.formularioDoProduto.value).subscribe({
      next: () => {
        this.notificacao.mostrarSucesso('Produto cadastrado com sucesso!');
        this.roteador.navigate(['/produtos']);
      },
      error: () => {
        // o toast de erro ja sobe pelo interceptador global
        this.estaSalvando = false;
      }
    });
  }
}
