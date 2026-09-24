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
    public async Task<IActionResult> uploadGame([FromForm] UploadGameDTO game, IFormFile image, IFormFile file)
    {
        if (this.opHelper.nameIsTaken(game.name))
        {
            return BadRequest (new {message = "This game name is already taken! Choose another name"});
        }
        string? userId = this.User.FindFirst("id")?.Value;

        if (userId == null)
        {
            return BadRequest(new { message = "User not found!" });
        }

        int developerId = int.Parse(userId);
        int gameId = await this.opHelper.uploadGame(game, image, file,developerId);

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
    public async Task<IActionResult> editMyGame([FromForm] EditGameDTO editedGame, IFormFile? image, IFormFile? file)
    {
        if (this.opHelper.nameIsTaken(editedGame.name))
        {
            return BadRequest (new {message = "This game name is already taken! Choose another name"});
        }
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        if (!this.opHelper.gameBelongsToDeveloper(editedGame.id, userId))
        {
            return Unauthorized (new{message = "This game does not belong to you!"});
        }
        bool edited = await this.opHelper.editGame(editedGame, image, file,editedGame.id);
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
        IEnumerable<ReturnPendingUploadReqToDev> list = this.opHelper.viewPendingGameRequests(userId);
        if(list == null)
        {
            return BadRequest (new {message = "No requests found!"});
        }
        return Ok(list);
    }
    [HttpPut("doNotShowUploadReq")]
    public bool markUploadReqAsDontShow(int gameId)
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        return this.opHelper.dontShowUploadReq(gameId,userId);
    }
    [HttpPut("doNotShowWarningEndReq")]
    public bool markWarningEndReqAsDontShow(int reqId)
    {
        return this.opHelper.dontShowWarningEndReq(reqId);
    }
    [Authorize (Policy = "CanViewHisWarnings")]
    [HttpGet("viewReceivedWarnings")]
    public IActionResult viewWarnings()
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        IEnumerable<ReturnWarningsDTO> list = this.opHelper.returnWarningsToDev(userId);
        if(list == null)
        {
            return BadRequest (new {message = "No warnings found!"});
        }
        return Ok(list);
    }
    [Authorize (Policy = "CanViewHisWarnings")]
    [HttpGet("getWarningsCount")]
    public int getWarningsCount()
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        return this.opHelper.getWarningsCount(userId);
    }
    [Authorize (Policy = "CanViewHisWarnings")]
    [HttpGet("getWarningReason")]
    public string getWarningReason(int gameId)
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        return this.opHelper.getWarningReason(gameId,userId);
    }
    [Authorize (Policy = "CanRequestToEndWarning")]
    [HttpPost("makeEndWarningRequest")]
    public IActionResult requestWarningEnd(WarningEndRequestDTO req)
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        if(this.opHelper.makeEndWarningRequest(req.warningId,req.requestNote, userId))
        {
            return Ok(new{message = "Request sent!"});
        }
        return BadRequest (new {message = "Failed to send request!"});
    }
    [HttpGet("getEndWarningReqs")]
    [Authorize(Policy = "CanViewHisEndWarningReqs")]
    public IActionResult getEndWarningReqs()
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        IEnumerable<ReturnWarningEndReqDTO> list = this.opHelper.getWarningEndReqOfDev(userId);
        if(list!=null)
        {
            return Ok(list);
        }
        return BadRequest (new {message = "Failed to get requests!"});
    }
}
