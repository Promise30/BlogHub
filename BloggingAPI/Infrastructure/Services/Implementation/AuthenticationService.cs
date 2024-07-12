using BloggingAPI.Domain.Entities;
using BloggingAPI.Domain.Entities.Dtos.Requests.Auth;
using BloggingAPI.Domain.Entities.Dtos.Responses;
using BloggingAPI.Domain.Entities.Dtos.Responses.Posts;
using BloggingAPI.Infrastructure.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BloggingAPI.Infrastructure.Services.Implementation
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILogger<AuthenticationService> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private User _user;
        public AuthenticationService(ILogger<AuthenticationService> logger, UserManager<User> userManager, IConfiguration configuration)
        {
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
        }
        public async Task<ApiResponse<IdentityResult>> RegisterUser(UserRegistrationDto userRegistrationDto)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(userRegistrationDto.Email);
                if (existingUser != null)
                {
                    _logger.Log(LogLevel.Information, $"Existing user found when trying to register new user with email: {userRegistrationDto.Email} at {DateTime.Now.ToString("yy-MM-dd:H m s")}");
                    return ApiResponse<IdentityResult>.Failure(400, "User already exists.");
                }

                var user = new User
                {
                    UserName = userRegistrationDto.UserName,
                    Email = userRegistrationDto.Email,
                    PhoneNumber = userRegistrationDto.PhoneNumber,
                    FirstName = userRegistrationDto.FirstName,
                    LastName = userRegistrationDto.LastName,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    RefreshToken = string.Empty,
                    
                };
                var result = await _userManager.CreateAsync(user, userRegistrationDto.Password);
                if (!result.Succeeded)
                {
                    _logger.Log(LogLevel.Information, $"Error occured while creating new user {userRegistrationDto.Email}: {result.Errors}");
                    return ApiResponse<IdentityResult>.Failure(400, result.Errors.ToString());
                }

                _logger.Log(LogLevel.Information, $"New user created with username-> {userRegistrationDto.UserName} at {DateTime.Now.ToString("yy-MM-dd:H m s")}");
                await _userManager.AddToRolesAsync(user, userRegistrationDto.Roles);
                return ApiResponse<IdentityResult>.Success(200,result, "Registration Successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occured while adding a registering a new user: {ex.Message}");
                return ApiResponse<IdentityResult>.Failure(500, "User could not be created");
            }
        }

        public async Task<ApiResponse<TokenDto>> ValidateUser(UserLoginDto userLoginDto)
        {
            try
            {
                _user = await _userManager.FindByNameAsync(userLoginDto.UserName);

                var result = (_user != null && await _userManager.CheckPasswordAsync(_user, userLoginDto.Password));
                if (!result)
                {
                    _logger.Log(LogLevel.Warning, $"{nameof(ValidateUser)}: Authentication failed. Wrong user name or password.");
                    return ApiResponse<TokenDto>.Failure(401, "Authentication failed. Invalid credentials");
                }
                var token = await CreateToken(true);
                _logger.Log(LogLevel.Information, $"New token credentials created for {_user.UserName}: {JsonSerializer.Serialize(token)}");
                return ApiResponse<TokenDto>.Success(200, token, "Validation successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occured while validating user credentials: {ex.Message}");
                return ApiResponse<TokenDto>.Failure(500, "User authentication failed.");
                
            }
        }

        
        public async Task<ApiResponse<TokenDto>> RefreshToken(GetNewTokenDto tokenDto)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(tokenDto.AccessToken);
                var user = await _userManager.FindByNameAsync(principal.Identity.Name);
                if (user == null || user.RefreshToken != tokenDto.RefreshToken ||
                user.RefreshTokenExpiryDate <= DateTime.Now)
                    return ApiResponse<TokenDto>.Failure(400, "Invalid client request. The tokenDto has some invalid values.");
                _user = user;
                var newToken = await CreateToken(populateExp: false);
                return ApiResponse<TokenDto>.Success(200, newToken, "Request Successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occured while trying to generate new access and refresh token for user: {ex.Message}");
                return ApiResponse<TokenDto>.Failure(500, "Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<string>> ForgotPasswordRequestAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
                if (user is null)
                    return ApiResponse<string>.Success(400, null, "Request unsuccessful");
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                _logger.Log(LogLevel.Information, $"Password reset token generated for {user.UserName} at {DateTime.Now}: {token}");
                return ApiResponse<string>.Success(200, token, null);
                
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occured while trying to perform the forgot password operation: {ex.Message}");
                return ApiResponse<string>.Failure(500, "Request unsuccessful");
            }
        }
        public async Task<ApiResponse<object>> PasswordResetAsync(PasswordResetDto passwordResetDto)
        {
            try
            {
                _user = await _userManager.FindByEmailAsync(passwordResetDto.Email);
                if (_user is null)
                    return ApiResponse<object>.Failure(404, null, "User does not exist");
                var resetPasswordResult = await _userManager.ResetPasswordAsync(_user, passwordResetDto.Token, passwordResetDto.Password);
                if (!resetPasswordResult.Succeeded)
                {
                    _logger.Log(LogLevel.Information, $"Error occured while trying to reset password for {_user.UserName}: {resetPasswordResult.Errors}");
                    return ApiResponse<object>.Failure(400, resetPasswordResult.Errors.ToList(), "Request unsuccessful.");
                }
                    
                _logger.Log(LogLevel.Information, $"Password successfully reset at {DateTime.Now} for {_user.UserName}");
                return ApiResponse<object>.Success(200, null, "Password successfully changed.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occured while trying to reset the password: {ex.Message}");
                return ApiResponse<object>.Failure(500, "Request unsuccessful");
            }
        }
        public async Task<ApiResponse<object>> UserEmailConfirmation(string token, string email)
        {
            try
            {
                _user = await _userManager.FindByEmailAsync(email);
                if (_user is null)
                {
                    return ApiResponse<object>.Failure(404, null, "User does not exist");
                }
                var result = await _userManager.ConfirmEmailAsync(_user, token);
                if (result.Succeeded)
                {
                    _logger.Log(LogLevel.Information, $"Email confirmation successful for {_user.UserName} at {DateTime.Now}");
                    return ApiResponse<object>.Success(200, "User email verification successful", null);
                }
                return ApiResponse<object>.Failure(400, result.Errors.ToList(), "User email verification failed.");

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occured while trying to confirm user email: {ex.Message}");
                return ApiResponse<object>.Failure(500, "Request unsuccessful");
            }
        }

        public async Task<ApiResponse<object>> DeleteUser(string userEmail)
        {
            try
            {
                _user = await _userManager.FindByEmailAsync(userEmail);
                if (_user is null)
                    return ApiResponse<object>.Failure(404, "User with the specified email does not exist");
                var result = await _userManager.DeleteAsync(_user);
                if (!result.Succeeded)
                {
                    _logger.Log(LogLevel.Information, $"Error occured while deleting user with username: {_user.UserName}");
                    return ApiResponse<object>.Failure(400, result.Errors.ToString());
                }
                _logger.Log(LogLevel.Information, $"{_user.UserName} deleted successfully.");
                return ApiResponse<object>.Success(204, "Request successful");

            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, ex.Message);
                return ApiResponse<object>.Failure(400, "User could not be deleted.");
            }
        }
        public async Task<ApiResponse<IEnumerable<object>>> GetUsers()
        {
            try
            {
                var users = _userManager.Users;
                _logger.Log(LogLevel.Information, $"Total number of users retrieved from the database: {users.Count()}");
                return ApiResponse<IEnumerable<object>>.Success(200, users, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, ex.Message);
                return ApiResponse<IEnumerable<object>>.Failure(500, "Error occured while retrieving users from the database.");
            }
        }
        public async Task<ApiResponse<object>> AddUserToRoleAsync(AddUserToRoleDto addUserToRoleDto)
        {
            try
            {
                _user = await _userManager.FindByEmailAsync(addUserToRoleDto.Email);
                if (_user == null)
                    return ApiResponse<object>.Failure(400, "User does not exist");
                var result = await _userManager.AddToRolesAsync(_user, addUserToRoleDto.Roles.ToList());
                if (!result.Succeeded)
                {
                    _logger.Log(LogLevel.Information, $"Error occured while adding roles to user: {result.Errors.ToList()}");
                    return ApiResponse<object>.Failure(400, "Request unsuccesful");
                }

                return ApiResponse<object>.Success(204, null);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log (LogLevel.Information, $"Error occured while adding roles to user: {ex.Message}");
                return ApiResponse<object>.Failure(400, "Request unsuccesful");
            }
        }
        public async Task<ApiResponse<IEnumerable<string>>> GetUserRolesAsync(string email)
        {
            try
            {
                _user = await _userManager.FindByEmailAsync(email);
                if (_user == null)
                    return ApiResponse<IEnumerable<string>>.Failure(400, "User does not exist");
                var result = await _userManager.GetRolesAsync(_user);
                
                _logger.Log(LogLevel.Information, $"Roles assigned to {_user.Email}: {result.ToList()}");
                return ApiResponse<IEnumerable<string>>.Success(200, result, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, $"Error occured while retrieving user roles: {ex.Message}");
                return ApiResponse<IEnumerable<string>>.Failure(400, "Request unsuccesful");
            }
        }
        
        #region Methods
        public async Task<TokenDto> CreateToken(bool populateExp)
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetClaims();
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
            var refreshToken = GenerateRefreshToken();
            _user.RefreshToken = refreshToken;
            if (populateExp)
                _user.RefreshTokenExpiryDate = DateTime.Now.AddDays(7);
            await _userManager.UpdateAsync(_user);
            var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            return new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiryDate = tokenOptions.ValidTo,
                RefreshTokenExpiryDateExpiry = _user.RefreshTokenExpiryDate
            };
        }
        private SigningCredentials GetSigningCredentials()
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["secretKey"]);
            var secret = new SymmetricSecurityKey(key);
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }
        private async Task<List<Claim>> GetClaims()
        {
            var claims = new List<Claim>
             {
             new Claim(ClaimTypes.Name, _user.UserName),
             new Claim(ClaimTypes.NameIdentifier, _user.Id),
             new Claim(ClaimTypes.Email, _user.Email),
             };
            var roles = await _userManager.GetRolesAsync(_user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return claims;
        }
        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var tokenOptions = new JwtSecurityToken
            (
            issuer: jwtSettings["validIssuer"],
            audience: jwtSettings["validAudience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["expires"])),
            signingCredentials: signingCredentials
            );
            return tokenOptions;
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["secretKey"])),
                ValidateLifetime = true,
                ValidIssuer = jwtSettings["validIssuer"],
                ValidAudience = jwtSettings["validAudience"]
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
            StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
            return principal;
        }
        #endregion
    }

}
