using Microsoft.EntityFrameworkCore;
using Servico.Faturamento.Clientes;
using Servico.Faturamento.Dados;
using Servico.Faturamento.Excecoes;
using Servico.Faturamento.Idempotencia;
using Servico.Faturamento.Middlewares;
using Servico.Faturamento.Modelos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FaturamentoContexto>(opcoes =>
    opcoes.UseSqlite(builder.Configuration.GetConnectionString("BancoDeDadosFaturamento")));

// registra o HttpClient que fala com o servico de estoque.
// o endereco vem do appsettings pra nao ficar chumbado no codigo
builder.Services.AddHttpClient<ClienteEstoqueRemoto>(cliente =>
{
    var enderecoDoEstoque = builder.Configuration["EnderecoDoServicoDeEstoque"]!;
    cliente.BaseAddress = new Uri(enderecoDoEstoque);
    cliente.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddSingleton<CacheDeRequisicoesRepetidas>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(opcoes =>
{
    opcoes.AddPolicy("PermiteQualquerCoisaPorEnquanto", politica =>
    {
        politica.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

using (var escopo = app.Services.CreateScope())
{
    var contexto = escopo.ServiceProvider.GetRequiredService<FaturamentoContexto>();
    contexto.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("PermiteQualquerCoisaPorEnquanto");
app.UseMiddleware<TratadorDeErroMiddleware>();

// ===== ENDPOINTS =====

// lista as notas, mais novas primeiro (LINQ orderby descendente)
app.MapGet("/api/notas", async (FaturamentoContexto db) =>
{
    var notas = await db.Notas
        .Include(n => n.Itens)
        .OrderByDescending(n => n.NumeroSequencial)
        .ToListAsync();

    return Results.Ok(notas);
});

app.MapGet("/api/notas/{numero:int}", async (int numero, FaturamentoContexto db) =>
{
    var nota = await db.Notas
        .Include(n => n.Itens)
        .FirstOrDefaultAsync(n => n.NumeroSequencial == numero);

    if (nota is null)
        throw new NotaNaoEncontradaExcecao(numero);

    return Results.Ok(nota);
});

// cria nota nova, com numeracao sequencial automatica
app.MapPost("/api/notas", async (NotaFiscalEntradaDto dto, FaturamentoContexto db) =>
{
    if (dto.Itens is null || dto.Itens.Count == 0)
        return Results.BadRequest(new { sucesso = false, mensagem = "A nota precisa ter pelo menos 1 produto." });

    // pega o maior numero que ja existe e soma 1 (se nao tiver nenhuma nota, comeca do 1)
    var proximoNumero = await db.Notas.AnyAsync()
        ? await db.Notas.MaxAsync(n => n.NumeroSequencial) + 1
        : 1;

    var notaNovinha = new NotaFiscalCabecalho
    {
        NumeroSequencial = proximoNumero,
        Status = StatusDaNota.Aberta,
        Itens = dto.Itens.Select(itemDto => new ItemDaNotaFiscal
        {
            CodigoDoProduto = itemDto.CodigoDoProduto,
            DescricaoDoProdutoNaHoraDaVenda = itemDto.DescricaoDoProdutoNaHoraDaVenda,
            Quantidade = itemDto.Quantidade
        }).ToList()
    };

    db.Notas.Add(notaNovinha);
    await db.SaveChangesAsync();

    return Results.Created($"/api/notas/{notaNovinha.NumeroSequencial}", notaNovinha);
});

// o endpoint principal: IMPRIMIR a nota.
// aqui que a magica (ou a desgraça, dependendo do dia) acontece
app.MapPost("/api/notas/{numero:int}/imprimir", async (
    int numero,
    HttpRequest requisicaoHttp,
    FaturamentoContexto db,
    ClienteEstoqueRemoto clienteDoEstoque,
    CacheDeRequisicoesRepetidas cacheIdempotente) =>
{
    // chave de idempotencia: se o front mandar o mesmo header duas vezes,
    // a gente so processa a primeira (evita duplo-clique fazendo baixa dupla no estoque)
    var chaveIdempotente = requisicaoHttp.Headers["X-Chave-Idempotencia"].FirstOrDefault();

    if (!string.IsNullOrEmpty(chaveIdempotente) && cacheIdempotente.JaProcessouEssaChave(chaveIdempotente))
    {
        return Results.Ok(new { sucesso = true, mensagem = "Essa impressao ja tinha sido processada antes (idempotencia ativada)." });
    }

    var nota = await db.Notas.Include(n => n.Itens).FirstOrDefaultAsync(n => n.NumeroSequencial == numero);

    if (nota is null)
        throw new NotaNaoEncontradaExcecao(numero);

    // regra obrigatoria: so pode imprimir nota Aberta
    if (nota.Status != StatusDaNota.Aberta)
        throw new NotaJaFechadaExcecao(numero);

    // vai la no servico de estoque e baixa cada item da nota.
    // se algum item der erro (estoque fora do ar, saldo insuficiente etc),
    // a excecao sobe e a nota CONTINUA aberta (nao fecha nada pela metade)
    foreach (var itemDaVez in nota.Itens)
    {
        await clienteDoEstoque.BaixarSaldoDoProdutoAsync(itemDaVez.CodigoDoProduto, itemDaVez.Quantidade);
    }

    nota.Status = StatusDaNota.Fechada;
    nota.FechadaEm = DateTime.Now;
    await db.SaveChangesAsync();

    if (!string.IsNullOrEmpty(chaveIdempotente))
        cacheIdempotente.MarcarComoProcessada(chaveIdempotente);

    return Results.Ok(nota);
});

app.Run();
