using Microsoft.AspNetCore.Mvc;

namespace simpleapi.Controllers;

public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IWheatherService _wheather;

    public WeatherForecastController(
        ILogger<WeatherForecastController> logger,
        IWheatherService wheather
    )
    {
        _logger = logger;
        _wheather = wheather;
    }

    [HttpGet("weather")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _wheather.GetAll());
    }

    [HttpPost("weather/add")]
    public async Task<IActionResult> Add([FromBody] WeatherInputModel model)
    {
        return Ok(await _wheather.Add(model));
    }

    [HttpGet("weather/{id}")]
    public async Task<IActionResult> Add(string id)
    {
        var res = await _wheather.Get(id);
        if (res is null)
        {
            return BadRequest();
        }
        return Ok(res);
    }

    [HttpPatch("weather")]
    public async Task<IActionResult> Patch([FromBody] WeatherPathInputModel model)
    {
        var res = await _wheather.Patch(model);
        if (res is null)
        {
            return BadRequest();
        }

        return Ok(res);
    }
}
