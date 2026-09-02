using Microsoft.AspNetCore.Mvc;
using GAME_CAFE.Data;
using GAME_CAFE.Models;
using GAME_CAFE.Dtos;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using GAME_CAFE.Helper;
using Microsoft.AspNetCore.Authorization;
using System.Numerics;

namespace GAME_CAFE.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private IConfiguration _config;
    private AuthHelper helper;

    public AuthController(IConfiguration con)
    {
        this._config = con;
        this._dapper = new DataContextDapper(this._config);
        this.helper = new AuthHelper(con);
    }
    [HttpPost("createNewAcc")]
    public IActionResult createNewAcc(CreateAccountDTO user)
    {
        //Can only create Cusomer or Developer here.
        if(user.role != "Customer" && user.role != "Developer")
        {
            return BadRequest(new { message = "Invalid role!" });
        }
        try
        {
            string email = user.Email;
            if (this.helper.emailExists(email))
            {
                return BadRequest(new { message = "Account for this email already exists!" });
            }


            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator ran = RandomNumberGenerator.Create())
            {
                ran.GetNonZeroBytes(passwordSalt);
            }

            byte[] passwordHash = this.helper.getPasswordHash(user.password, passwordSalt);

            string sqlToAdduser = @"INSERT INTO Users (name, email, phone) VALUES (@name,@email,@phone)";
            List<SqlParameter> userPar = new List<SqlParameter>
            {
                new SqlParameter("@name", user.Name),
                new SqlParameter("@email", user.Email),
                new SqlParameter("@phone", user.Phone),
            };
        
            //Insert user in Users table
            if (this._dapper.ExecuteSQL_WithParameters(sqlToAdduser, userPar))
            {
                //Get the id of the concerned user to add in Auth
                int userId = this.helper.getId(user.Email);

                int roleId=3;
                if(user.role == "Developer")
                {
                    roleId = 4;
                }
                if(user.role == "Super Admin")
                {
                    roleId = 1;
                }
                //Query to add in UserRoles table
                string sql_addInUserRoles = @"INSERT INTO UserRoles VALUES (@userId,@roleId)";
                List<SqlParameter> rolesParam = new List<SqlParameter>
                {
                    new SqlParameter("@userId", userId),
                    new SqlParameter("@roleId", roleId),
                };

                if (this._dapper.ExecuteSQL_WithParameters(sql_addInUserRoles, rolesParam))
                {
                        
                }
                //Query to add in auth
                string SQL_AddAuth = @"INSERT INTO Auth (userId,PasswordSalt,PasswordHash) VALUES (@userId, @passwordSalt, @passwordHash)";
            
                List<SqlParameter> authPar = new List<SqlParameter>
                {
                    new SqlParameter("@userId", userId),
                    new SqlParameter("@passwordSalt", SqlDbType.VarBinary) { Value = passwordSalt },
                    new SqlParameter("@passwordHash", SqlDbType.VarBinary) { Value = passwordHash }
                };
                //Insert auth info in Auth table
                if (this._dapper.ExecuteSQL_WithParameters(SQL_AddAuth, authPar))
                {
                    //Insert permissions of the users
                    this.helper.insertPermissions(userId,user.role);
                    IEnumerable<string> allPermissions =
                    this.helper.getUserPermissions(userId);
                    string token = this.helper.CreateToken(userId, user.role, allPermissions);
                    return Ok(new
                    {
                        token = token
                    });
                
                    // return Ok(new { message = "Account created!" });
                }
                return StatusCode(500, new { message = "Failed to create account!" });
            }
            
            return StatusCode(500, new { message = "Failed to create account!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
    [HttpPost("Login")]
    public IActionResult login(UserLoginDTO user)
    {
        if (!this.helper.emailExists(user.Email))
        {
            return Unauthorized(new { message = "Account not found for this email!" });
        }
        //Get the id of the concerned user
        int userId = this.helper.getId(user.Email);

        //Get password salt of the user from DB.
        string sqlToFindPassSalt = @"SELECT PasswordSalt FROM Auth WHERE userId = @id";
        List<SqlParameter> param = new List<SqlParameter>
        {
          new SqlParameter("@id",userId)  
        };

        byte[]? passwordSalt = this._dapper.returnSingle_WithParameters<byte[]>(sqlToFindPassSalt,param);
        if(passwordSalt == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }
        //Make hash of the entered password
        byte[]? passwordHash = this.helper.getPasswordHash(user.Password,passwordSalt);

        //Get the real password hash
        string sqlToGetHash = @"SELECT PasswordHash FROM Auth WHERE userId = @id";
        List<SqlParameter> paramForHash = new List<SqlParameter>
        {
          new SqlParameter("@id",userId)  
        };
        byte[]? actualHash = this._dapper.returnSingle_WithParameters<byte[]>(sqlToGetHash,paramForHash);

        if(actualHash == null || actualHash == default)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        //Compare the actual hash and entered password`s hash
        if (passwordHash.Length != actualHash.Length)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }
        for(int i=0; i<passwordHash.Length; i++)
        {
            if(passwordHash[i] != actualHash[i])
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }
        }
        //Get role of user
        string? role = this.helper.getRole(userId);
        if(role == null || role == default)
        {
            return Unauthorized(new { message = "Something went wrong, unable to find your account!" });
        }
        //Now get all permissions of the user from UserPermissions
        IEnumerable<string> allPermissions = this.helper.getUserPermissions(userId);

        //If login is successful, return token.
        string token = this.helper.CreateToken(userId, role, allPermissions);
        return Ok(new
        {
            token = token
        });
    }
}