using System.Net.Http.Json;
using Servico.Faturamento.Excecoes;

namespace Servico.Faturamento.Clientes;

// classe responsavel por "falar" com o outro microsservico (Estoque) via http
// dei o nome de "Remoto" so pra deixar claro que é uma chamada de rede, nao local
public class ClienteEstoqueRemoto
{
    private readonly HttpClient _http;
    private readonly ILogger<ClienteEstoqueRemoto> _logger;

    public ClienteEstoqueRemoto(HttpClient http, ILogger<ClienteEstoqueRemoto> logger)
    {
        _http = http;
        _logger = logger;
    }

    // manda baixar o saldo de um produto la no estoque.
    // fiz um "retry" bem manual aqui (3 tentativas com um delayzinho) pra cobrir
    // o requisito de "tratamento de falha" do teste. Sei que da pra usar Polly
    // pra isso, mas fiz na unha mesmo pra mostrar que entendi o problema.
    public async Task BaixarSaldoDoProdutoAsync(string codigoProduto, int quantidade)
    {
        var corpoDaRequisicao = new { CodigoProduto = codigoProduto, QuantidadeParaBaixar = quantidade };

        Exception? ultimoErroQueRolou = null;

        for (int tentativaAtual = 1; tentativaAtual <= 3; tentativaAtual++)
        {
            try
            {
                var resposta = await _http.PostAsJsonAsync("/api/produtos/baixar-saldo", corpoDaRequisicao);

                if (resposta.IsSuccessStatusCode)
                {
                    return; // deu bom, sai fora
                }

                // se o proprio estoque respondeu com erro de negocio (ex: saldo insuficiente),
                // nao adianta tentar de novo, entao repassa o erro na hora
                if ((int)resposta.StatusCode is >= 400 and < 500)
                {
                    var textoDoErro = await resposta.Content.ReadAsStringAsync();
                    throw new EstoqueIndisponivelExcecao(textoDoErro);
                }

                ultimoErroQueRolou = new Exception($"estoque respondeu com status {resposta.StatusCode}");
            }
            catch (HttpRequestException ex)
            {
                // aqui cai quando o servico ta fora do ar mesmo (connection refused etc)
                _logger.LogWarning("tentativa {Tentativa} de falar com o estoque falhou: {Erro}", tentativaAtual, ex.Message);
                ultimoErroQueRolou = ex;
            }

            // espera um pouquinho antes de tentar de novo (backoff bem simplesinho)
            await Task.Delay(500 * tentativaAtual);
        }

        // se chegou aqui é pq nenhuma das 3 tentativas deu certo
        throw new EstoqueIndisponivelExcecao(ultimoErroQueRolou?.Message ?? "erro desconhecido");
    }
}
