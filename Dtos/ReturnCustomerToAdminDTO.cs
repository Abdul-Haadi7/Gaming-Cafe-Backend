namespace GAME_CAFE.Dtos;
public class ReturnCustomerToAdminDTO
{
    public int id{get;set;}
    public string name{get;set;}
    public string email{get;set;}
    public string phone{get;set;}
    public int totalGamesBought{get;set;}
    public bool isActive{get;set;}
    public ReturnCustomerToAdminDTO()
    {
        this.id = 0;
        this.name = "";
        this.email = "";
        this.phone = "";
        this.totalGamesBought = 0;
        this.isActive = true;
   }
}