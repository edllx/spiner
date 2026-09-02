using Npgsql;

namespace simpleapi;

public interface IWheatherService
{
    Task<WeatherForecast> Add(WeatherInputModel model);
    Task<IEnumerable<WeatherForecast>> GetAll();
    Task<WeatherForecast?> Get(string id);
    Task<WeatherForecast?> Patch(WeatherPathInputModel model);
}

public class WheatherService(IDataContext db) : IWheatherService
{
    public async Task<WeatherForecast> Add(WeatherInputModel model)
    {
        WeatherForecast res = new() { Date = DateTime.Now };
        string query = """
            INSERT INTO weathers(temperature,type,id) 
            VALUES(@Temperature,@Type,@Id);
            """;
        try
        {
            using NpgsqlConnection con = (NpgsqlConnection)db.CreateConnection();
            using NpgsqlCommand cmd = new(query, con);
            cmd.Parameters.AddWithValue("@Temperature", model.Temperature);
            cmd.Parameters.AddWithValue("@Type", model.Type);
            cmd.Parameters.AddWithValue("@Id", res.Id);
            db.ExecuteNonQuery(query, cmd);

            switch (model.Type)
            {
                case nameof(SupportedTempTypes.Fahrenheit):
                    res.TemperatureF = model.Temperature;
                    break;
                default:

                    res.TemperatureC = model.Temperature;
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        return await Task.FromResult(res);
    }

    public async Task<WeatherForecast?> Get(string id)
    {
        string query = """
            SELECT 
              temperature,
              type,
              id
            FROM weathers
            WHERE id = @Id;
            """;
        try
        {
            using NpgsqlConnection con = (NpgsqlConnection)db.CreateConnection();
            using NpgsqlCommand cmd = new(query, con);
            cmd.Parameters.AddWithValue("@Id", id);
            NpgsqlDataReader reader = (NpgsqlDataReader)db.ExecuteReader(query, cmd);

            if (reader.Read())
            {
                string type = (string)reader["type"];
                double temperature = (double)((Int16)reader["temperature"]);
                WeatherForecast res = new(id);
                switch (type)
                {
                    case nameof(SupportedTempTypes.Fahrenheit):
                        res.TemperatureF = temperature;
                        break;
                    default:

                        res.TemperatureC = temperature;
                        break;
                }

                return await Task.FromResult(res);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        return null;
    }

    public async Task<WeatherForecast?> Patch(WeatherPathInputModel model)
    {
        string query = """
            UPDATE weathers
            SET 
              temperature = @Temperature,
              type = @Type
            WHERE id = @Id;
            """;
        try
        {
            using NpgsqlConnection con = (NpgsqlConnection)db.CreateConnection();
            using NpgsqlCommand cmd = new(query, con);
            cmd.Parameters.AddWithValue("@Id", model.Id);
            cmd.Parameters.AddWithValue("@Type", model.Type);
            cmd.Parameters.AddWithValue("@Temperature", model.Temperature);
            db.ExecuteNonQuery(query, cmd);
            WeatherForecast res = new(model.Id);

            switch (model.Type)
            {
                case nameof(SupportedTempTypes.Fahrenheit):
                    res.TemperatureF = model.Temperature;
                    break;
                default:

                    res.TemperatureC = model.Temperature;
                    break;
            }
            return res;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        return null;
    }

    public async Task<IEnumerable<WeatherForecast>> GetAll()
    {
        List<WeatherForecast> resList = [];
        string query = """
            SELECT 
              temperature,
              type,
              id
            FROM weathers;
            """;
        try
        {
            using NpgsqlConnection con = (NpgsqlConnection)db.CreateConnection();
            using NpgsqlCommand cmd = new(query, con);
            NpgsqlDataReader reader = (NpgsqlDataReader)db.ExecuteReader(query, cmd);

            while (reader.Read())
            {
                string type = (string)reader["type"];
                string id = (string)reader["id"];
                double temperature = (double)((Int16)reader["temperature"]);
                WeatherForecast res = new(id);
                switch (type)
                {
                    case nameof(SupportedTempTypes.Fahrenheit):
                        res.TemperatureF = temperature;
                        break;
                    default:

                        res.TemperatureC = temperature;
                        break;
                }
                resList.Add(res);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        return await Task.FromResult(resList);
    }
}
