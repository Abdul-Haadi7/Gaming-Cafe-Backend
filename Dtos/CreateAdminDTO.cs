namespace GAME_CAFE.Dtos;
public class CreateAdminDTO
{
    public string Name{get;set;}
    public string Email{get;set;}
    public string Phone{get;set;}
    public DateTime dateOfBirth {get;set;}
    public string password{get;set;}
    public string role {get;set;}

    //While creating admin, the super admin will choose which permissions should be given to this admin out of all admin permissions. 
    public IEnumerable<int> permissions {get;set;}
    public CreateAdminDTO()
    {
        this.Name = "";
        this.Email = "";
        this.Phone = "";
        this.role = "";
        this.password = "";
    }
}