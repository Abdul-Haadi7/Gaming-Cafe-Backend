namespace GAME_CAFE.Dtos;
public class CreateAccountDTO
{
    public string Name{get;set;}
    public string Email{get;set;}
    public string Phone{get;set;}
    public string password{get;set;}
    public string role {get;set;}
    public CreateAccountDTO()
    {
        this.Name = "";
        this.Email = "";
        this.Phone = "";
        this.role = "";
        this.password = "";
   }
}