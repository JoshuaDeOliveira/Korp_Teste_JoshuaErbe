import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProdutoDoEstoque, ProdutoParaCadastrar } from './produto.model';

// esse servico so conversa com o microsservico de Estoque
@Injectable({ providedIn: 'root' })
export class ServicoDeProduto {

  // troquei pra porta do servico de estoque, ajusta se rodar em outra porta ai na sua maquina
  private enderecoBaseDaApi = 'http://localhost:5050/api/produtos';

  constructor(private http: HttpClient) { }

  buscarTodosOsProdutos(): Observable<ProdutoDoEstoque[]> {
    return this.http.get<ProdutoDoEstoque[]>(this.enderecoBaseDaApi);
  }

  cadastrarNovoProduto(produtoNovo: ProdutoParaCadastrar): Observable<ProdutoDoEstoque> {
    return this.http.post<ProdutoDoEstoque>(this.enderecoBaseDaApi, produtoNovo);
  }

  apagarProduto(codigo: string): Observable<void> {
    return this.http.delete<void>(`${this.enderecoBaseDaApi}/${codigo}`);
  }
}
