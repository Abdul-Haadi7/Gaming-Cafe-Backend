using GAME_CAFE.Dtos;

public class TempAdminDTO{
    public int id { get; set; }
    public string name { get; set; }
    public string email { get; set; }
    public string phone { get; set; }
    public bool isActive { get; set; }
    public string? permissions { get; set; }
}