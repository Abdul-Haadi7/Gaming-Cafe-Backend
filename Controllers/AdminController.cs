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
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private IConfiguration _config;
    private AuthHelper helper;
    private readonly OperationsHelper opHelper;

    public AdminController(IConfiguration con)
    {
        this._config = con;
        this._dapper = new DataContextDapper(this._config);
        this.helper = new AuthHelper(con);
        this.opHelper = new OperationsHelper(con);
    }

    [HttpGet("getAdminName")]
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
    [HttpGet("getPendingUploadRequests")]
    [Authorize(Policy = "CanApproveGame")]
    public IActionResult getAllGameRequests()
    {
        IEnumerable<ReturnUploadReqToAdminDTO> list = this.opHelper.getAllGameRequests();
        if(list == null)
        {
            return BadRequest (new {message = "No requests found!"});
        }
        return Ok(list);
    }
    [HttpPut("approveGame")]
    [Authorize(Policy = "CanApproveGame")]
    public IActionResult approveGame(int gameId)
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        if (this.opHelper.approveGame(userId, gameId))
        {
            return new OkObjectResult(new { message = "Game approved!" });
        }
        return BadRequest (new {message = "Failed to approve game!"});
    }
    [HttpPut("rejectGame")]
    [Authorize(Policy = "CanRejectGame")]
    public IActionResult rejectGame(int gameId, string reason)
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        if (this.opHelper.rejectGame(userId, gameId, reason))
        {
            return new OkObjectResult(new { message = "Game rejected!" });
        }
        return BadRequest (new {message = "Failed to reject game!"});
    }
    [HttpGet("getActiveReqCount")]
    public int getActiveRequestsCount()
    {
        return this.opHelper.countActiveRequest();
    }
    [HttpGet("getAdminAllGames")]
    
    [Authorize(Policy = "CanViewAllGames")]
    public IActionResult getAllGames()
    {
        IEnumerable<ReturnGamesToAdminDTO> list = this.opHelper.returnGamesToAdmin();
        
        if (list != null)
        {
            return Ok(list);
        }
        return BadRequest(new{message = "Games not found"});
    }
    
    [HttpPost("sendWarning")]
    [Authorize(Policy = "CanSendWarning")]
    public IActionResult sendWarning(int gameId, string reason)
    {
        if (!this.opHelper.gameExists(gameId))
        {
            return NotFound (new {message = "Game not found!"});
        }
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        if (this.opHelper.sendWarning(gameId, reason, userId))
        {
            return new OkObjectResult(new { message = "Warning sent!" });
        }
        return BadRequest (new {message = "Failed to send warning!"});
    }
    [HttpGet("getAllActiveWarnings")]
    [Authorize(Policy = "CanViewAllWarnings")]
    public IActionResult getAllWarnings()
    {
        IEnumerable<ReturnWarningsDTO> list = this.opHelper.returnAllWarnings();
        if (list != null)
        {
            return Ok(list);
        }
        return BadRequest (new {message = "Failed to get warnings!"});
    }
    [HttpGet("getActiveWarningsCount")]
    [Authorize(Policy = "CanViewAllWarnings")]
    public int getActiveWarningsCount()
    {
        return this.opHelper.getAllWarningsCount();
    }
    [HttpPut("endWarning")]
    [Authorize(Policy = "CanEndWarning")]
    public IActionResult endWarning(ReturnWarningsDTO warning)
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        bool done = this.opHelper.endWarning(warning,userId);
        if (done)
        {
            return new OkObjectResult(new { message = "Warning ended!" });
        }
        return BadRequest (new {message = "Failed to end warning!"});
    }
    [HttpGet("getAllWarningEndReq")]
    [Authorize(Policy = "CanViewAllEndWarningReq")]
    public IActionResult getAllWarningEndReqs()
    {
        IEnumerable<ReturnWarningEndReqToAdmin> list = this.opHelper.getAllWarningEndReq();
        if (list != null)
        {
            return Ok(list);
        }
        return BadRequest (new {message = "Failed to get warnings!"});
    }
    [HttpGet("countEndWarningReq")]
    [Authorize(Policy = "CanViewAllEndWarningReq")]
    public int countEndWarningReq()
    {
        return this.opHelper.countEndWarningReq();
    }
    [HttpPut("approveEndWarningReq")]
    [Authorize(Policy = "CanEndWarning")]
    public IActionResult approveEndWarningReq(int warningId)
    {
        ReturnWarningsDTO warning = this.opHelper.getWarningById(warningId);
        return this.endWarning(warning);    
    }
}