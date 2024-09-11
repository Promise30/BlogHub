using BloggingAPI.Contracts.Dtos.Requests.Auth;
using BloggingAPI.Contracts.Dtos.Responses;
using BloggingAPI.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BloggingAPI.Presentation.Controllers
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
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationDto userRegistrationDto)
        {

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var result = await _authenticationService.RegisterUser(userRegistrationDto);
            return StatusCode(result.StatusCode, result);
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authenticationService.ValidateUser(userLoginDto);
            return StatusCode(result.StatusCode, result);
        }
        [AllowAnonymous]
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var result = await _authenticationService.UserEmailConfirmation(token, email);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authenticationService.ForgotPasswordRequestAsync(forgotPasswordDto);
            return StatusCode(result.StatusCode, result);
        }
        [AllowAnonymous]
        [HttpGet("reset-password")]
        public IActionResult ResetUserPassword(string email, string token)
        {
            // Optionally, render a view for password reset or redirect to a frontend URL.
            return Ok(new { email, token });
        }
        [Authorize]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(PasswordResetDto passwordResetDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authenticationService.PasswordResetAsync(passwordResetDto);
            return StatusCode(result.StatusCode, result);
        }
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(GetNewTokenDto tokenDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authenticationService.RefreshToken(tokenDto);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "Administrator")]
        [HttpDelete("delete-user")]
        public async Task<IActionResult> DeleteUser(string userEmail)
        {
            var result = await _authenticationService.DeleteUser(userEmail);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles ="Administrator")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllRegisteredUsers()
        {
            var result = await _authenticationService.GetUsers();
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "Administrator")]
        [HttpGet("users/roles")]
        public async Task<IActionResult> GetAllRegisteredUsersByRole(string roleName)
        {
            var result = await _authenticationService.GetUsersByRoleAsync(roleName);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "Administrator")]
        [HttpPost("addRolesToUsers")]
        public async Task<IActionResult> AddRolesToUsers(AddUserToRoleDto addUserToRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authenticationService.AddUserToRoleAsync(addUserToRoleDto);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "Administrator")]
        [HttpPost("removeUserFromRoles")]
        public async Task<IActionResult> RemoveUserFromRoles(RemoveUserFromRoleDto removeUserFromRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authenticationService.RemoveUserFromRoleAsync(removeUserFromRoleDto);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "Administrator")]
        [HttpGet("userRoles")]
        public async Task<IActionResult> GetUserRoles(string email)
        {
            var result = await _authenticationService.GetUserRolesAsync(email);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize]
        [HttpPatch("updateUser")]
        public async Task<IActionResult> UpdateUser(UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            { return BadRequest(ModelState); }
            var result = await _authenticationService.UpdateUserProfileDto(updateUserDto);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize]
        [HttpPatch("update-user")]
        public async Task<IActionResult> PartiallyUpdateUser( [FromBody] JsonPatchDocument<UpdateUserDto> patchDoc)
        {
            if (patchDoc is null)
                return BadRequest("Invalid payload. PatchDoc cannot be null");
            var result = await _authenticationService.GetUserForPatchAsync();
            patchDoc.ApplyTo(result.Data.userToPatch);
            if (!TryValidateModel(result.Data.userToPatch))
            {
                return ValidationProblem(ModelState);
            }
            var response = await _authenticationService.SaveChangesForPatch(result.Data.userToPatch, result.Data.userEntity);
            return StatusCode(response.StatusCode, response);
        }
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var result = await _authenticationService.ChangeUserPasswordAsync(changePasswordDto);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize]
        [HttpPost("change-email")]
        public async Task<IActionResult> ChangeEmail(ChangeEmailDto changeEmailDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _authenticationService.ChangeUserEmailAsync(changeEmailDto);
            return StatusCode(result.StatusCode, result);
        }
        [AllowAnonymous]
        [HttpGet("confirm-email-change")]
        public async Task<IActionResult> ConfirmUserEmailChange(string token, string oldEmail, string newEmail)
        {
            var result = await _authenticationService.NewUserEmailConfirmation(token, oldEmail, newEmail);
            return StatusCode(result.StatusCode, result);
        }
    }
}
