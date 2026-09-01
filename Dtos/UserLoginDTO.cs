namespace GAME_CAFE.Dtos;

public class UserLoginDTO
{
    public string Email{get;set;}
    public string Password{get;set;}

    public UserLoginDTO()
    {
        this.Email = "";
        this.Password = "";
    }
}