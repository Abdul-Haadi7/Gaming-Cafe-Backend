using System.Reflection.Metadata.Ecma335;
using GAME_CAFE.Data;
using GAME_CAFE.Dtos;
using GAME_CAFE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.JSInterop.Infrastructure;

namespace GAME_CAFE.Helper;

public class OperationsHelper
{
    private readonly IConfiguration _config;
    private readonly DataContextDapper _dapper;

    public OperationsHelper(IConfiguration con)
    {
        this._config = con;
        this._dapper = new DataContextDapper(this._config);
    }

    public string getName(int userId)
    {
        string sql = "SELECT name FROM Users WHERE id = @id";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@id", userId)
        };

        string? name = this._dapper.returnSingle_WithParameters<string>(sql, parameters);
        return name;
    }
    // public IActionResult uploadGame(UploadGameDTO game, int developerId)
    // {
    //     string sql = @"INSERT INTO Games (name, price, intro, description, genre, downloadLink, imageLink, discountPercentage, developerId) 
    //                    VALUES (@name, @price, @intro, @description, @genre, @downloadLink, @imageLink, @discountPercentage, @developerId)";
    //     List<SqlParameter> parameters = new List<SqlParameter>
    //     {
    //         new SqlParameter("@name", game.name),
    //         new SqlParameter("@price", game.price),
    //         new SqlParameter("@intro", game.intro),
    //         new SqlParameter("@description", game.description),
    //         new SqlParameter("@genre", game.genre),
    //         new SqlParameter("@downloadLink", game.downloadLink),
    //         new SqlParameter("@imageLink", game.imageLink),
    //         new SqlParameter("@discountPercentage", game.discountPercentage),
    //         new SqlParameter("@developerId", developerId)
    //     };

    //     bool success = this._dapper.ExecuteSQL_WithParameters(sql, parameters);
    //     if (success)
    //     {
    //         return new OkObjectResult(new { message = "Game uploaded successfully!" });
    //     }
    //     else
    //     {
    //         return new BadRequestObjectResult(new { message = "Failed to upload the game." });
    //     }
    // }
    public int uploadGame(UploadGameDTO game, int developerId)
    {
        string sql = @"
            INSERT INTO Games 
                (name, price, intro, description, genre, downloadLink, imageLink, discountPercentage, developerId) 
            OUTPUT INSERTED.id
            VALUES 
                (@name, @price, @intro, @description, @genre, @downloadLink, @imageLink, @discountPercentage, @developerId)";

        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@name", game.name),
            new SqlParameter("@price", game.price),
            new SqlParameter("@intro", game.intro),
            new SqlParameter("@description", game.description),
            new SqlParameter("@genre", game.genre),
            new SqlParameter("@downloadLink", game.downloadLink),
            new SqlParameter("@imageLink", game.imageLink),
            new SqlParameter("@discountPercentage", game.discountPercentage),
            new SqlParameter("@developerId", developerId)
        };

        int gameId = this._dapper.ExecuteScalar_WithParameters(sql, parameters);

        return gameId;
    }

    private IActionResult BadRequest(object value)
    {
        throw new NotImplementedException();
    }

    private IActionResult Ok(object value)
    {
        throw new NotImplementedException();
    }

    public bool gameBelongsToDeveloper(int gameId, int developerId)
    {
        if (!this.gameExists(gameId))
        {
            return false;
        }
        string sql = @"SELECT COUNT(*) FROM Games WHERE id = @gameId 
                   AND developerId = @developerId";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@gameId", gameId),
            new SqlParameter("@developerId", developerId)
        };

        int count = this._dapper.returnSingle_WithParameters<int>(sql, parameters);
        return count > 0;
    }
    public Boolean gameExists(int gameId)
    {
        string sql = "SELECT COUNT(*) FROM Games WHERE id = @gameId";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@gameId", gameId)
        };

        int count = this._dapper.returnSingle_WithParameters<int>(sql, parameters);
        return count > 0;
    }
    public IActionResult uploadGameRequirements(GameRequirementsDTO requirements)
    {
        string sql = @"INSERT INTO Game_Requirements (gameId, os, processor, ram, graphicsCard, storage) 
                       VALUES (@gameId, @os, @processor, @ram, @graphicsCard, @storage)";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@gameId", requirements.gameId),
            new SqlParameter("@os", requirements.os),
            new SqlParameter("@processor", requirements.processor),
            new SqlParameter("@ram", requirements.ram),
            new SqlParameter("@graphicsCard", requirements.graphicsCard),
            new SqlParameter("@storage", requirements.storage)
        };

        bool success = this._dapper.ExecuteSQL_WithParameters(sql, parameters);
        if (success)
        {
            return new OkObjectResult(new { message = "Game requirements uploaded successfully!" });
        }
        else
        {
            return new BadRequestObjectResult(new { message = "Failed to upload the game requirements." });
        }
    }
    public IEnumerable<ReturnGamesToDevDTO> getGames(int devId)
    {
        string sql = @"SELECT id,name,price,intro,description,genre,downloadLink,imageLink,
        discountPercentage,hasWarning,isActive,isPublic FROM Games WHERE developerId = @devId";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@devId", devId),
        };
        IEnumerable<ReturnGamesToDevDTO> list = this._dapper.loadObject_WithParameters<ReturnGamesToDevDTO>(sql,parameters);
        int numOfRatings=0;
        decimal totalRating = 0;
        decimal avgRating=0;
        foreach (var game in list)
        {
            game.rating = 0;
            game.soldAmount = 0;
            game.earned = 0;
            game.soldAmount = this.getTotalSold(game.id);
            game.earned = this.getMoneyEarned(game.id);
            numOfRatings = this.getRatingAmount(game.id);
            totalRating = this.getRatingTotal(game.id);
            if(numOfRatings == 0 || totalRating == 0)
            {
                continue;
            }
            avgRating = totalRating/numOfRatings;
            game.rating = avgRating;
            numOfRatings = 0;
            totalRating = 0;
            avgRating = 0;
            
        }
        return list;
    }
    public int getRatingAmount(int gameId)
    {
        //Count the number of ratings
        string sqlCount = @"SELECT COUNT(*) FROM Game_Ratings WHERE gameId = @id";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@id", gameId)
        };
        int numOfRatings=0;
        numOfRatings = this._dapper.returnSingle_WithParameters<int>(sqlCount,parameters);
        return numOfRatings;
    }
    public decimal getRatingTotal(int gameId)
    {
        //Count the number of ratings
        string sqlCount = @"SELECT SUM(ratingGiven) FROM Game_Ratings WHERE gameId = @id";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@id", gameId)
        };
        decimal numOfRatings=0;
        numOfRatings = this._dapper.returnSingle_WithParameters<decimal>(sqlCount,parameters);
        return numOfRatings;
    }
    public int getTotalSold(int gameId)
    {
        int sold = 0;
        string sql = @"SELECT COUNT(*) FROM Sale_Records WHERE gameId = @id"; 
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@id", gameId)
        };
        sold = this._dapper.returnSingle_WithParameters<int>(sql,parameters);
        return sold;
    }
    public decimal getMoneyEarned(int gameId)
    {
        decimal total = 0;
        string sql = @"SELECT SUM(price) FROM Sale_Records WHERE gameId = @id"; 
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@id", gameId)
        };
        total = this._dapper.returnSingle_WithParameters<decimal>(sql,parameters);
        return total;
    }
    public Boolean userExists(int userId)
    {
        string sql = "SELECT * FROM Users WHERE id = @userId";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@userId", userId),
        };
        int n = this._dapper.returnSingle_WithParameters<int>(sql,parameters);
        return n>0;
    }
    public IActionResult deleteGame(int gameId)
    {
        string sql = @"UPDATE Games SET isActive = 0 WHERE id = @id";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@id", gameId),
        };
        bool deleted = this._dapper.ExecuteSQL_WithParameters(sql,parameters);
        if (deleted)
        {
            return new OkObjectResult(new { message = "Game deleted!" });
        }
        return BadRequest (new {message = "Could not delete the game!"});
    }
    public EditGameDTO? getGameById(int gameId)
    {
        string sql = @"SELECT id,name,price,intro,description,genre,downloadLink,imageLink,
        discountPercentage FROM Games WHERE id = @id";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@id", gameId),
        };
        EditGameDTO? game = this._dapper.returnSingleObj_WithParameters<EditGameDTO>(sql,parameters);
        
        return game;
    }
    public ReturnGameReqDTO? getReqById(int gameId)
    {
        string sql = @"SELECT os,processor,ram,graphicsCard,storage FROM Game_Requirements 
        WHERE gameId = @id";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@id", gameId),
        };
        ReturnGameReqDTO? req = this._dapper.returnSingleObj_WithParameters<ReturnGameReqDTO>(sql,parameters);
        return req;
    }
    public bool editGame(EditGameDTO editedGame,int gameId)
    {
        string sql = @"UPDATE Games SET name = @newName, price = @newPrice, genre = @newGenre,
        intro = @newIntro, description = @newDesc, downloadLink = @newDownLink,
        imageLink = @newImgLink, discountPercentage = @newDis WHERE id = @id";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@newName", editedGame.name),
            new SqlParameter("@newPrice", editedGame.price),
            new SqlParameter("@newGenre", editedGame.genre),
            new SqlParameter("@newIntro", editedGame.intro),
            new SqlParameter("@newDesc", editedGame.description),
            new SqlParameter("@newDownLink", editedGame.downloadLink),
            new SqlParameter("@newImgLink", editedGame.imageLink),
            new SqlParameter("@newDis", editedGame.discountPercentage),
            new SqlParameter("@id", gameId)
        };
        return this._dapper.ExecuteSQL_WithParameters(sql,parameters);
    }
    public bool editRequirements(EditRequirementsDTO requirements,int gameId)
    {
        string sql = @"UPDATE Game_Requirements SET os = @newOS,
        processor = @newProc,
        ram = @newRAM,
        graphicsCard = @newGPU,
        storage = @newSto WHERE gameId = @id";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@newOS", requirements.os),
            new SqlParameter("@newProc", requirements.processor),
            new SqlParameter("@newRAM", requirements.ram),
            new SqlParameter("@newGPU", requirements.graphicsCard),
            new SqlParameter("@newSto", requirements.storage),
            new SqlParameter("@id", gameId)
        };
        return this._dapper.ExecuteSQL_WithParameters(sql,parameters);
    }
    public bool toggleAvailability(int gameId)
    {
        string sql = @"UPDATE Games SET isPublic = ~isPublic WHERE id = @id";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@id", gameId)
        };
        bool done = this._dapper.ExecuteSQL_WithParameters(sql,parameters);
         //If game is set to private, remove is from carts of customers
        if(!this.getCurrentAvailability(gameId))
        {
            this.deleteFromAllCarts(gameId);
        }
        return done;
    }
    public bool getCurrentAvailability(int gameId)
    {
        string sql = @"SELECT isPublic FROM games WHERE id = @id";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@id", gameId)
        };
        bool avail = this._dapper.returnSingle_WithParameters<bool>(sql,parameters);
       
        return avail;
    }
    public void deleteFromAllCarts(int gameId)
    {
        String sql = @"DELETE FROM Cart WHERE gameId = @id";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@id", gameId)
        };
        this._dapper.ExecuteSQL_WithParameters(sql,parameters);
    }
    public IEnumerable<ReturnGamesToCustomerDTO> returnGamesToCust(int userId)
    {
        string sql = @"SELECT id,name,price,intro,description,genre,downloadLink,imageLink,
        discountPercentage FROM Games WHERE isActive = 1 AND isPublic = 1";
   
        IEnumerable<ReturnGamesToCustomerDTO> list = this._dapper.loadData<ReturnGamesToCustomerDTO>(sql);
        int numOfRatings=0;
        decimal totalRating = 0;
        decimal avgRating=0;
        foreach (var game in list)
        {            
            sql = @"SELECT name FROM Users u WHERE u.id = (SELECT developerId from Games g WHERE g.id = @gameId)";
    
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@gameId", game.id)
            };
            game.developerName = this._dapper.returnSingle_WithParameters<string>(sql,parameters);

            game.rating = 0;
            numOfRatings = this.getRatingAmount(game.id);
            totalRating = this.getRatingTotal(game.id);
            if(numOfRatings == 0 || totalRating == 0)
            {
                continue;
            }
            avgRating = totalRating/numOfRatings;
            game.rating = avgRating;
            numOfRatings = 0;
            totalRating = 0;
            avgRating = 0;
            game.alreadyOwned = this.alreadyOwned(userId,game.id);
        }
        return list;
    }
    public bool alreadyOwned(int userId, int gameId)
    {
        string sql = @"SELECT COUNT(*) FROM Sale_Records WHERE buyerId = @buyerId AND gameId = @gameId";
        List<SqlParameter> parameters2 = new List<SqlParameter>
        {
            new SqlParameter("@buyerId", userId),
            new SqlParameter("@gameId", gameId)
        };
        int count = this._dapper.returnSingle_WithParameters<int>(sql,parameters2);
        if (count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public ReturnGamesToCustomerDTO returnSingleGameToCust(int gameId)
    {
        string sql = @"SELECT id,name,price,intro,description,genre,downloadLink,imageLink,
        discountPercentage FROM Games WHERE id = @gameId AND isActive = 1 AND isPublic = 1";
   
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@gameId", gameId)
        };
        ReturnGamesToCustomerDTO game= this._dapper.returnSingleObj_WithParameters<ReturnGamesToCustomerDTO>(sql,parameters);
        int numOfRatings=0;
        decimal totalRating = 0;
        decimal avgRating=0;
        if(game!=null)
        {            
            sql = @"SELECT name FROM Users u WHERE u.id = (SELECT developerId from Games g WHERE g.id = @gameId)";
    
            List<SqlParameter> parameters2 = new List<SqlParameter>
            {
                new SqlParameter("@gameId", game.id)
            };
            game.developerName = this._dapper.returnSingle_WithParameters<string>(sql,parameters2);
            game.rating = 0;
            numOfRatings = this.getRatingAmount(game.id);
            totalRating = this.getRatingTotal(game.id);
            if(numOfRatings != 0 && totalRating != 0)
            {
                avgRating = totalRating/numOfRatings;
                game.rating = avgRating;
            }
     
        }
        return game;
    }
    public bool alreadyInCart(int gameId, int userId)
    {
        bool done = false;
        string sql = @"SELECT COUNT(*) FROM Cart WHERE gameId = @gameId AND buyerId = @userId";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@userId", userId),
            new SqlParameter("@gameId", gameId)
        };
        int count = this._dapper.returnSingle_WithParameters<int>(sql,parameters);
        if(count > 0)
        {
            done = true;
        }
        return done;
    }
    public bool addToCart(int gameId, int userId)
    {
        string sql = @"INSERT INTO Cart VALUES (@userId,@gameId)";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@userId", userId),
            new SqlParameter("@gameId", gameId)
        };
        bool done = this._dapper.ExecuteSQL_WithParameters(sql,parameters);
        return done;
    }
    public bool rateGame(int userId,int gameId, decimal rating)
    {
        string sql = @"INSERT INTO Game_Ratings VALUES (@userId,@gameId,@rating)";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@userId", userId),
            new SqlParameter("@gameId", gameId),
            new SqlParameter("@rating", rating)
        };
        return this._dapper.ExecuteSQL_WithParameters(sql,parameters);
    }
    public bool alreadyRated(int userId,int gameId)
    {
        string sql = @"SELECT COUNT(*) FROM Game_Ratings WHERE userId = @userId AND gameId = @gameId";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@userId", userId),
            new SqlParameter("@gameId", gameId)
        };
        int count = this._dapper.returnSingle_WithParameters<int>(sql,parameters);
        if(count>0){
            return true;
        }
        return false;
    }
    public decimal getRating(int userId, int gameId)
    {
        string sql = @"SELECT ratingGiven FROM Game_Ratings WHERE userId = @userId and gameId = @gameId";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@userId", userId),
            new SqlParameter("@gameId", gameId)
        };
        return this._dapper.returnSingle_WithParameters<decimal>(sql,parameters);
    }
    public List<ReturnCartGamesDTO> getCartGames(int userId)
    {
        List<ReturnCartGamesDTO> games = new List<ReturnCartGamesDTO>();
        string sql = @"SELECT gameId FROM Cart WHERE buyerId = @userId";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@userId", userId)
        };
        IEnumerable<int> gameIds = this._dapper.loadData_WithParameters<int>(sql,parameters);
        if(gameIds == null)
        {
            return null;
        }
        string? name="";
        decimal price = 0;
        int discount = 0;
        ReturnCartGamesDTO game;
        foreach(var id in gameIds)
        {
            //Get game`s name
            name = this.getGameName(id);

            //Get game`s real price
            price = this.getGamePrice(id);

            //Get game`s discount percentage
            discount = this.getGameDiscount(id);

            //Calculate discounted price
            if(discount > 0)
            {
                price = price - (price * discount / 100);
            }
            game = new ReturnCartGamesDTO(id,name,price);
            games.Add(game);
            name = "";
            price = discount = 0;
        }
        return games;
    }
    private string getGameName(int gameId)
    {
        string sql = @"SELECT name FROM Games WHERE id = @gameId";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@gameId", gameId)
        };
        return this._dapper.returnSingle_WithParameters<string>(sql,parameters);
    }
    private decimal getGamePrice(int gameId)
    {
        string sql = @"SELECT price FROM Games WHERE id = @gameId";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@gameId", gameId)
        };
        return this._dapper.returnSingle_WithParameters<decimal>(sql,parameters);
    }
    private int getGameDiscount(int gameId)
    {
        string sql = @"SELECT discountPercentage FROM Games WHERE id = @gameId";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@gameId", gameId)
        };
        return this._dapper.returnSingle_WithParameters<int>(sql,parameters);
    }


    public bool deleteFromCart(int userId,int gameId)
    {
        string sql = @"DELETE FROM CART WHERE buyerId = @userId AND gameId = @gameId";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@userId", userId),
            new SqlParameter("@gameId", gameId)
        };
        return this._dapper.ExecuteSQL_WithParameters(sql,parameters);
    }
    public bool clearCart(int userId)
    {
        string sql = @"DELETE FROM CART WHERE buyerId = @userId";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@userId", userId)
        };
        return this._dapper.ExecuteSQL_WithParameters(sql,parameters);
    }
    public bool checkOut(int buyerid)
    {
        //Fetch all the gameIds in customer`s cart currently
        string sql = @"SELECT gameId FROM Cart WHERE buyerId = @buyerid";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@buyerid", buyerid)
        };
        IEnumerable<int> gameIds = this._dapper.loadData_WithParameters<int>(sql,parameters);
        decimal price = 0;
        int discount = 0;
        //Insert each game into sale records one by one
        foreach(var gameId in gameIds)
        {
            price = this.getGamePrice(gameId);
            discount = this.getGameDiscount(gameId);
            if(discount > 0)
            {
                price = price - (price * discount / 100);
            }
            sql = @"INSERT INTO Sale_Records VALUES (@gameId,@buyerId,@price)";
            List<SqlParameter> parameters2 = new List<SqlParameter>
            {
                new SqlParameter("@gameId", gameId),
                new SqlParameter("@buyerId", buyerid),
                new SqlParameter("@price", price)
            };
            if (!this._dapper.ExecuteSQL_WithParameters(sql, parameters2))
            {
                return false;
            }
        }
        if (this.clearCart(buyerid))
        {
            return true;
        }
        return false;
    }
    public int getCartCount(int userId)
    {
        string sql = "SELECT COUNT(*) FROM Cart WHERE buyerId = @buyerId";
        List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@buyerId", userId)
        };
        return this._dapper.returnSingle_WithParameters<int>(sql,parameters);
    }
}