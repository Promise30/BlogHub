using BloggingAPI.Contracts.Dtos.Requests.Auth;
using BloggingAPI.Contracts.Dtos.Responses;
using BloggingAPI.Contracts.Dtos.Responses.Auth;
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
        /// <summary>
        /// Registers a new user with the provided registration details.
        /// </summary>
        /// <param name="userRegistrationDto">The DTO containing the user registration information, including username, email, password, and other details.</param>
        /// <returns>A response indicating the success or failure of the registration process, including any validation errors or user information.</returns>
        [AllowAnonymous]
        [HttpPost("register")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Register(UserRegistrationDto userRegistrationDto)
        {

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var result = await _authenticationService.RegisterUser(userRegistrationDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Authenticates a user and provides a JWT access and refresh token if the credentials are valid.
        /// </summary>
        /// <param name="userLoginDto">The DTO containing the username and password for authentication.</param>
        /// <returns>A response with a JWT access token if authentication is successful, or an error message if the credentials are invalid.</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authenticationService.ValidateUser(userLoginDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Confirms the user's email address using a token sent via email.
        /// </summary>
        /// <param name="token">The email confirmation token.</param>
        /// <param name="email">The user's email address.</param>
        /// <returns>Returns a status indicating the success or failure of the email confirmation process.</returns>
        [AllowAnonymous]
        [HttpGet("confirm-email")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var result = await _authenticationService.UserEmailConfirmation(token, email);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Initiates the password reset process by sending the reset link to the user's email address
        /// </summary>
        /// <param name="forgotPasswordDto">The email address associated with the user account</param>
        /// <returns>A password reset link</returns>
        //[Authorize]
        [HttpPost("forgot-password")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authenticationService.ForgotPasswordRequestAsync(forgotPasswordDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Verifies the user's email and password reset token, preparing the user for a password reset.
        /// </summary>
        /// <param name="email">The user's email address associated with the reset request.</param>
        /// <param name="token">The token for validating the password reset request.</param>
        /// <returns>An object containing the email and token if valid.</returns>
        [AllowAnonymous]
        [HttpGet("reset-password")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public IActionResult ResetUserPassword(string email, string token)
        {
            // Optionally, render a view for password reset or redirect to a frontend URL.
            return Ok(new { email, token });
        }
        /// <summary>
        /// Resets the user's password using the provided new password and token.
        /// </summary>
        /// <param name="passwordResetDto">The DTO containing the user's email, new password and token for validation.</param>
        /// <returns>The result of the password reset operation, including success or failure information.</returns>
        //[Authorize]
        [HttpPost("reset-password")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword(PasswordResetDto passwordResetDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authenticationService.PasswordResetAsync(passwordResetDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Refreshes the access token using the provided refresh token, if valid.
        /// </summary>
        /// <param name="tokenDto">The DTO containing the access token and refresh token for validation.</param>
        /// <returns>The new access token if the refresh is successful, or an error message if not.</returns>
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken(GetNewTokenDto tokenDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authenticationService.RefreshToken(tokenDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Deletes a user specified by their email address. Only accessible to users with the "Administrator" role.
        /// </summary>
        /// <param name="userEmail">The email address of the user to be deleted.</param>
        /// <returns>A response indicating the result of the delete operation, including success or failure details.</returns>s
        //[Authorize(Roles ="Administrator")]
        [HttpDelete("delete-user")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(string userEmail)
        {
            var result = await _authenticationService.DeleteUser(userEmail);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Returns details of all registered users. Only accessible to users with the 'Administrator' role.
        /// </summary>
        /// <returns>A response containing the list of users if successful, or an error message if there was an issue retrieving users.</returns>
        //[Authorize(Roles ="Administrator")]
        [HttpGet("users")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRegisteredUsers()
        {
            var result = await _authenticationService.GetUsers();
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Returns details of all registered users with the specified role. 
        /// Only accessible to users with the 'Administrator' role.
        /// </summary>
        /// <param name="roleName">The name of the role to filter users by</param>
        /// <returns>A response containing the list of users if successful, or an error message if there was an error</returns>
        // [Authorize(Roles = "Administrator")]
        [HttpGet("users/roles")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRegisteredUsersByRole(string roleName)
        {
            var result = await _authenticationService.GetUsersByRoleAsync(roleName);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Adds specified role(s) to a registered user. Only accessible to users with the 'Administrator' role.
        /// </summary>
        /// <param name="addUserToRoleDto">The data transfer object containing user email and roles to add.</param>
        /// <returns>A response indicating the success or failure of the operation.</returns>
        //[Authorize(Roles ="Administrator")]
        [HttpPost("addRolesToUsers")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddRolesToUsers(AddUserToRoleDto addUserToRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authenticationService.AddUserToRoleAsync(addUserToRoleDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Removes specified role from a registered user. Only accessible to users with the 'Administrator' role.
        /// </summary>
        /// <param name="removeUserFromRoleDto">The data transfer object containing user email and roles to remove.</param>
        /// <returns>A response indicating the success or failure of the operation.</returns>
        //[Authorize(Roles ="Administrator")]
        [HttpPost("removeUserFromRoles")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveUserFromRoles(RemoveUserFromRoleDto removeUserFromRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _authenticationService.RemoveUserFromRoleAsync(removeUserFromRoleDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Retrieves the role(s) assigned to a user based on the provided email address. Only accessible to users with the 'Administrator' role.
        /// </summary>
        /// <param name="email">The email address of the user whose roles are to be retrieved.</param>
        /// <returns>A response containing the list of roles if successful, or an error message if the user does not exist or another issue occurs.</returns>
        //[Authorize(Roles ="Administrator")]
        [HttpGet("userRoles")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserRoles(string email)
        {
            var result = await _authenticationService.GetUserRolesAsync(email);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Updates the details of an authenticated user
        /// </summary>
        /// <param name="updateUserDto">The details of the user's profile that are to be updated</param>
        /// <returns>A response indicating the success of the update operation including success or failure information</returns>
        //[Authorize]
        [HttpPatch("update-user")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            { return BadRequest(ModelState); }
            var result = await _authenticationService.UpdateUserProfileDto(updateUserDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Updates the details of an authenticated user
        /// </summary>
        /// <param name="patchDoc">The details of the user's profile that are to be updated</param>
        /// <returns>A response indicating the success of the update operation including success or failure information</returns>
        //[Authorize]
        [HttpPatch("update-user-profile")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
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
        /// <summary>
        /// Changes the password of the currently authenticated user.
        /// </summary>
        /// <param name="changePasswordDto">The data transfer object containing current and new passwords.</param>
        /// <returns>Returns a status indicating success or failure of the password change operation.</returns>
        //[Authorize]
        [HttpPost("change-password")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var result = await _authenticationService.ChangeUserPasswordAsync(changePasswordDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Changes the email of the currently authenticated user.
        /// </summary>
        /// <param name="changeEmailDto">The data transfer object containing current and new email.</param>
        /// <returns>Returns a status indicating success or failure of the email change operation.</returns>
        //[Authorize]
        [HttpPost("change-email")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangeEmail(ChangeEmailDto changeEmailDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _authenticationService.ChangeUserEmailAsync(changeEmailDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Confirms the user's new email address using a token sent via email.
        /// </summary>
        /// <param name="token">The email confirmation token.</param>
        /// <param name="oldEmail">The user's old email address.</param>
        /// <param name="newEmail">The user's new email address.</param>
        /// <returns>Returns a status indicating the success or failure of the email confirmation process.</returns>
        [AllowAnonymous]
        [HttpGet("confirm-email-change")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmUserEmailChange(string token, string oldEmail, string newEmail)
        {
            var result = await _authenticationService.NewUserEmailConfirmation(token, oldEmail, newEmail);
            return StatusCode(result.StatusCode, result);
        }
    }
}
