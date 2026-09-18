namespace GAME_CAFE.Dtos;
public class ReturnDeveloperToAdminDTO
{
    public int id{get;set;}
    public string name{get;set;}
    public string email{get;set;}
    public string phone{get;set;}
    public int totalActiveGames{get;set;}
    public bool isActive{get;set;}
    public ReturnDeveloperToAdminDTO()
    {
        this.id = 0;
        this.name = "";
        this.email = "";
        this.phone = "";
        this.totalActiveGames = 0;
        this.isActive = true;
   }
}