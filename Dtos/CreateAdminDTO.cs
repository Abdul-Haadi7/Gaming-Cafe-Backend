namespace GAME_CAFE.Dtos;
public class CreateAdminDTO
{
    public string name{get;set;}
    public string email{get;set;}
    public string phone{get;set;}
    public string password{get;set;}
    public string role {get;set;}

    //While creating admin, the super admin will choose which permissions should be given to this admin out of all admin permissions. 
    public IEnumerable<int> permissions {get;set;}
    public CreateAdminDTO()
    {
        this.name = "";
        this.email = "";
        this.phone = "";
        this.role = "";
        this.password = "";
    }
}