namespace GAME_CAFE.Dtos;

public class ReturnGamesToAdminDTO
{
    public int id{get;set;}
    public string name{get;set;}
    public decimal price{get;set;}
    public string genre{get;set;}
    public decimal discountPercentage{get;set;}
    public string developerName {get;set;}
    public decimal rating {get;set;}
    public bool hasWarning{get;set;}
    public bool isPublic{get;set;}
}