namespace GAME_CAFE.Dtos;
public class ReturnDeveloperToAdminDTO
{
    public string name{get;set;}
    public string email{get;set;}
    public string phone{get;set;}
    public int totalActiveGames{get;set;}
    public bool isActive{get;set;}
    public ReturnDeveloperToAdminDTO()
    {
        this.name = "";
        this.email = "";
        this.phone = "";
        this.totalActiveGames = 0;
        this.isActive = true;
   }
}