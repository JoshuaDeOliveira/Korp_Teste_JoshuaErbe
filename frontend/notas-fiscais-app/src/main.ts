import 'zone.js'; // precisa vir ANTES de qualquer outro import, senao da erro NG0908 na hora de subir
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { ModuloRaiz } from './app/app.module';

platformBrowserDynamic().bootstrapModule(ModuloRaiz)
  .catch(erroNaInicializacao => console.error('deu erro subindo a aplicacao:', erroNaInicializacao));
