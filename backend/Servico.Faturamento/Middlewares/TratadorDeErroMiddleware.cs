using System.Net;
using System.Text.Json;
using Servico.Faturamento.Excecoes;

namespace Servico.Faturamento.Middlewares;

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
        catch (NotaNaoEncontradaExcecao ex)
        {
            await EscreverErroAsync(contexto, HttpStatusCode.NotFound, ex.Message, ex);
        }
        catch (NotaJaFechadaExcecao ex)
        {
            await EscreverErroAsync(contexto, HttpStatusCode.Conflict, ex.Message, ex);
        }
        catch (EstoqueIndisponivelExcecao ex)
        {
            // esse é o cenario de "microsservico caiu" -> devolve 503 pro front
            // conseguir mostrar um aviso decente pro usuario, sem quebrar a nota
            await EscreverErroAsync(contexto, HttpStatusCode.ServiceUnavailable, ex.Message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "erro nao mapeado no servico de faturamento");
            await EscreverErroAsync(contexto, HttpStatusCode.InternalServerError,
                "Deu ruim aqui no faturamento, tenta de novo.", ex);
        }
    }

    private async Task EscreverErroAsync(HttpContext contexto, HttpStatusCode status, string mensagem, Exception ex)
    {
        if (status == HttpStatusCode.ServiceUnavailable)
            _logger.LogWarning(ex, "estoque indisponivel");

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
