namespace GAME_CAFE.Dtos;

public class ReturnPendingUploadReqToDev
{
    public int id{get;set;}
    public string name{get;set;}
    public decimal price{get;set;}
    public string genre{get;set;}
    public string status {get;set;}
    public bool isApproved{get;set;}
    public bool isRejected {get;set;}
    public string rejectionReason{get;set;}
}