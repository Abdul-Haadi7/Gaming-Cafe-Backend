namespace GAME_CAFE.Models;

public class GameRequirements
{
    public int gameId {get;set;}
    public string os {get;set;} 
    public string processor {get;set;}
    public string ram {get;set;}
    public string graphicsCard {get;set;}

    public string storage {get;set;}
    public GameRequirements()
    {
        this.gameId = 0;
        this.os = "";
        this.processor = "";
        this.ram = "";
        this.graphicsCard = "";
        this.storage = "";
    }
}

