namespace GAME_CAFE.Models;

public class Game
{
    public int id{get;set;}
    public string name{get;set;}
    public double price{get;set;}
    public string intro{get;set;}
    public string description {get;set;}
    public string genre{get;set;}
    public string downloadLink{get;set;}
    public string imageLink{get;set;}
    public decimal discountPercentage{get;set;}
    public int developerId{get;set;}
    public bool isActive{get;set;}
    public int? approvedBy{get;set;}
    public int? rejectedBy{get;set;}
    public string rejectionReason{get;set;}
    public Game()
    {
        this.id=0;
        this.name = "";
        this.price = 0;
        this.intro = "";
        this.description = "";
        this.genre = "";
        this.downloadLink = "";
        this.imageLink = "";
        this.discountPercentage = 0;
        this.developerId = 0;
        this.approvedBy = null;
        this.rejectedBy = null;
        this.rejectionReason = "";
    }
}

