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
    [HttpGet("getAllGames")]
    [Authorize(Policy = "CanViewAllGames")]
    public IActionResult getAllGames()
    {
        IEnumerable<ReturnGamesToCustomerDTO> list = this.opHelper.returnGamesToCust();
        
        if (list != null)
        {
            return Ok(list);
        }
        return BadRequest(new{message = "Games not found"});
    }
    [HttpGet("getSingleGame")]
    [Authorize(Policy = "CanViewAllGames")]
    public IActionResult getGameById(int gameId)
    {
        ReturnGamesToCustomerDTO game = this.opHelper.returnSingleGameToCust(gameId);
        
        if (game != null)
        {
            return Ok(game);
        }
        return BadRequest(new{message = "Game not found"});
    }
    [HttpPost("addGameToCart")]
    [Authorize(Policy = "CanAddGameToCart")]
    public IActionResult addToCart(int gameId)
    {
        if (!this.opHelper.gameExists(gameId))
        {
            return NotFound(new { message = "Game not found!" });
        }
        string? userId = this.User.FindFirst("id")?.Value;
        int id = int.Parse(userId);
        if (this.opHelper.alreadyInCart(gameId, id))
        {
            return BadRequest("This game is already in your cart!");
        }
        bool done = this.opHelper.addToCart(gameId,id);
        if (done)
        {
            return new OkObjectResult("Game added to cart!" );
        }
        return BadRequest("Game could not be added to cart!");
    }
    [HttpGet("getMyRating")]
    public IActionResult getRating(int gameId)
    {
        if (!this.opHelper.gameExists(gameId))
        {
            return NotFound(new { message = "Game not found!" });
        }
        string? userId = this.User.FindFirst("id")?.Value;
        int id = int.Parse(userId);
        if (!this.opHelper.alreadyRated(id, gameId))
        {
            return BadRequest("You have not rated this game yet!");
        }
        if (!this.opHelper.gameExists(gameId))
        {
            return NotFound(new { message = "Game not found!" });
        }

        decimal rating = this.opHelper.getRating(id,gameId);
        if(rating > 0)
        {
            return Ok(rating);
        }
        return BadRequest("Could not get rating!");
    }
    [HttpPost("rateGame")]
    // [Authorize(Policy = "CanRateGames")]
    public IActionResult rateGame(decimal gameRating,int gameId)
    {
        Console.WriteLine(gameRating);
        if (!this.opHelper.gameExists(gameId))
        {
            return NotFound(new { message = "Game not found!" });
        }
        if(gameRating > 10 || gameRating <= 0)
        {
            return BadRequest("Invalid rating!");
        }
        string? userId = this.User.FindFirst("id")?.Value;
        int id = int.Parse(userId);
        if (this.opHelper.alreadyRated(id, gameId))
        {
            return BadRequest("You already rated this game!");
        }
        if(userId == null)
        {
            return Unauthorized("User not found!");
        }
        bool done = this.opHelper.rateGame(id,gameId,gameRating);
        if (!done)
        {
            return BadRequest("Rating could not be saved!");
        }
        return new OkObjectResult("Rating saved!" );
    }
    [HttpGet("getGameReq")]
    [Authorize(Policy = "CanViewAllGames")]
    public IActionResult getRequirements(int gameId)
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);

        if (!this.opHelper.userExists(userId))
        {
            return BadRequest(new{message = "User not found"});
        }
     
        ReturnGameReqDTO? req = this.opHelper.getReqById(gameId);
        if (req == null)
        {
            return NotFound(new { message = "Requirements not found!" });
        }
        return Ok(req);

    }
}