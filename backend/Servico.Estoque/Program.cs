using Microsoft.EntityFrameworkCore;
using Servico.Estoque.Dados;
using Servico.Estoque.Excecoes;
using Servico.Estoque.Middlewares;
using Servico.Estoque.Modelos;

var builder = WebApplication.CreateBuilder(args);

// registra o EF com sqlite (banco de verdade em arquivo, bem simples de rodar)
builder.Services.AddDbContext<EstoqueContexto>(opcoes =>
    opcoes.UseSqlite(builder.Configuration.GetConnectionString("BancoDeDadosEstoque")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// libera o cors geral pra nao dar dor de cabeça com o Angular em outra porta
builder.Services.AddCors(opcoes =>
{
    opcoes.AddPolicy("PermiteQualquerCoisaPorEnquanto", politica =>
    {
        politica.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// cria o banco automatico se nao existir (jeito rapido, sem migrations chatas)
using (var escopo = app.Services.CreateScope())
{
    var contexto = escopo.ServiceProvider.GetRequiredService<EstoqueContexto>();
    contexto.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("PermiteQualquerCoisaPorEnquanto");

// meu middleware de erro customizado entra bem no comecinho do pipeline
app.UseMiddleware<TratadorDeErroMiddleware>();

// ===== ENDPOINTS =====

// lista todo mundo (com um LINQ bem basico de ordenacao)
app.MapGet("/api/produtos", async (EstoqueContexto db) =>
{
    var listaOrdenadinha = await db.Produtos
        .OrderBy(p => p.Codigo)
        .ToListAsync();

    return Results.Ok(listaOrdenadinha);
});

// pega um produto especifico pelo codigo
app.MapGet("/api/produtos/{codigo}", async (string codigo, EstoqueContexto db) =>
{
    var produtoAchado = await db.Produtos.FirstOrDefaultAsync(p => p.Codigo == codigo);

    if (produtoAchado is null)
        throw new ProdutoNaoEncontradoExcecao(codigo);

    return Results.Ok(produtoAchado);
});

// cadastra produto novo
app.MapPost("/api/produtos", async (ProdutoEstoqueEntradaDto dto, EstoqueContexto db) =>
{
    var jaExisteEsseCodigo = await db.Produtos.AnyAsync(p => p.Codigo == dto.Codigo);
    if (jaExisteEsseCodigo)
        return Results.BadRequest(new { sucesso = false, mensagem = "Ja existe produto com esse codigo, viu?" });

    var produtoNovinho = new ProdutoEstoque
    {
        Codigo = dto.Codigo,
        Descricao = dto.Descricao,
        SaldoAtual = dto.SaldoAtual
    };

    db.Produtos.Add(produtoNovinho);
    await db.SaveChangesAsync();

    return Results.Created($"/api/produtos/{produtoNovinho.Codigo}", produtoNovinho);
});

// endpoint interno chamado pelo servico de faturamento na hora de imprimir a nota
// (baixa o saldo do produto). Coloquei um lock bem tosco pra tentar evitar
// concorrencia bagunçando o saldo quando duas notas mexem no mesmo produto ao mesmo tempo.
var travaDeConcorrenciaTosca = new SemaphoreSlim(1, 1);

app.MapPost("/api/produtos/baixar-saldo", async (ProdutoEstoqueBaixaDto dto, EstoqueContexto db) =>
{
    await travaDeConcorrenciaTosca.WaitAsync();
    try
    {
        var produtoAlvo = await db.Produtos.FirstOrDefaultAsync(p => p.Codigo == dto.CodigoProduto);

        if (produtoAlvo is null)
            throw new ProdutoNaoEncontradoExcecao(dto.CodigoProduto);

        if (produtoAlvo.SaldoAtual < dto.QuantidadeParaBaixar)
            throw new SaldoInsuficienteExcecao(produtoAlvo.Codigo, produtoAlvo.SaldoAtual, dto.QuantidadeParaBaixar);

        produtoAlvo.SaldoAtual -= dto.QuantidadeParaBaixar;
        await db.SaveChangesAsync();

        return Results.Ok(produtoAlvo);
    }
    finally
    {
        // sempre solta a trava, senao trava o servico inteiro pra sempre (quase fiz isso no teste kkk)
        travaDeConcorrenciaTosca.Release();
    }
});

// endpoint pra simular o servico caindo (usei isso pra testar o cenario de falha pedido no teste)
app.MapPost("/api/produtos/simular-servico-de-pau", () =>
{
    throw new Exception("Falha proposital pra testar resiliencia (endpoint de teste mesmo).");
});

app.Run();
