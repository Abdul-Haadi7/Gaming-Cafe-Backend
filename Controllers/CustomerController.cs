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

    [Authorize(Roles = "Customer")]
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
    [Authorize(Roles = "Customer")]
    [HttpGet("getAllGames")]
    [Authorize(Policy = "CanViewAllGames")]
    public IActionResult getAllGames()
    {
        string? userId = this.User.FindFirst("id")?.Value;
        int id = int.Parse(userId);
        IEnumerable<ReturnGamesToCustomerDTO> list = this.opHelper.returnGamesToCust(id);
        
        if (list != null)
        {
            return Ok(list);
        }
        return BadRequest(new{message = "Games not found"});
    }
    [HttpGet("getSingleGame")]
    [Authorize(Policy = "CanViewAllGames")]
    [Authorize(Roles = "Customer, Admin, Super Admin")]
    public IActionResult getGameById(int gameId)
    {
        ReturnGamesToCustomerDTO game = this.opHelper.returnSingleGameToCust(gameId);
        
        if (game != null)
        {
            return Ok(game);
        }
        return BadRequest(new{message = "Game not found"});
    }
    [Authorize(Roles = "Customer")]
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
    [Authorize(Roles = "Customer")]
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
    [Authorize(Roles = "Customer")]
    [HttpPost("rateGame")]
    [Authorize(Policy = "CanRateGames")]
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
    [Authorize(Roles = "Customer, Admin, Super Admin")]
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
    [Authorize(Roles = "Customer")]
    [HttpGet("getCartItems")]
    [Authorize(Policy = "CanViewOwnCart")]
    public IActionResult getCartItems()
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        if (!this.opHelper.userExists(userId))
        {
            return BadRequest(new{message = "User not found"});
        }
        List<ReturnCartGamesDTO> games = this.opHelper.getCartGames(userId);
        if (games != null)
        {
            return Ok(games);
        } 
        return Ok(new List<ReturnCartGamesDTO>());
    }
    [Authorize(Roles = "Customer")]
    [HttpDelete("deleteFromCart")]
    [Authorize(Policy = "CanRemoveGameFromCart")]
    public IActionResult deleteFromCart(int gameId)
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        if (!this.opHelper.userExists(userId))
        {
            return BadRequest(new{message = "User not found"});
        }
        if (this.opHelper.deleteFromCart(userId, gameId))
        {
            return Ok(new { message = "Game removed from cart!" });
        }
        return BadRequest("Game could not be removed from cart!");
    }
    [Authorize(Roles = "Customer")]
    [HttpDelete("clearCart")]
    [Authorize(Policy = "CanRemoveGameFromCart")]
    public IActionResult clearCart()
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        if (!this.opHelper.userExists(userId))
        {
            return BadRequest(new{message = "User not found"});
        }
        if (this.opHelper.clearCart(userId))
        {
            return Ok(new { message = "Cart cleared!" });
        }
        return BadRequest("Cart could not be cleared!");
    }
    [Authorize(Roles = "Customer")]
    [HttpPost("checkOut")]
    [Authorize(Policy = "CanCheckOut")]
    public IActionResult checkOut()
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        if (!this.opHelper.userExists(userId))
        {
            return BadRequest(new{message = "User not found"});
        }
        if (this.opHelper.checkOut(userId))
        {
            return Ok(new { message = "Download started!" });
        }
        return BadRequest("Cart could not be cleared!");
    }
    [Authorize(Roles = "Customer")]
    [HttpGet("getCartCount")]
    [Authorize(Policy = "CanAddGameToCart")]
    public int getCartCount()
    {
        string? id = this.User.FindFirst("id")?.Value;
        int userId = int.Parse(id);
        return this.opHelper.getCartCount(userId);
    }
    [Authorize(Roles = "Customer")]
    [HttpGet("isGameOwned")]
    public bool alreadyOwned(int gameId)
    {
       string? id = this.User.FindFirst("id")?.Value;
       int userId = int.Parse(id);
       return this.opHelper.alreadyOwned(userId,gameId);
    }
}