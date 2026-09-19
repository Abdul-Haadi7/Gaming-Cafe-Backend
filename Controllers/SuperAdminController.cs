using System.Data;
using System.Security.Cryptography;
using GAME_CAFE.Data;
using GAME_CAFE.Dtos;
using GAME_CAFE.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Data.SqlClient;

namespace GAME_CAFE.Controllers;

[ApiController]
[Authorize(Roles = "Super Admin")]
public class SuperAdminController : ControllerBase
{

    private readonly DataContextDapper _dapper;
    private IConfiguration _config;
    private AuthHelper helper;
    private readonly OperationsHelper opHelper;

    public SuperAdminController(IConfiguration con)
    {
        this._config = con;
        this._dapper = new DataContextDapper(this._config);
        this.helper = new AuthHelper(con);
        this.opHelper = new OperationsHelper(con);
    }

    [Authorize (Policy = "CanAddAdmin")]
    [HttpPost("addAdmin")]
    public IActionResult addAdmin(CreateAdminDTO admin)
    {
      
        try
        {
            string email = admin.email;
            if (this.helper.emailExists(email))
            {
                return BadRequest(new { message = "Account for this email already exists!" });
            }
            //Make sure the role is Admin
            admin.role = "Admin";
            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator ran = RandomNumberGenerator.Create())
            {
                ran.GetNonZeroBytes(passwordSalt);
            }

            byte[] passwordHash = this.helper.getPasswordHash(admin.password, passwordSalt);

            string sqlToAddAdmin = @"INSERT INTO Users (name, email, phone) VALUES (@name,@email,@phone)";
            List<SqlParameter> userPar = new List<SqlParameter>
            {
                new SqlParameter("@name", admin.name),
                new SqlParameter("@email", admin.email),
                new SqlParameter("@phone", admin.phone),
            };
            //Insert admin in Users table
            if (this._dapper.ExecuteSQL_WithParameters(sqlToAddAdmin, userPar))
            {
                //Get the id of the this admin to add in Auth
                int userId = this.helper.getId(admin.email);

                int roleId=2;
             
                //Query to add in UserRoles table
                string sql_addInUserRoles = @"INSERT INTO UserRoles VALUES (@userId,@roleId)";
                List<SqlParameter> rolesParam = new List<SqlParameter>
                {
                    new SqlParameter("@userId", userId),
                    new SqlParameter("@roleId", roleId),
                };

                this._dapper.ExecuteSQL_WithParameters(sql_addInUserRoles, rolesParam);
                
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
                    //Insert permissions of the admin
                    this.helper.insertAdminPermissions(userId,admin.permissions);
                    return Ok(new { message = "Admin added!" });
                }
                return StatusCode(500, new { message = "Failed to create account!" });
            }
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
    [Authorize (Policy = "CanAddAdmin")]
    [HttpGet("getAdminPermissions")]
    public IActionResult getAllAdminPermissions()
    {
        IEnumerable<Permission> list = this.opHelper.getAdminPerm();
        if (list != null)
        {
            return Ok(list);
        }
        return BadRequest (new {message = "Failed to get permissions!"});
    }
    [HttpGet("getSuperAdminName")]
    public string getName()
    {
        string? userId = this.User.FindFirst("id")?.Value;
        if(userId == null)
        {
            return "";
        }
        int id = int.Parse(userId);
        string name = this.opHelper.getName(id);
        return name;
    }
    [Authorize (Policy = "CanDeleteAnyGame")]
    [HttpDelete("deleteGame")]
    public IActionResult deleteGame(int gameId)
    {
        return this.opHelper.deleteGame(gameId);
    }
    [Authorize (Policy = "CanBlockCustomers")]
    [HttpPut("blockCust")]
    public IActionResult blockCust(int custId)
    {
        if(!this.opHelper.userRoleIsCorrect(custId,"Customer")){
            return BadRequest (new {message = "Invalid role!"});
        }
        return this.opHelper.blockOrUnblockUser(custId,false);
    }
    [Authorize (Policy = "CanBlockDevelopers")]
    [HttpPut("blockDev")]
    public IActionResult blockDev(int devId)
    {
   
        if(!this.opHelper.userRoleIsCorrect(devId,"Developer")){
            return BadRequest (new {message = "Invalid role!!"});
        }
        return this.opHelper.blockOrUnblockUser(devId,false);
    }
    [Authorize (Policy = "CanBlockAdmins")]
    [HttpPut("blockAdmin")]
    public IActionResult blockAdmin(int adminId)
    {
        if(!this.opHelper.userRoleIsCorrect(adminId,"Admin")){
            return BadRequest (new {message = "Invalid role!!"});
        }
        return this.opHelper.blockOrUnblockUser(adminId,false);
    }

    [Authorize (Policy = "CanUnblockCustomers")]
    [HttpPut("unblockCust")]
    public IActionResult unblockCust(int custId)
    {
        if(!this.opHelper.userRoleIsCorrect(custId,"Customer")){
            return BadRequest (new {message = "Invalid role!"});
        }
        return this.opHelper.blockOrUnblockUser(custId,true);
    }
    [Authorize (Policy = "CanUnblockDevelopers")]
    [HttpPut("unblockDev")]
    public IActionResult unblockDev(int devId)
    {
        if(!this.opHelper.userRoleIsCorrect(devId,"Developer")){
            return BadRequest (new {message = "Invalid role!!"});
        }
        return this.opHelper.blockOrUnblockUser(devId,true);
    }
    [Authorize (Policy = "CanUnblockAdmins")]
    [HttpPut("unblockAdmin")]
    public IActionResult unblockAdmin(int adminId)
    {
        if(!this.opHelper.userRoleIsCorrect(adminId,"Admin")){
            return BadRequest (new {message = "Invalid role!!"});
        }
        return this.opHelper.blockOrUnblockUser(adminId,true);
    }
    [HttpGet("getAllAdmins")]
    [Authorize(Policy = "CanViewAllAdmins")]
    public IActionResult getAllAdmins()
    {
        IEnumerable<ReturnAdminToSuperAdminDTO> list = 
        this.opHelper.getAllAdmins();
        if(list!=null){
            return Ok(list);
        }
        return BadRequest (new {message = "Unable to get admins!"});
    }

}