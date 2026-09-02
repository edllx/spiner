namespace simpleapi;

using System.Data;
using System.Data.Common;
using Npgsql;

public interface IDataContext
{
    DbConnection CreateConnection();
    int ExecuteNonQuery(string query, DbCommand cmd);
    DbDataReader ExecuteReader(string query, DbCommand cmd);
    DataTable ExecuteQuery(string query, DbCommand cmd);
}

public class NpgsqlContext(IConfiguration configuration) : IDataContext
{
    private readonly string _connectionString = configuration["DB_CONNECTION_STRING"] ?? throw new Exception("Missing config : DB_CONNECTION_STRING");

    public DbConnection CreateConnection()
    {
        NpgsqlConnection con = new NpgsqlConnection(_connectionString);
        return con;
    }

    public int ExecuteNonQuery(string query, DbCommand cmd)
    {
        cmd.Connection?.Open();
        return cmd.ExecuteNonQuery();
    }

    public DataTable ExecuteQuery(string query, DbCommand cmd)
    {
        try
        {
            cmd.Connection?.Open();
            using NpgsqlDataAdapter adapter = new((NpgsqlCommand)cmd);
            DataTable resultTable = new();
            adapter.Fill(resultTable);
            return resultTable;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        return new DataTable();
    }

    public DbDataReader ExecuteReader(string query, DbCommand cmd)
    {
        cmd.Connection?.Open();
        return (NpgsqlDataReader)cmd.ExecuteReader();
    }
}
