using BloggingAPI.Domain.Entities.Dtos.Requests.Auth;
using BloggingAPI.Domain.Entities.Dtos.Responses;
using Microsoft.AspNetCore.Identity;

namespace BloggingAPI.Infrastructure.Services.Interface
{
    public interface IAuthenticationService
    { 
        Task<ApiResponse<IdentityResult>> RegisterUser(UserRegistrationDto userRegistrationDto);
        Task<ApiResponse<TokenDto>> ValidateUser(UserLoginDto userLoginDto);
        Task<TokenDto> CreateToken(bool populateExp);
        Task<ApiResponse<TokenDto>> RefreshToken(GetNewTokenDto tokenDto);
        Task<ApiResponse<object>> DeleteUser(string userEmail);
        Task<ApiResponse<IEnumerable<object>>> GetUsers();
        Task<ApiResponse<string>> ForgotPasswordRequestAsync(ForgotPasswordDto forgotPasswordDto);
        Task<ApiResponse<object>> PasswordResetAsync(PasswordResetDto passwordResetDto);
        Task<ApiResponse<object>> UserEmailConfirmation(string token, string email);
        Task<ApiResponse<object>> AddUserToRoleAsync(AddUserToRoleDto addUserToRoleDto);
        Task<ApiResponse<IEnumerable<string>>> GetUserRolesAsync(string email);
    }
}
