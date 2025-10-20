
using LAF.Dtos;
using System.Threading.Tasks;

public interface IAuthenticationService
{
    Task<AuthenticationResponse?> AuthenticateAsync(AuthenticationRequest request);
}