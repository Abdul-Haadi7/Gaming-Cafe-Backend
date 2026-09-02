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
[Authorize(Roles = "Customer")]
public class CustomerController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private IConfiguration _config;
    private AuthHelper helper;
    private readonly OperationsHelper opHelper;

    public CustomerController(IConfiguration con)
    {
        this._config = con;
        this._dapper = new DataContextDapper(this._config);
        this.helper = new AuthHelper(con);
        this.opHelper = new OperationsHelper(con);
    }

    [HttpGet("getCustName")]
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
}