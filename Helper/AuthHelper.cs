using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using GAME_CAFE.Data;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace GAME_CAFE.Helper;

public class AuthHelper
{
    private readonly IConfiguration _config;
    private readonly DataContextDapper _dapper;

    public AuthHelper(IConfiguration con)
    {
        this._config = con;
        this._dapper = new DataContextDapper(this._config);
    }
    public byte[] getPasswordHash(string password, byte[] salt)
    {
        //Make one string by combining the salt and password key
        string passwordKey = this._config.GetSection("AppSettings:PasswordKey").Value
        +Convert.ToBase64String(salt);

        //Generate hash
        byte[] passwordHash = KeyDerivation.Pbkdf2(password: password,
        salt: Encoding.ASCII.GetBytes(passwordKey),
        prf: KeyDerivationPrf.HMACSHA256,
        iterationCount:6767,
        numBytesRequested:256/8);
        return passwordHash;
    }
    public string CreateToken(int id, string role, IEnumerable<string> allPermissions)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim("id", id.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        foreach (string permission in allPermissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        string? tokenKeyString = _config["AppSettings:TokenKey"];

        SymmetricSecurityKey tokenKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(tokenKeyString ?? "")
            );

        SigningCredentials credentials = new SigningCredentials(
            tokenKey,
            SecurityAlgorithms.HmacSha512Signature
        );

        SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = credentials,
            Expires = DateTime.UtcNow.AddDays(1),
            Issuer = _config["AppSettings:Issuer"],
            Audience = _config["AppSettings:Audience"]
        };

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

        SecurityToken token = handler.CreateToken(descriptor);

        return handler.WriteToken(token);
    }
    
    public bool emailExists(string email)
    {
        string sql_checkEmail = @"SELECT email FROM Users WHERE email = @email";

        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@email", email)
        };

        IEnumerable<string> existingEmails =
            this._dapper.loadData_WithParameters<string>(
                sql_checkEmail,
                parameters
            );

        return existingEmails.Any();
    }
    public void insertPermissions(int userId,string role)
    {
        int roleId = 3;
        if(role == "Super Admin")
        {
            roleId = 1;
        }

        if(role == "Developer")
        {
            roleId = 4;
        }
        //Get all the permissions of concerned role from DB
        string sql_getAllPer = @"SELECT permissionId FROM RolePermissions WHERE roleId = @roleId";
        List<SqlParameter> param = new List<SqlParameter>
        {
            new SqlParameter("@roleId", roleId),
        };
        Console.WriteLine(sql_getAllPer);
        //All permissions of concerned role are now in this list
        IEnumerable<int> allPermissions = this._dapper.loadData_WithParameters<int>(sql_getAllPer,param);
        foreach(int str in allPermissions)
        {
            Console.WriteLine(str);
        }
        //Now insert all permissions in UserPermissions table.
        foreach(int per in allPermissions)
        {
            string sql_insertPerm = @"INSERT INTO UserPermissions VALUES (@userId,@permId)";
            List<SqlParameter> param_insertPerm = new List<SqlParameter>
            {
                new SqlParameter("@userId", userId),
                new SqlParameter("@permId", per)
            };
            Console.WriteLine(sql_insertPerm);
            this._dapper.ExecuteSQL_WithParameters(sql_insertPerm,param_insertPerm);
        }
    }

    /*Since every admin has custom permissions chosen by super admin and we do not want to give all admin permissions 
    to every admin, we have separate method to add admin`s permissions.*/
    public void insertAdminPermissions(int userId,IEnumerable<int> permissions)
    {
        foreach(int per in permissions)
        {
            string sql_insertPerm = @"INSERT INTO UserPermissions VALUES (@userId,@permId)";
            List<SqlParameter> param_insertPerm = new List<SqlParameter>
            {
                new SqlParameter("@userId", userId),
                new SqlParameter("@permId", per)
            };
            Console.WriteLine(sql_insertPerm);
            this._dapper.ExecuteSQL_WithParameters(sql_insertPerm,param_insertPerm);
        }
    }
    public int getId(string email)
    {
        string sqlToGetId = @"SELECT id from Users WHERE email = @email";
    
        List<SqlParameter> paramForId = new List<SqlParameter>
        {
            new SqlParameter("@email", email),
        };
        int userId = this._dapper.returnSingle_WithParameters<int>(sqlToGetId,paramForId);
        return userId;
    }
    public string getRole(int userId)
    {
        string sql_ToFetchUserRole = @"SELECT name FROM Roles WHERE id = (SELECT roleId from UserRoles WHERE userId = @userId)";
        List<SqlParameter> paramForRole = new List<SqlParameter>
        {
            new SqlParameter("@userId",userId)  
        };
        string? role = this._dapper.returnSingle_WithParameters<string>(sql_ToFetchUserRole,paramForRole);
        return role;
    }
    public IEnumerable<string> getUserPermissions(int userId)
    {
        string sql_getPerm = @"SELECT name FROM Permissions WHERE id in 
        (SELECT permissionId from UserPermissions WHERE userId = @userId)";
        List<SqlParameter> paramForPerms = new List<SqlParameter>
        {
            new SqlParameter("@userId",userId)  
        };
        IEnumerable<string> allPermissions = this._dapper.loadData_WithParameters<string>(sql_getPerm,paramForPerms);
        return allPermissions;
    }
}