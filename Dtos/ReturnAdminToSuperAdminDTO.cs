namespace GAME_CAFE.Dtos;
public class ReturnAdminToSuperAdminDTO
{
    public int id{get;set;}
    public string name{get;set;}
    public string email{get;set;}
    public string phone{get;set;}
    public bool isActive{get;set;}
    public IEnumerable<string> permissions{get;set;}
    public ReturnAdminToSuperAdminDTO()
    {
        this.id = 0;
        this.name = "";
        this.email = "";
        this.phone = "";
        this.isActive = true;
   }
}