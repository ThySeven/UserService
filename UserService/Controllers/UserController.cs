using Microsoft.AspNetCore.Mvc;
using UserService.Repositories;
using UserService.Models;
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

        [HttpGet("{id}")]
        public IActionResult GetUser(string id)
        {
            try
            {
                return Ok(_userRepository.GetById(id));
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Failed to find user with id: {id}: {ex}");
                return BadRequest("Bad request");
            }
        }

        [HttpPut("update")]
        public IActionResult UpdateUser(UserModelDTO user)
        {
            try
            {
                _userRepository.UpdateUser(user);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Failed to find user with id: {user.id}: {ex}");
                return BadRequest("Bad request");
            }
        }
        
        [HttpPut("updatepassword")]
        public IActionResult UpdatePassword(PasswordUpdataRecord passwordUpdataRecord)
        {
            try
            {
                _userRepository.UpdatePassword(passwordUpdataRecord.LoginModel, passwordUpdataRecord.newPassword);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Failed to find user with username: {passwordUpdataRecord.LoginModel.Username}: {ex}");
                return BadRequest("Bad request");
            }
        }
        public record PasswordUpdataRecord(LoginModel LoginModel, string newPassword);

        [HttpPost("create")]
        public IActionResult CreateUser(UserModel user)
        {
            try
            {
                _userRepository.CreateUser(user);
                _logger.LogInformation($"user created: {user}");
                return Ok();
            }

            catch (Exception ex) 
            {
                _logger.LogCritical($"Failed to create user {user}: {ex}");
                return BadRequest("Bad request");
            }
        }
        [HttpDelete("delete/{id}")]
        public IActionResult DeleteUser(string id)
        {
            try
            {
                _userRepository.DeleteUser(id);
                return Ok();
            }
            catch(Exception ex)
            {
                _logger.LogCritical($"Failed to delete user {id}: {ex}");
                return BadRequest("Bad request");
            }
        }
        [HttpGet("login")]
        public IActionResult Login(LoginModel credentials)
        {
            try
            {
                return Ok(_userRepository.Login(credentials));
            }
            catch(Exception ex) 
            {
                _logger.LogCritical($"Failed to validate credentials: {ex}");
                return BadRequest("Bad request");
            }   
        }
        [HttpGet("verify/{id}")]
        public IActionResult VerifyUser(string id)
        {
            try
            {
                _userRepository.VerifyUser(id);
                return Ok("Skal redirect til login side");
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Failed to validate user with id: {id}: {ex}");
                return BadRequest("Bad request");
            }
        }
    }
}
