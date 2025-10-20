namespace LAF.Dtos;

public class AuthenticationResponse
{
    public string Token { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
}