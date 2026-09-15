using GAME_CAFE.Dtos;
public class ReturnWarningEndReqDTO
{
    public int id{get;set;}
    public int warningId { get; set; }
    public string reason{get;set;}
    public string requestNote { get; set; } = "";
    public int gameId{get;set;}
    public string gameName{get;set;}
    public string developerName{get;set;}
    public bool isAccepted {get;set;}
    public bool isRejected{get;set;}
    public bool doNotShowAgain{get;set;}
  

}