namespace Fora.Domain.Entities;

public class WebUser
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
}

public static class WebUserRoleEnum
{
    public readonly static string Admin = "Admin";
    public readonly static string Guest = "Guest";
}