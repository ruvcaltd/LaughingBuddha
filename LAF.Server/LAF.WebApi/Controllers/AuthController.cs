namespace LAF.WebApi.Controllers;

using LAF.Dtos;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    public AuthController(IAuthenticationService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="request">The login credentials</param>
    /// <returns>Authentication response with JWT token if successful</returns>
    /// <response code="200">Returns the authentication response with token</response>
    /// <response code="401">If the credentials are invalid</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> Login(AuthenticationRequest request)
    {
        var response = await _authService.AuthenticateAsync(request);

        if (response == null)
        {
            return Unauthorized();
        }

        return Ok(response);
    }
}