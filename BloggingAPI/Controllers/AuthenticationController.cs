using BloggingAPI.Domain.Entities.Dtos.Requests.Auth;
using BloggingAPI.Domain.Entities.Dtos.Responses;
using BloggingAPI.Infrastructure.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BloggingAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    //[ApiExplorerSettings(GroupName = "v1")]

    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationDto userRegistrationDto)
        {
            var result = await _authenticationService.RegisterUser(userRegistrationDto);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            var result = await _authenticationService.ValidateUser(userLoginDto);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            var result = await _authenticationService.ForgotPasswordRequestAsync(forgotPasswordDto);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(PasswordResetDto passwordResetDto)
        {
            var result = await _authenticationService.PasswordResetAsync(passwordResetDto);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(GetNewTokenDto tokenDto)
        {
            var result = await _authenticationService.RefreshToken(tokenDto);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete("delete-user")]
        public async Task<IActionResult> DeleteUser(string userEmail)
        {
            var result = await _authenticationService.DeleteUser(userEmail);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("users")]
        public async Task<IActionResult> GetAllRegisteredUsers()
        {
            var result = await _authenticationService.GetUsers();
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("addRolesToUsers")]
        public async Task<IActionResult> AddRolesToUsers(AddUserToRoleDto addUserToRoleDto)
        {
            var result = await _authenticationService.AddUserToRoleAsync(addUserToRoleDto);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("userRoles")]
        public async Task<IActionResult> GetUserRoles(string email)
        {
            var result = await _authenticationService.GetUserRolesAsync(email);
            return StatusCode(result.StatusCode, result);
        }
    }
}
