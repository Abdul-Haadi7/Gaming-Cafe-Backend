namespace GAME_CAFE.Dtos;
public class EditAdminDTO
{
    public int id{get;set;}
    public string name{get;set;}
    public string email{get;set;}
    public string phone{get;set;}
    public IEnumerable<int> permissions {get;set;}
    public EditAdminDTO()
    {
        this.id = 0;
        this.name = "";
        this.email = "";
        this.phone = "";
    }
}