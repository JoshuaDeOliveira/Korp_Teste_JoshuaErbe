# Korp_Teste_JoshuaErbe

Sistema de emissão de Notas Fiscais feito para o teste técnico da Korp. Composto por **2 microsserviços em C#/.NET 8** (Estoque e Faturamento) e um **front-end em Angular 17**.

## Como rodar

### Pré-requisitos
- .NET SDK 8.0
- Node.js 18+ e Angular CLI (`npm install -g @angular/cli`)

### 1. Subir o Servico.Estoque
```bash
cd backend/Servico.Estoque
dotnet restore
dotnet run --urls=http://localhost:5050
```
O banco `estoque_dados.db` (SQLite) é criado automaticamente na primeira execução.

### 2. Subir o Servico.Faturamento
Em outro terminal:
```bash
cd backend/Servico.Faturamento
dotnet restore
dotnet run --urls=http://localhost:5090
```
Confira em `appsettings.json` se `EnderecoDoServicoDeEstoque` aponta para `http://localhost:5050`.

### 3. Subir o front-end Angular
Em outro terminal:
```bash
cd frontend/notas-fiscais-app
npm install
ng serve
```
Acesse `http://localhost:4200`.

### Ordem de uso sugerida na demo
1. Cadastrar 1 ou 2 produtos em **Produtos > Novo Produto**.
2. Criar uma nota em **Notas Fiscais > Nova Nota**, escolhendo produto(s) e quantidade(s).
3. Na lista de notas, clicar em **Imprimir Nota**: o botão mostra o spinner de carregando, a nota vira "Fechada" e o saldo do produto é atualizado.
4. Tentar clicar em "Imprimir" de novo na mesma nota -> o botão fica desabilitado (regra: só imprime nota Aberta).
5. Para mostrar o cenário de falha: parar o `Servico.Estoque` (Ctrl+C no terminal dele) e tentar imprimir uma nota nova -> aparece o toast de erro vindo do backend (status 503) e a nota continua Aberta, sem quebrar nada. Depois é só subir o serviço de novo e imprimir com sucesso.

---

## Detalhamento técnico

### Ciclos de vida do Angular utilizados
- **`ngOnInit`**: usado em `ComponenteListaDeProdutos`, `ComponenteListaDeNotas` e `ComponenteFormularioDeNota` para carregar os dados da API assim que o componente é montado.
- **`ngOnDestroy`**: usado em `ComponenteListaDeProdutos` e `ComponenteListaDeNotas` para cancelar (`unsubscribe`) as subscriptions RxJS abertas e evitar vazamento de memória quando o usuário sai da tela.

### Uso do RxJS
Usado em vários pontos:
- **`Observable` + `.subscribe()`**: toda comunicação HTTP com os dois microsserviços (via `HttpClient`) retorna `Observable`.
- **`catchError`** (`interceptador-de-erros.ts`): um `HttpInterceptor` global captura qualquer erro HTTP de qualquer chamada da aplicação e dispara um toast de notificação — assim não é preciso tratar erro em cada componente separadamente.
- **`finalize`**: usado em `nota-lista.component.ts` para garantir que o spinner de "imprimindo..." pare de girar independente da chamada ter dado certo ou erro.
- **`Subscription`**: gerenciamento manual de inscrições, canceladas no `ngOnDestroy`.

### Outras bibliotecas utilizadas (front-end)
- **Angular Material + Angular CDK**: biblioteca de componentes visuais (tabelas, cards, botões, spinners, snackbar, form-fields, select). É a biblioteca de componentes visuais usada no projeto.
- **Angular Forms (Reactive Forms + ngModel)**: `ReactiveFormsModule` no cadastro de produto (com `Validators`), e `ngModel` no formulário mais simples de nota fiscal.
- **RxJS**: já detalhado acima (vem junto com o Angular).

### Gerenciamento de dependências no back-end
O back-end foi feito em **C#**, não em Golang, então o gerenciamento de dependências foi feito via **NuGet**, através dos arquivos `.csproj` de cada microsserviço (`Servico.Estoque.csproj` e `Servico.Faturamento.csproj`), com os pacotes:
- `Microsoft.EntityFrameworkCore.Sqlite` — ORM + banco de dados.
- `Microsoft.EntityFrameworkCore.Design` — geração de banco.
- `Swashbuckle.AspNetCore` — Swagger/OpenAPI pra documentar e testar os endpoints.

### Framework utilizado
**ASP.NET Core 8 (Minimal APIs)**, com **Entity Framework Core** como ORM e **SQLite** como banco de dados físico (arquivo `.db` local, criado automaticamente via `EnsureCreated()`).

### Tratamento de erros e exceções no back-end
- Exceções de negócio customizadas: `ProdutoNaoEncontradoExcecao`, `SaldoInsuficienteExcecao` (Estoque) e `NotaNaoEncontradaExcecao`, `NotaJaFechadaExcecao`, `EstoqueIndisponivelExcecao` (Faturamento).
- Um **middleware global** (`TratadorDeErroMiddleware`) em cada serviço captura essas exceções no pipeline do ASP.NET Core e converte em respostas HTTP com status apropriado (404, 409, 503, 500), sempre devolvendo um JSON padronizado com `sucesso` e `mensagem`.
- **Cenário de falha entre microsserviços**: o `ClienteEstoqueRemoto` (dentro do Faturamento) faz até 3 tentativas com backoff simples ao chamar o Estoque; se todas falharem, lança `EstoqueIndisponivelExcecao`, que vira um HTTP 503 — a nota **permanece Aberta** (não é fechada pela metade) e o front-end mostra a mensagem de erro via toast, permitindo tentar imprimir de novo depois que o serviço voltar.

### Uso de LINQ
usado no back-end C#, por exemplo:
- `db.Produtos.OrderBy(p => p.Codigo).ToListAsync()` — listagem ordenada de produtos.
- `db.Produtos.FirstOrDefaultAsync(p => p.Codigo == codigo)` — busca por código.
- `db.Notas.MaxAsync(n => n.NumeroSequencial)` — cálculo do próximo número sequencial da nota.
- `dto.Itens.Select(itemDto => new ItemDaNotaFiscal {...}).ToList()` — conversão de DTO de entrada para entidade.

### Deletar produto
Endpoint `DELETE /api/produtos/{codigo}` no `Servico.Estoque`, com botão de lixeira na tela de listagem de produtos (com confirmação antes de apagar). Não foi feita nenhuma checagem cruzada com o `Servico.Faturamento` porque cada nota já guarda uma cópia da descrição do produto no momento da venda (`DescricaoDoProdutoNaHoraDaVenda`), então apagar um produto do estoque não quebra o histórico de notas já emitidas — isso é inclusive um reflexo de cada microsserviço ter seu próprio banco, sem foreign key entre eles.

### Itens opcionais implementados
- **Idempotência**: header `X-Chave-Idempotencia` enviado pelo front no momento da impressão; o Faturamento guarda em cache (`CacheDeRequisicoesRepetidas`, em memória) as chaves já processadas para não duplicar a baixa de estoque em caso de clique duplo/reenvio.
- **Concorrência**: implementado de forma simples, com um `SemaphoreSlim` no Servico.Estoque protegendo o trecho que lê e decrementa o saldo, evitando que duas notas simultâneas "furem" o saldo de um produto com 1 unidade. É uma solução básica (lock em memória de uma única instância), não um controle distribuído — daria pra evoluir usando concorrência otimista do próprio EF Core (`RowVersion`), mas para o escopo do teste ficou assim.
- **IA**: não implementado por falta de tempo dentro do prazo do teste.
