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
[Authorize(Roles = "Developer")]
public class DeveloperController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private IConfiguration _config;
    private AuthHelper helper;
    private readonly OperationsHelper opHelper;

    public DeveloperController(IConfiguration con)
    {
        this._config = con;
        this._dapper = new DataContextDapper(this._config);
        this.helper = new AuthHelper(con);
        this.opHelper = new OperationsHelper(con);
    }

    [HttpGet("getDevName")]
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

    [Authorize(Policy = "CanUploadGames")]
    [HttpPost("uploadGame")]
    public IActionResult uploadGame(UploadGameDTO game)
    {
        string? userId = this.User.FindFirst("id")?.Value;

        if (userId == null)
        {
            return BadRequest(new { message = "User not found!" });
        }

        int developerId = int.Parse(userId);

        int gameId = this.opHelper.uploadGame(game, developerId);

        if (gameId <= 0)
        {
            return BadRequest(new { message = "Failed to upload the game." });
        }

        return Ok(new
        {
            message = "Game uploaded successfully!",
            gameId = gameId
        });
    }
    [Authorize (Policy = "CanUploadGames")]
    [HttpPost("uploadGameRequirements")]
    public IActionResult uploadGameRequirements(GameRequirementsDTO requirements)
    {
        if(!this.opHelper.gameExists(requirements.gameId))
        {
            return BadRequest(new { message = "Game does not exist!" });
        }
        string? userId = this.User.FindFirst("id")?.Value;
        if(userId == null)
        {
            return BadRequest(new { message = "User not found!" });
        }
        int id = int.Parse(userId);
        //Check if game belongs to the developer
        if(!this.opHelper.gameBelongsToDeveloper(requirements.gameId, id))
        {
            return Unauthorized(new { message = "This game does not belong to you!" });
        }
        return this.opHelper.uploadGameRequirements(requirements);
    }
}