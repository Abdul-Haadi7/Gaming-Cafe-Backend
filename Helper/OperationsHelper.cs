using GAME_CAFE.Data;
using Microsoft.Data.SqlClient;

namespace GAME_CAFE.Helper;

public class OperationsHelper
{
    private readonly IConfiguration _config;
    private readonly DataContextDapper _dapper;

    public OperationsHelper(IConfiguration con)
    {
        this._config = con;
        this._dapper = new DataContextDapper(this._config);
    }

    public string getName(int userId)
    {
        string sql = "SELECT name FROM Users WHERE id = @id";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@id", userId)
        };

        string? name = this._dapper.returnSingle_WithParameters<string>(sql, parameters);
        return name;
    }
}