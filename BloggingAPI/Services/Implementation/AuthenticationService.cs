
using BloggingAPI.Contracts.Dtos.Requests.Auth;
using BloggingAPI.Contracts.Dtos.Responses;
using BloggingAPI.Contracts.Dtos.Responses.Auth;
using BloggingAPI.Domain.Entities;
using BloggingAPI.Persistence.Extensions;
using BloggingAPI.Services.Interface;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BloggingAPI.Services.Implementation
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILogger<AuthenticationService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelper _urlHelper;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private ApplicationUser? _user;
        public AuthenticationService(ILogger<AuthenticationService> logger,
                                     UserManager<ApplicationUser> userManager,
                                     IConfiguration configuration,
                                     IHttpContextAccessor httpContextAccessor,
                                     IUrlHelper urlHelper,
                                     IEmailService emailService,
                                     IEmailTemplateService emailTemplateService)
        {
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _urlHelper = urlHelper;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
        }
        public async Task<ApiResponse<object>> RegisterUser(UserRegistrationDto userRegistrationDto)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(userRegistrationDto.Email);
                if (existingUser is not null)
                {
                    _logger.Log(LogLevel.Information, $"Existing user found when trying to register new user with email: {userRegistrationDto.Email} at {DateTime.Now.ToString("yy-MM-dd:H m s")}");
                    return ApiResponse<object>.Failure(400, "User already exists.");
                }
                var user = new ApplicationUser
                {
                    UserName = userRegistrationDto.UserName,
                    Email = userRegistrationDto.Email,
                    PhoneNumber = userRegistrationDto.PhoneNumber,
                    FirstName = userRegistrationDto.FirstName,
                    LastName = userRegistrationDto.LastName,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    RefreshToken = string.Empty,
                    PhoneCountryCode = userRegistrationDto.PhoneCountryCode,
                };
                var result = await _userManager.CreateAsync(user, userRegistrationDto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(x => new { error = x.Code, x.Description });
                    var errorString = string.Join("; ", errors);
                    _logger.Log(LogLevel.Information, $"Error occured while creating new user {userRegistrationDto.Email}: {errorString}");
                    return ApiResponse<object>.Failure(statusCode: StatusCodes.Status400BadRequest, data: errors, message: "Request unsuccessful");
                }
                _logger.Log(LogLevel.Information, $"New user created with username-> {user.UserName} and id -> {user.Id} at {user.DateCreated}");
                await _userManager.AddToRolesAsync(user, userRegistrationDto.Roles);
                var userToReturn = user.MapToUserResponseDto();

                // Generate email content and setup a background task to handle it
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = _urlHelper.Action("ConfirmEmail", "Authentication", new { email = user.Email, token }, _httpContextAccessor?.HttpContext?.Request.Scheme);
                var emailContent = _emailTemplateService.GenerateRegistrationConfirmationEmail(userRegistrationDto.UserName, confirmationLink);

                // Enqueue email sending
                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email, "Confirm your email", emailContent));
                return ApiResponse<object>.Success(200, userToReturn, "Registration Successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occured while adding a registering a new user: {ex.Message}");
                return ApiResponse<object>.Failure(500, "User could not be created");
            }
        }
        public async Task<ApiResponse<TokenDto>> ValidateUser(UserLoginDto userLoginDto)
        {
            try
            {
                _user = await _userManager.FindByNameAsync(userLoginDto.UserName);
                var result = _user != null && await _userManager.CheckPasswordAsync(_user, userLoginDto.Password);
                if (!result)
                {
                    _logger.Log(LogLevel.Warning, $"{nameof(ValidateUser)}: Authentication failed. Invalid user name or password.");
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

                // Generate email content and setup a background task to handle it
                var forgotPasswordLink = _urlHelper.Action("ResetUserPassword", "Authentication", new { email = user.Email, token }, _httpContextAccessor?.HttpContext?.Request.Scheme);
                var emailContent = _emailTemplateService.GeneratePasswordResetEmail(user.UserName, forgotPasswordLink);
                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(forgotPasswordDto.Email, "Reset your password", emailContent));

                _logger.Log(LogLevel.Information, "Password reset token generated for {userEmail} at {time}: {token}", user.Email, DateTime.Now, token);

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
                    var user = await _userManager.FindByEmailAsync(passwordResetDto.Email);
                    if (user is null)
                        return ApiResponse<object>.Failure(404, "User does not exist");
                    var resetPasswordResult = await _userManager.ResetPasswordAsync(user, passwordResetDto.Token, passwordResetDto.Password);
                    if (!resetPasswordResult.Succeeded)
                    {
                        var errorMessages = resetPasswordResult.Errors.Select(e => e.Description).ToList();
                        _logger.Log(LogLevel.Information, "Error occurred while trying to reset password for {userEmail}: {errors}", user.Email, errorMessages);
                        return ApiResponse<object>.Failure(400, null, "Request unsuccessful.", errors: errorMessages);
                    }
                    // Invalidate refresh token
                    user.RefreshToken = null;
                    user.RefreshTokenExpiryDate = null;
                    await _userManager.UpdateAsync(user);
                    _logger.Log(LogLevel.Information, "Password successfully reset at {time} for {userEmail}", DateTime.Now, user.Email);
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
                return ApiResponse<object>.Failure(400, result.Errors.Select(x => new { error = x.Code, x.Description }), "User email verification failed.");

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
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, ex.Message);
                return ApiResponse<object>.Failure(400, "User could not be deleted.");
            }
        }
        public async Task<ApiResponse<IEnumerable<UserResponseDto>>> GetUsers()
        {
            try
            {
                var users = _userManager.Users;
                var usersToReturn = users.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    UserName = u.UserName,
                    PhoneCountryCode = u.PhoneCountryCode,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                    DateCreated = u.DateCreated,
                    DateModified = u.DateModified
                }).ToList();
                _logger.Log(LogLevel.Information, $"Total number of users retrieved from the database: {users.Count()}");
                return ApiResponse<IEnumerable<UserResponseDto>>.Success(200, usersToReturn, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, ex.Message);
                return ApiResponse<IEnumerable<UserResponseDto>>.Failure(500, "Error occured while retrieving users from the database.");
            }
        }
        public async Task<ApiResponse<IEnumerable<UserResponseDto>>> GetUsersByRoleAsync(string roleName)
        {
            try
            {
                var users = await _userManager.GetUsersInRoleAsync(roleName);
                var usersToReturn = users.Select(u => u.MapToUserResponseDto()).ToList();
                _logger.Log(LogLevel.Information, $"Total number of users with the role {roleName} retrieved: {users.Count()}");
                return ApiResponse<IEnumerable<UserResponseDto>>.Success(200, usersToReturn, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, ex.Message);
                return ApiResponse<IEnumerable<UserResponseDto>>.Failure(500, "Error occured while retrieving users from the database.");
            }
        }
        public async Task<ApiResponse<object>> AddUserToRoleAsync(AddUserToRoleDto addUserToRoleDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(addUserToRoleDto.Email);
                if (user == null)
                    return ApiResponse<object>.Failure(400, "User does not exist");

                // Get the roles the user currently has
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Identify roles that need to be added
                var rolesToAdd = addUserToRoleDto.Roles.Except(currentRoles).ToList();

                if (!rolesToAdd.Any())
                    return ApiResponse<object>.Success(200, "User already has the specified role");

                // Add the new roles to the user
                var result = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!result.Succeeded)
                {
                    var errorMessage = result.Errors.Select(e => e.Description).ToList();
                    _logger.Log(LogLevel.Information, "Error occurred while adding roles to user: {errors}", errorMessage.ToList());
                    return ApiResponse<object>.Failure(400, "Request unsuccessful");
                }
                return ApiResponse<object>.Success(200, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, $"Error occured while adding roles to user: {ex.Message}");
                return ApiResponse<object>.Failure(400, "Request unsuccesful");
            }
        }
        public async Task<ApiResponse<object>> RemoveUserFromRoleAsync(RemoveUserFromRoleDto removeUserFromRoleDto)
        {
            try
            {
                _user = await _userManager.FindByEmailAsync(removeUserFromRoleDto.Email);
                if (_user == null)
                    return ApiResponse<object>.Failure(400, "User does not exist");
                var result = await _userManager.RemoveFromRolesAsync(_user, removeUserFromRoleDto.Roles);
                if (!result.Succeeded)
                {
                    _logger.Log(LogLevel.Information, $"Error occured while removing user from roles: {result.Errors.Select(x => new { error = x.Code, x.Description })}");
                    return ApiResponse<object>.Failure(400, result.Errors.Select(x => new { error = x.Code, x.Description }), "Request unsuccesful");
                }

                return ApiResponse<object>.Success(204, null);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, $"Error occured while adding roles to user: {ex.Message}");
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
        public async Task<ApiResponse<(UpdateUserDto userToPatch, ApplicationUser userEntity)>> GetUserForPatchAsync()
        {
            try
            {
                var ApplicationUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userEntity = await _userManager.FindByIdAsync(ApplicationUserId);
                if (userEntity is null)
                    return ApiResponse<(UpdateUserDto userToPatch, ApplicationUser userEntity)>.Failure(404, "User does not exist");
                var userToPatch = new UpdateUserDto
                {
                    FirstName = userEntity.FirstName,
                    LastName = userEntity.LastName,
                    PhoneCountryCode = userEntity.PhoneCountryCode,
                    PhoneNumber = userEntity.PhoneNumber,
                };
                return ApiResponse<(UpdateUserDto userToPatch, ApplicationUser userEntity)>.Success(200, (userToPatch, userEntity), "Success");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, $"Error occured while trying to update user details: {ex.Message}");
                return ApiResponse<(UpdateUserDto userToPatch, ApplicationUser userEntity)>.Failure(400, "Request unsuccesful");
            }
        }

        public async Task<ApiResponse<object>> SaveChangesForPatch(UpdateUserDto userToPatch, ApplicationUser userEntity)
        {
            userEntity.FirstName = userToPatch.FirstName;
            userEntity.LastName = userToPatch.LastName;
            userEntity.PhoneCountryCode = userToPatch.PhoneCountryCode;
            userEntity.PhoneNumber = userToPatch.PhoneNumber;
            userEntity.DateModified = DateTime.UtcNow;

            IdentityResult result = await _userManager.UpdateAsync(userEntity);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => new { error = x.Code, x.Description });
                var errorString = string.Join("; ", errors);
                _logger.Log(LogLevel.Information, $"Error occured while updating user details {userEntity.Email}: {errorString}");
                return ApiResponse<object>.Failure(statusCode: StatusCodes.Status400BadRequest, data: errors, message: "Request unsuccessful");
            }
            return ApiResponse<object>.Success(204, null);
        }
        public async Task<ApiResponse<object>> ChangeUserPasswordAsync(ChangePasswordDto changePasswordDto)
        {
            try
            {
                var ApplicationUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _user = await _userManager.FindByIdAsync(ApplicationUserId);
                if (_user is null)
                    return ApiResponse<object>.Failure(404, "User does not exist");
                var result = await _userManager.ChangePasswordAsync(_user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(x => new { error = x.Code, x.Description });
                    var errorString = string.Join("; ", errors);
                    _logger.Log(LogLevel.Information, $"Error occured while changing user password {_user.Email}: {errorString}");
                    return ApiResponse<object>.Failure(statusCode: StatusCodes.Status400BadRequest, data: errors, message: "Request unsuccessful");
                }
                return ApiResponse<object>.Success(204, null);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, $"Error occured while changing user password: {ex.Message}");
                return ApiResponse<object>.Failure(400, "Request unsuccesful");

            }
        }
        public async Task<ApiResponse<object>> ChangeUserEmailAsync(ChangeEmailDto changeEmailDto)
        {
            try
            {
                var ApplicationUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _user = await _userManager.FindByIdAsync(ApplicationUserId);
                if (_user is null)
                    return ApiResponse<object>.Failure(404, "User does not exist");
                if (_user.Email == changeEmailDto.NewEmail)
                    return ApiResponse<object>.Failure(400, "New email is the same as the current email");
                var token = await _userManager.GenerateChangeEmailTokenAsync(_user, changeEmailDto.NewEmail);
                _logger.Log(LogLevel.Information, $"New email confirmation token for {_user.Email}: {token}");

                var emailChangeConfirmationLink = _urlHelper.Action("ConfirmUserEmailChange", "Authentication", new { oldEmail = _user.Email, newEmail= changeEmailDto.NewEmail, token }, _httpContextAccessor?.HttpContext?.Request.Scheme);
                var emailContent = _emailTemplateService.GenerateEmailChangeConfirmationLink(_user.UserName, emailChangeConfirmationLink);
                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(changeEmailDto.NewEmail, "Confirm your new email", emailContent));

                return ApiResponse<object>.Success(200, token, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, $"Error occured while changing user password: {ex.Message}");
                return ApiResponse<object>.Failure(400, "Request unsuccesful");

            }
        }
        public async Task<ApiResponse<object>> NewUserEmailConfirmation(string token, string oldEmail, string newEmail)
        {
            try
            {
                _user = await _userManager.FindByIdAsync(oldEmail);
                if (_user is null)
                {
                    return ApiResponse<object>.Failure(404, null, "User does not exist");
                }
                var result = await _userManager.ChangeEmailAsync(_user, newEmail, token);
                if (result.Succeeded)
                {
                    _logger.Log(LogLevel.Information, $"Email confirmation successful for {_user.UserName} at {DateTime.Now}");
                    return ApiResponse<object>.Success(200, "New User email verification successful");
                }
                return ApiResponse<object>.Failure(400, result.Errors.Select(x => new { error = x.Code, x.Description }), "User email verification failed.");

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occured while trying to confirm new user email: {ex.Message}");
                return ApiResponse<object>.Failure(500, "Request unsuccessful");
            }
        }
        public async Task<ApiResponse<object>> UpdateUserProfileDto(UpdateUserDto updateUserDto)
        {
            try
            {
                var ApplicationUserId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userEntity = await _userManager.FindByIdAsync(ApplicationUserId);
                if (userEntity is null)
                    return ApiResponse<object>.Failure(404, "User does not exist");

                if (userEntity.FirstName != null)
                    userEntity.FirstName = updateUserDto.FirstName;
                if (userEntity.LastName != null)
                    userEntity.LastName = updateUserDto.LastName;
                if (userEntity.PhoneCountryCode != null)
                    userEntity.PhoneCountryCode = updateUserDto.PhoneCountryCode;
                if (userEntity.PhoneNumber != null)
                    userEntity.PhoneNumber = updateUserDto.PhoneNumber;
                userEntity.DateModified = DateTime.UtcNow;

                IdentityResult result = await _userManager.UpdateAsync(userEntity);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(x => new { error = x.Code, x.Description });
                    var errorString = string.Join("; ", errors);
                    _logger.Log(LogLevel.Information, $"Error occured while updating user details {userEntity.Email}: {errorString}");
                    return ApiResponse<object>.Failure(statusCode: StatusCodes.Status400BadRequest, data: errors, message: "Request unsuccessful");
                }
                return ApiResponse<object>.Success(204, null);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, $"Error occured while trying to update user details: {ex.Message}");
                return ApiResponse<object>.Failure(400, "Request unsuccesful");
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
            var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtConfiguration>();
            var key = Encoding.UTF8.GetBytes(jwtSettings.secretKey);
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
            var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtConfiguration>();
            var tokenOptions = new JwtSecurityToken
            (
            issuer: jwtSettings.validIssuer,
            audience: jwtSettings.validAudience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.expires)),
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
            var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtConfiguration>();
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.secretKey)),
                ValidateLifetime = true,
                ValidIssuer = jwtSettings.validIssuer,
                ValidAudience = jwtSettings.validAudience,
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
