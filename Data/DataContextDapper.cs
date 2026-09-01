using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace GAME_CAFE.Data;

public class DataContextDapper
{
    private readonly IConfiguration _config;
    private readonly IDbConnection connection;

    public DataContextDapper(IConfiguration config)
    {
        this._config = config;
        this.connection = new SqlConnection(this._config.GetConnectionString("Gaming_Cafe_DB"));
    }

    public T? returnSingle<T>(string sql)
    {
        return this.connection.QuerySingleOrDefault<T>(sql);
    }

    public IEnumerable<T> loadData<T>(string sql)
    {
        // Console.WriteLine("\n\n"+sql+"\n\n");
        return this.connection.Query<T>(sql);
    }


    public bool executeSQL(string sql)
    {
        int n = this.connection.Execute(sql);
        if (n > 0)
        {
            return true;
        }
        return false;
    }
    public bool ExecuteSQL_WithParameters(string sql, List<SqlParameter> par)
    {
        SqlCommand cmd = new SqlCommand(sql);

        foreach (SqlParameter p in par)
        {
            cmd.Parameters.Add(p);
        }
        SqlConnection con = new SqlConnection(this._config.GetConnectionString("Gaming_Cafe_DB"));
        con.Open();
        cmd.Connection = con;
        int rowsAffected = cmd.ExecuteNonQuery();
        con.Close();

        if(rowsAffected > 0)
        {
            return true;
        }
        return false;
    }
    public IEnumerable<T> loadData_WithParameters<T>(string sql,List<SqlParameter> par)
    {
        List<T> result = new List<T>();

        using (SqlConnection con = new SqlConnection(
            this._config.GetConnectionString("Gaming_Cafe_DB")))
        {
            con.Open();

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                foreach (SqlParameter p in par)
                {
                    cmd.Parameters.Add(p);
                }

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add((T)reader[0]);
                    }
                }
            }
        }

        return result;
    }
    // public T? returnSingle_WithParameters<T>(string sql,List<SqlParameter> par)
    // {
    //     using (SqlConnection con = new SqlConnection(
    //         this._config.GetConnectionString("Gaming_Cafe_DB")))
    //     {
    //         con.Open();

    //         using (SqlCommand cmd = new SqlCommand(sql, con))
    //         {
    //             foreach (SqlParameter p in par)
    //             {
    //                 cmd.Parameters.Add(p);
    //             }

    //             object? result = cmd.ExecuteScalar();

    //             if (result == null || result == DBNull.Value)
    //             {
    //                 return default;
    //             }

    //             return (T)result;
    //         }
    //     }
    // }
    public T? returnSingle_WithParameters<T>(
    string sql,
    List<SqlParameter> par)
{
    using (SqlConnection con = new SqlConnection(
        this._config.GetConnectionString("Gaming_Cafe_DB")))
    {
        con.Open();

        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
            foreach (SqlParameter p in par)
            {
                cmd.Parameters.Add(p);
            }

            object? result = cmd.ExecuteScalar();

            if (result == null || result == DBNull.Value)
            {
                return default;
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }
    }
}
}