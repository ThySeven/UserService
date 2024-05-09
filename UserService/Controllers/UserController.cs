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
        [HttpGet("validate")]
        public IActionResult Login(string userName,string password)
        {
            try
            {
                _userRepository.ValidateUser(userName,password);
                return Ok();
            }
            catch(Exception ex) 
            {
                _logger.LogCritical($"Failed to validate credentials: {ex}");
                return BadRequest("Bad request");
            }   
        }
        [HttpPut("verify/{id}")]
        public IActionResult VerifyUser(string id)
        {
            try
            {
                _userRepository.VerifyUser(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Failed to validate user with id: {id}: {ex}");
                return BadRequest("Bad request");
            }
        }
    }
}
