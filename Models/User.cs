namespace GAME_CAFE.Models;

public class User
{
    public int id{get;set;}
    public string Name{get;set;}
    public string Email{get;set;}
    public string Phone{get;set;}
    public Boolean isActive{get;set;}
    public User()
    {
        this.id=0;
        this.Name = "";
        this.Email = "";
        this.Phone = "";
        this.isActive = true;
   }
}