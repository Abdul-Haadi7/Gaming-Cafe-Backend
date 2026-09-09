namespace GAME_CAFE.Models;

public class ReturnCartGamesDTO
{
    public int id{get;set;}
    public string name{get;set;}
    public decimal price{get;set;}
    public ReturnCartGamesDTO()
    {
        this.id=0;
        this.name = "";
        this.price = 0;
    }
    public ReturnCartGamesDTO(int id,string name,decimal price)
    {
        this.id = id;
        this.name = name;
        this.price = price;
    }
}

