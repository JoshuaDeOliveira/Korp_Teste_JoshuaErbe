import { Component } from '@angular/core';

// esse é o componente raiz, so tem o menu de navegacao e o router-outlet
@Component({
  selector: 'app-raiz',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class ComponenteRaiz {
  tituloDoSistema = 'Sistema de Notas Fiscais (Teste Korp)';
}
