namespace simpleapi;

public enum SupportedTempTypes
{
    Celsius,
    Fahrenheit,
}

public class WeatherForecast
{
    public DateTime Date { get; set; }
    public string Id { get; set; }
    private double _tempC = 0;
    private double _tempF = 0;
    public double TemperatureC
    {
        get => _tempC;
        set
        {
            _tempC = value;
            _tempF = double.Round(32 + value / 0.5556, 2);
        }
    }

    public double TemperatureF
    {
        get => _tempF;
        set
        {
            _tempF = value;
            _tempC = double.Round((value - 32) * 0.5556, 2);
        }
    }

    public WeatherForecast()
    {
        Id = Tools.GenerateRandomString(32, "W-");
    }

    public WeatherForecast(string id)
    {
        Id = id;
    }
}

public class WeatherInputModel
{
    public DateOnly Date { get; set; }
    public double Temperature { get; set; }
    public string Type { get; set; } = SupportedTempTypes.Celsius.ToString();

    public override string ToString()
    {
        return $"{Temperature} {Enum.Parse<SupportedTempTypes>(Type)} : {Date}";
    }
}

public class WeatherPathInputModel
{
    public string Id { get; set; } = "";
    public double Temperature { get; set; }
    public string Type { get; set; } = SupportedTempTypes.Celsius.ToString();

    public override string ToString()
    {
        return $"{Temperature} {Enum.Parse<SupportedTempTypes>(Type)}";
    }
}
