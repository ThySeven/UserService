using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Repositories;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        public readonly IUserRepository _userRepository;
        public readonly ILogger<IUserRepository> _logger;
        public readonly IConfiguration _config;

        public UserController(ILogger<IUserRepository> logger, IConfiguration config, IUserRepository userRepository) 
        {
            _config = config;
            _logger = logger;
            _userRepository = userRepository;

            var hostName = System.Net.Dns.GetHostName();
            var ips = System.Net.Dns.GetHostAddresses(hostName);
            var _ipaddr = ips.First().MapToIPv4().ToString();
            _logger.LogDebug(1, $"XYZ Service responding from {_ipaddr}");
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetUser(string id)
        {
            try
            {
                string token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

                if (TokenHandler.DecodeToken(token).Username == _userRepository.GetById(id).Username
                    || Request.Headers["X-Internal-ApiKey"] == WebManager.GetInstance.HttpClient.DefaultRequestHeaders.First(x => x.Key == "X-Internal-ApiKey").Value)
                {
                    return Ok(_userRepository.GetById(id));
                }
                
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Failed to find user with id: {id}: {ex}");
                return BadRequest($"Failed to find user with id: {id}: {ex}");
            }
        }

        [Authorize]
        [HttpPut("update")]
        public IActionResult UpdateUser(UserModelDTO user)
        {
            try
            {
                string token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

                if (TokenHandler.DecodeToken(token).Username == _userRepository.GetById(user.Id).Username)
                {
                    _userRepository.UpdateUser(user);
                    string newToken = TokenHandler.GenerateNewJwtToken(user);
                    _logger.LogInformation($"Information updated for user: {user.Username}");
                    return Ok($"Information updated for user: {user.Username} \n\n {new { token = newToken }}");
                }
                
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Failed to find user with id: {user.Id}: {ex}");
                return BadRequest($"Failed to find user with id: {user.Id}: {ex}");
            }
        }
        
        [Authorize]
        [HttpPut("updatepassword")]
        public IActionResult UpdatePassword(PasswordUpdateRecord passwordUpdateRecord)
        {
            try
            {
                string token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");

                if (passwordUpdateRecord.LoginModel.Username == TokenHandler.DecodeToken(token).Username)
                {
                    _userRepository.UpdatePassword(passwordUpdateRecord.LoginModel, passwordUpdateRecord.newPassword);
                    _logger.LogInformation($"Password updated for user: {passwordUpdateRecord.LoginModel.Username}");
                    return Ok($"Password updated for user: {passwordUpdateRecord.LoginModel.Username}");
                }
                
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Failed to update password for user with username: {passwordUpdateRecord.LoginModel.Username}: {ex}");
                return BadRequest($"Failed to update password for user with username: {passwordUpdateRecord.LoginModel.Username}: {ex}");
            }
        }
        public record PasswordUpdateRecord(LoginModel LoginModel, string newPassword);

        [AllowAnonymous]
        [HttpPost("create")]
        public IActionResult CreateUser(UserModel user)
        {
            try
            {
                _userRepository.CreateUser(user);
                _logger.LogInformation($"User created: {user.Username}");
                return Ok($"User created: {user.Username}");
            }

            catch (Exception ex) 
            {
                _logger.LogCritical($"Failed to create user: {user.Username}: {ex}");
                return BadRequest($"Failed to create user: {user.Username}: {ex}");
            }
        }
        
        [Authorize]
        [HttpDelete("delete/{id}")]
        public IActionResult DeleteUser(string id)
        {
            try
            {
                string token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                
                if (TokenHandler.DecodeToken(token).Username == _userRepository.GetById(id).Username)
                {
                    _userRepository.DeleteUser(id);
                    return Ok($"User deleted with id: {id}");
                }
                
                return Unauthorized();
            }
            catch(Exception ex)
            {
                _logger.LogCritical($"Failed to delete user with id: {id}: {ex}");
                return BadRequest($"Failed to delete user with id: {id}: {ex}");
            }
        }
        
        [AllowAnonymous]
        [HttpGet("login")]
        public IActionResult Login(LoginModel credentials)
        {
            try
            {
                string token = TokenHandler.GenerateJwtToken(JsonSerializer.Serialize(_userRepository.Login(credentials)));
                return Ok($"{new { token }}");
            }
            catch(Exception ex) 
            {
                _logger.LogCritical($"Failed to validate credentials: {ex}");
                return BadRequest($"Failed to validate credentials: {ex}");
            }
        }
        
        [AllowAnonymous]
        [HttpGet("verify/{id}")]
        public IActionResult VerifyUser(string id)
        {
            try
            {
                _userRepository.VerifyUser(id);
                _logger.LogInformation($"User with id: {id}, has been verified");
                return Ok("Skal redirect til login side");
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Failed to validate user with id: {id}: {ex}");
                return BadRequest($"Failed to validate user with id: {id}: {ex}");
            }
        }
    }
}
