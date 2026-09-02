using GAME_CAFE.Data;
using GAME_CAFE.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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

    public Boolean gameBelongsToDeveloper(int gameId, int developerId)
    {
        string sql = "SELECT * FROM Games WHERE id = @gameId AND developerId = @developerId";
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
}