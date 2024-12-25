using Microsoft.AspNetCore.Mvc;

namespace CTE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CteController : ControllerBase
{
    [HttpPost("calcular")]
    public IActionResult CalcularCte([FromBody] CteRequest request)
    {
        try
        {
            // Validar os dados da operação
            if (string.IsNullOrEmpty(request.Origem) || string.IsNullOrEmpty(request.Destino))
                return BadRequest("Origem e destino devem ser informados.");

            if (request.DistanciaKm <= 0)
                return BadRequest("A distância deve ser maior que zero.");

            if (request.PesoCarga <= 0 || request.TarifaPorPeso <= 0)
                return BadRequest("Peso e tarifa por peso devem ser maiores que zero.");

            // Calcular o valor do frete
            var valorFrete = (request.TarifaPorPeso * request.PesoCarga) + request.DespesasAdicionais;

            // Calcular a base de cálculo do ICMS
            var baseCalculoIcms = valorFrete / (1 - request.AliquotaIcms / 100);

            // Calcular o valor do ICMS
            var valorIcms = baseCalculoIcms * (request.AliquotaIcms / 100);

            // Calcular o valor total do CT-e
            var valorTotalCte = valorFrete + valorIcms;

            // Retornar os valores calculados e informações adicionais
            var response = new CteResponse
            {
                ValorFrete = valorFrete,
                ValorIcms = valorIcms,
                ValorTotalCte = valorTotalCte,
                Informacoes = new
                {
                    request.Quantidade,
                    request.Volume,
                    request.Origem,
                    request.Destino,
                    request.DistanciaKm,
                    request.InicioOperacao
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

// Modelo de requisição
public class CteRequest
{
    // Dados da carga
    public decimal TarifaPorPeso { get; set; } // Tarifa por tonelada
    public decimal PesoCarga { get; set; } // Peso da carga em toneladas
    public decimal DespesasAdicionais { get; set; } // Pedágios e taxas adicionais
    public decimal AliquotaIcms { get; set; } // Alíquota do ICMS em %

    // Dados da operação
    public int Quantidade { get; set; } // Quantidade de itens transportados
    public decimal Volume { get; set; } // Volume total em m³
    public string Origem { get; set; } // Cidade de origem
    public string Destino { get; set; } // Cidade de destino
    public double DistanciaKm { get; set; } // Distância em quilômetros
    public DateTime InicioOperacao { get; set; } // Data e hora de início
}

// Modelo de resposta
public class CteResponse
{
    public decimal ValorFrete { get; set; }
    public decimal ValorIcms { get; set; }
    public decimal ValorTotalCte { get; set; }
    public object Informacoes { get; set; } // Dados adicionais sobre a operação
}