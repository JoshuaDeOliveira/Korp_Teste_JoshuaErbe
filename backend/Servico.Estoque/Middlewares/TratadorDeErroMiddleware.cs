using System.Net;
using System.Text.Json;
using Servico.Estoque.Excecoes;

namespace Servico.Estoque.Middlewares;

// middleware "pega tudo" de erro. Sei que da pra fazer mais bonito com
// ProblemDetails do proprio asp.net, mas fiz na mao mesmo pra entender o fluxo.
public class TratadorDeErroMiddleware
{
    private readonly RequestDelegate _proximo;
    private readonly ILogger<TratadorDeErroMiddleware> _logger;

    public TratadorDeErroMiddleware(RequestDelegate proximo, ILogger<TratadorDeErroMiddleware> logger)
    {
        _proximo = proximo;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _proximo(contexto);
        }
        catch (ProdutoNaoEncontradoExcecao ex)
        {
            _logger.LogWarning(ex, "produto nao encontrado");
            await EscreverErroAsync(contexto, HttpStatusCode.NotFound, ex.Message);
        }
        catch (SaldoInsuficienteExcecao ex)
        {
            _logger.LogWarning(ex, "saldo insuficiente");
            await EscreverErroAsync(contexto, HttpStatusCode.Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            // aqui é o "deu ruim geral", loga tudo pra poder debugar depois
            _logger.LogError(ex, "erro nao mapeado no servico de estoque");
            await EscreverErroAsync(contexto, HttpStatusCode.InternalServerError,
                "Deu algum erro estranho aqui no estoque, tenta de novo mais tarde.");
        }
    }

    private static async Task EscreverErroAsync(HttpContext contexto, HttpStatusCode status, string mensagem)
    {
        contexto.Response.ContentType = "application/json";
        contexto.Response.StatusCode = (int)status;

        var corpoDoErro = new
        {
            sucesso = false,
            mensagem,
            dataDoErro = DateTime.Now
        };

        await contexto.Response.WriteAsync(JsonSerializer.Serialize(corpoDoErro));
    }
}
