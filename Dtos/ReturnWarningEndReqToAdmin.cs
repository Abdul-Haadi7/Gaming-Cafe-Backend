using GAME_CAFE.Dtos;
public class ReturnWarningEndReqToAdmin
{
    public int id{get;set;}
    public int warningId { get; set; }
    public string reason{get;set;}
    public string requestNote { get; set; } = "";
    public int gameId{get;set;}
    public string gameName{get;set;}
    public string developerName{get;set;}

}