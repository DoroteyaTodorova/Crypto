using CryptoBackend.Dto;
using CryptoBackend.Interface;
using Microsoft.AspNetCore.Mvc;

namespace CryptoBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;
        private readonly ILogger<PortfolioController> _logger;

        public PortfolioController(
            IPortfolioService portfolioService,
            ILogger<PortfolioController> logger)
        {
            _portfolioService = portfolioService;
            _logger = logger;
        }

        [HttpPost("calculate")]
        [ProducesResponseType(typeof(List<PortfolioResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadPortfolioJson([FromBody] PortfolioRequest request)
        {
            if (request?.Portfolio?.Any() != true)
            {
                _logger.LogWarning("Received empty or null portfolio request.");
                return BadRequest("Portfolio is empty or null.");
            }

            try
            {
                _logger.LogInformation("Processing portfolio calculation for {Count} items.", request.Portfolio.Count);
                var results = await _portfolioService.CalculatePortfolioAsync(request);
                _logger.LogInformation("Portfolio calculation completed successfully.");
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calculating portfolio.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
