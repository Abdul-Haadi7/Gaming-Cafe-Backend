using System.Data;
using System.Security.Cryptography;
using GAME_CAFE.Data;
using GAME_CAFE.Dtos;
using GAME_CAFE.Helper;
using GAME_CAFE.Models;
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
    [HttpGet("getMyGames")]
    public IActionResult getMyGames()
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userid = int.Parse(id);
        if (!this.opHelper.userExists(userid))
        {
            return BadRequest(new{message = "User not found"});
        }
        IEnumerable<ReturnGamesToDevDTO> list = this.opHelper.getGames(userid);
        return Ok(list);
    }
    [HttpGet("getGameById")]
    [Authorize (Policy = "CanViewOwnGame")]
    public IActionResult getGameById(int gameId)
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);

        if (!this.opHelper.userExists(userId))
        {
            return BadRequest(new{message = "User not found"});
        }
        if (!this.opHelper.gameBelongsToDeveloper(gameId, userId))
        {
            return Unauthorized (new{message = "This game does not belong to you!"});
        }
        EditGameDTO? game = this.opHelper.getGameById(gameId);
        if (game == null)
        {
            return NotFound(new { message = "Game not found!" });
        }
        return Ok(game);

    }

    [HttpGet("getGameReqById")]
    public IActionResult getRequirementsById(int gameId)
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);

        if (!this.opHelper.userExists(userId))
        {
            return BadRequest(new{message = "User not found"});
        }
        if (!this.opHelper.gameBelongsToDeveloper(gameId, userId))
        {
            return Unauthorized (new{message = "This game does not belong to you!"});
        }
        ReturnGameReqDTO? req = this.opHelper.getReqById(gameId);
        if (req == null)
        {
            return NotFound(new { message = "Game not found!" });
        }
        return Ok(req);

    }

    [Authorize (Policy = "DeleteOwnGame")]
    [HttpDelete("deleteMyGame")]
    public IActionResult deleteMyGame(int gameId)
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        if (!this.opHelper.gameBelongsToDeveloper(gameId, userId))
        {
            return Unauthorized (new{message = "This game does not belong to you!"});
        }
        return this.opHelper.deleteGame(gameId);
    }
    [Authorize (Policy = "EditOwnGames")]
    [HttpPut("editMyGame")]
    public IActionResult editMyGame(EditGameDTO editedGame, int gameId)
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        if (!this.opHelper.gameBelongsToDeveloper(gameId, userId))
        {
            return Unauthorized (new{message = "This game does not belong to you!"});
        }
        bool edited = this.opHelper.editGame(editedGame, gameId);
        if (edited)
        {
            return new OkObjectResult(new { message = "Game edited!" });
        }
        return BadRequest (new {message = "Game could not be edited!"});
    }
    [Authorize (Policy = "CanEditOwnGameRequirements")]
    [HttpPut("editMyGameReuirements")]
    public IActionResult editGameRequirements(EditRequirementsDTO requirements, int gameId)
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        if (!this.opHelper.gameBelongsToDeveloper(gameId, userId))
        {
            return Unauthorized (new{message = "This game does not belong to you!"});
        }
        bool edited = this.opHelper.editRequirements(requirements,gameId);
        if (edited)
        {
            return new OkObjectResult(new { message = "Requirements edited!" });
        }
        return BadRequest(new{message = "Unable to edit!"});
    }
    [Authorize (Policy = "EditOwnGames")]
    [HttpPut("toggleAvailability")]
    public IActionResult toggleGameAvailability(int gameId)
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        if (!this.opHelper.gameBelongsToDeveloper(gameId, userId))
        {
            return Unauthorized (new{message = "This game does not belong to you!"});
        }
        bool done = this.opHelper.toggleAvailability(gameId);
        if (done)
        {
            return new OkObjectResult(new { message = "Availability toggled!" });
        }
        return BadRequest (new {message = "Could not toggle availability!"});
    }
    [Authorize (Policy = "CanViewOwnRequests")]
    [HttpGet("viewPendingGameRequests")]
    public IActionResult viewPendingGameRequests()
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        IEnumerable<ReturnGamesToDevDTO> list = this.opHelper.viewPendingGameRequests(userId);
        if(list == null)
        {
            return BadRequest (new {message = "No requests found!"});
        }
        return Ok(list);
    }
}