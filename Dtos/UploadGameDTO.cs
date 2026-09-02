namespace GAME_CAFE.Dtos;

public class UploadGameDTO
{
    public string name{get;set;}
    public double price{get;set;}
    public string intro{get;set;}
    public string description {get;set;}
    public string genre{get;set;}
    public string downloadLink{get;set;}
    public string imageLink{get;set;}
    public decimal discountPercentage{get;set;}
    public UploadGameDTO()
    {
        this.name = "";
        this.price = 0;
        this.intro = "";
        this.description = "";
        this.genre = "";
        this.downloadLink = "";
        this.imageLink = "";
        this.discountPercentage = 0;
    }
}