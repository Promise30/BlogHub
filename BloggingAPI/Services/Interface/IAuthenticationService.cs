using BloggingAPI.Contracts.Dtos.Requests.Auth;
using BloggingAPI.Contracts.Dtos.Responses;
using Microsoft.AspNetCore.Identity;

namespace BloggingAPI.Services.Interface
{
    public interface IAuthenticationService
    {
        Task<ApiResponse<object>> RegisterUser(UserRegistrationDto userRegistrationDto);
        Task<ApiResponse<TokenDto>> ValidateUser(UserLoginDto userLoginDto);
        Task<TokenDto> CreateToken(bool populateExp);
        Task<ApiResponse<TokenDto>> RefreshToken(GetNewTokenDto tokenDto);
        Task<ApiResponse<object>> DeleteUser(string userEmail);
        Task<ApiResponse<IEnumerable<object>>> GetUsers();
        Task<ApiResponse<string>> ForgotPasswordRequestAsync(ForgotPasswordDto forgotPasswordDto);
        Task<ApiResponse<object>> PasswordResetAsync(PasswordResetDto passwordResetDto);
        Task<ApiResponse<object>> UserEmailConfirmation(string token, string email);
        Task<ApiResponse<object>> AddUserToRoleAsync(AddUserToRoleDto addUserToRoleDto);
        Task<ApiResponse<object>> RemoveUserFromRoleAsync(RemoveUserFromRoleDto removeUserFromRoleDto);
        Task<ApiResponse<IEnumerable<string>>> GetUserRolesAsync(string email);
    }
}
