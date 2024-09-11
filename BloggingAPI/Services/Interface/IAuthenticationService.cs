using BloggingAPI.Contracts.Dtos.Requests.Auth;
using BloggingAPI.Contracts.Dtos.Responses;
using BloggingAPI.Contracts.Dtos.Responses.Auth;
using BloggingAPI.Domain.Entities;
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
        Task<ApiResponse<IEnumerable<UserResponseDto>>> GetUsers();
        Task<ApiResponse<string>> ForgotPasswordRequestAsync(ForgotPasswordDto forgotPasswordDto);
        Task<ApiResponse<object>> PasswordResetAsync(PasswordResetDto passwordResetDto);
        Task<ApiResponse<object>> UserEmailConfirmation(string token, string email);
        Task<ApiResponse<object>> AddUserToRoleAsync(AddUserToRoleDto addUserToRoleDto);
        Task<ApiResponse<object>> RemoveUserFromRoleAsync(RemoveUserFromRoleDto removeUserFromRoleDto);
        Task<ApiResponse<IEnumerable<string>>> GetUserRolesAsync(string email);
        Task<ApiResponse<IEnumerable<UserResponseDto>>> GetUsersByRoleAsync(string roleName);   
        Task<ApiResponse<(UpdateUserDto userToPatch, ApplicationUser userEntity)>> GetUserForPatchAsync();
        Task<ApiResponse<object>> SaveChangesForPatch(UpdateUserDto userToPatch, ApplicationUser userEntity);
        Task<ApiResponse<object>> ChangeUserPasswordAsync(ChangePasswordDto changePasswordDto);
        Task<ApiResponse<object>> ChangeUserEmailAsync(ChangeEmailDto changeEmailDto);
        Task<ApiResponse<object>> NewUserEmailConfirmation(string token, string oldEmail, string newEmail);
        Task<ApiResponse<object>> UpdateUserProfileDto(UpdateUserDto updateUserDto);
    }
}
