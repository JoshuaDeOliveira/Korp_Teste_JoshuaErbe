import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ComponenteListaDeProdutos } from './produtos/produto-lista/produto-lista.component';
import { ComponenteFormularioDeProduto } from './produtos/produto-form/produto-form.component';
import { ComponenteListaDeNotas } from './notas/nota-lista/nota-lista.component';
import { ComponenteFormularioDeNota } from './notas/nota-form/nota-form.component';

// rotas bem diretas, nada de lazy loading pq o sistema é pequeno mesmo
const rotasDoSistema: Routes = [
  { path: '', redirectTo: 'produtos', pathMatch: 'full' },
  { path: 'produtos', component: ComponenteListaDeProdutos },
  { path: 'produtos/novo', component: ComponenteFormularioDeProduto },
  { path: 'notas', component: ComponenteListaDeNotas },
  { path: 'notas/nova', component: ComponenteFormularioDeNota }
];

@NgModule({
  imports: [RouterModule.forRoot(rotasDoSistema)],
  exports: [RouterModule]
})
export class ModuloDeRotas { }
