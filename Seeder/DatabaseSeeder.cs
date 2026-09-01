using System.Security.Cryptography;
using System.Data;
using Microsoft.Data.SqlClient;
using GAME_CAFE.Data;
using GAME_CAFE.Helper;

namespace GAME_CAFE.Seeder;

public class DatabaseSeeder
{
    private readonly DataContextDapper _dapper;
    private readonly AuthHelper _helper;
    private readonly IConfiguration _config;

    public DatabaseSeeder(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        _helper = new AuthHelper(config);
        _config = config;
    }

    public void SeedSuperAdmin()
    {
        IConfigurationSection superAdmin = this._config.GetSection("SuperAdmin");;
        string name = superAdmin["Name"];
        string email = superAdmin["Email"];
        string password = superAdmin["Password"];
        string phone = superAdmin["Phone"];
        DateTime dateOfBirth = DateTime.Parse(superAdmin["DateOfBirth"]);

        //Check if Super Admin already exists
        string checkSql = @"
            SELECT COUNT(*)
            FROM Users u
            INNER JOIN UserRoles ur ON u.id = ur.userId
            WHERE u.email = @email
            AND ur.roleId = 1";

        List<SqlParameter> checkParams = new()
        {
            new SqlParameter("@email", email)
        };

        int existingSuperAdmins =
            _dapper.returnSingle_WithParameters<int>(checkSql,checkParams);

        if (existingSuperAdmins > 0)
        {
            return;
        }

        //Generate password salt
        byte[] passwordSalt = new byte[128 / 8];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetNonZeroBytes(passwordSalt);
        }

        //Generate password hash 
        byte[] passwordHash = _helper.getPasswordHash(password, passwordSalt);

        //Insert into Users
        string userSql = @"
            INSERT INTO Users
            (name, email, phone, dateOfBirth)
            VALUES
            (@name, @email, @phone, @dateOfBirth)";

        List<SqlParameter> userParams = new()
        {
            new SqlParameter("@name", name),
            new SqlParameter("@email", email),
            new SqlParameter("@phone", phone),
            new SqlParameter("@dateOfBirth", dateOfBirth)
        };
        //Insert
        this._dapper.ExecuteSQL_WithParameters(userSql,userParams);
        //Get id
        int userId = this._helper.getId(email);

        //Insert Super Admin role
        string roleSql = @"INSERT INTO UserRoles (userId, roleId) VALUES (@userId, @roleId)";

        List<SqlParameter> roleParams = new()
        {
            new SqlParameter("@userId", userId),
            new SqlParameter("@roleId", 1)
        };

        _dapper.ExecuteSQL_WithParameters(roleSql,roleParams);

        //Insert in auth
        string authSql = @"INSERT INTO Auth (userId, passwordSalt, passwordHash)
            VALUES (@userId, @passwordSalt, @passwordHash)";

        List<SqlParameter> authParams = new()
        {
            new SqlParameter("@userId", userId),
            new SqlParameter("@passwordSalt", SqlDbType.VarBinary)
            {
                Value = passwordSalt
            },
            new SqlParameter("@passwordHash", SqlDbType.VarBinary)
            {
                Value = passwordHash
            }
        };

        _dapper.ExecuteSQL_WithParameters(authSql,authParams);

        //Give Super Admin all permissions of super admin role
        _helper.insertPermissions(userId,"Super Admin");
    }
}