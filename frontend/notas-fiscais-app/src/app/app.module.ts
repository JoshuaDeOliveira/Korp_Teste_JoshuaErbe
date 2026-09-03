import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

// Angular Material - biblioteca de componentes visuais usada no projeto
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';

import { ModuloDeRotas } from './app-routing.module';
import { ComponenteRaiz } from './app.component';

import { ComponenteListaDeProdutos } from './produtos/produto-lista/produto-lista.component';
import { ComponenteFormularioDeProduto } from './produtos/produto-form/produto-form.component';
import { ComponenteListaDeNotas } from './notas/nota-lista/nota-lista.component';
import { ComponenteFormularioDeNota } from './notas/nota-form/nota-form.component';

import { InterceptadorDeErros } from './nucleo/interceptadores/interceptador-de-erros';

@NgModule({
  declarations: [
    ComponenteRaiz,
    ComponenteListaDeProdutos,
    ComponenteFormularioDeProduto,
    ComponenteListaDeNotas,
    ComponenteFormularioDeNota
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    ModuloDeRotas,

    MatToolbarModule,
    MatButtonModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  providers: [
    // registra o interceptador global de erro http (RxJS catchError la dentro)
    { provide: HTTP_INTERCEPTORS, useClass: InterceptadorDeErros, multi: true }
  ],
  bootstrap: [ComponenteRaiz]
})
export class ModuloRaiz { }
