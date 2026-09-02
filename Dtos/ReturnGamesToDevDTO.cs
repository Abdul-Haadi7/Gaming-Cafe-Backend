namespace GAME_CAFE.Dtos;

public class ReturnGamesToDevDTO
{
    public int id{get;set;}
    public string name{get;set;}
    public decimal price{get;set;}
    public string intro{get;set;}
    public string description {get;set;}
    public string genre{get;set;}
    public string downloadLink{get;set;}
    public string imageLink{get;set;}
    public decimal discountPercentage{get;set;}
    public bool hasWarning {get;set;}
    public bool isActive{get;set;}
}