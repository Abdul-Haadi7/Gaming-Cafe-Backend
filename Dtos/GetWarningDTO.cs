using GAME_CAFE.Dtos;
using Microsoft.AspNetCore.WebUtilities;
public class GetWarningDTO
{
    public int id {get;set;}
    public int gameId {get;set;}
    public string reason {get;set;}
    public int issuedBy {get;set;}
    public DateTime issuedAt {get;set;}
    public bool requestedToEnd {get;set;}
    public string developerName{get;set;}
}