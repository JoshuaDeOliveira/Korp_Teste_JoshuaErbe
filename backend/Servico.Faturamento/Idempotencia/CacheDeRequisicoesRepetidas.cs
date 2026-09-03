using System.Collections.Concurrent;

namespace Servico.Faturamento.Idempotencia;

// cache bem simples em memoria pra evitar que a mesma requisicao de "imprimir"
// seja processada duas vezes se o usuario ficar clicando/apertando F5.
// obs: em memoria mesmo, sei que se reiniciar o servico perde tudo, mas
// pro escopo do teste ta de bom tamanho.
public class CacheDeRequisicoesRepetidas
{
    private readonly ConcurrentDictionary<string, DateTime> _chavesJaProcessadas = new();

    public bool JaProcessouEssaChave(string chaveDeIdempotencia)
    {
        return _chavesJaProcessadas.ContainsKey(chaveDeIdempotencia);
    }

    public void MarcarComoProcessada(string chaveDeIdempotencia)
    {
        _chavesJaProcessadas[chaveDeIdempotencia] = DateTime.Now;
    }
}
