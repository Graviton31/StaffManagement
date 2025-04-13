using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StaffManagementApi.Data;
using StaffManagementApi.Models;
using StaffManagementApi.ModelsDto;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkerController : ControllerBase
    {
        private readonly ContextStaffManagement _context;
        private readonly IConfiguration _configuration;

        public WorkerController(ContextStaffManagement context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] WorkerRegistrationDto registrationDto)
        {
            var existingWorker = await _context.Workers
                .FirstOrDefaultAsync(w =>
                    w.Name == registrationDto.Name &&
                    w.Surname == registrationDto.Surname &&
                    w.Patronymic == registrationDto.Patronymic &&
                    w.BirthDate == registrationDto.BirthDate);

            if (existingWorker != null)
            {
                return BadRequest("Работник с такими ФИО и датой рождения уже существует.");
            }

            var credentialHasher = new CredentialHasher();
            credentialHasher.SetCredentials(registrationDto.WorkEmail, registrationDto.Password);

            byte[] hashedPassword = credentialHasher.HashCredentials();

            var newWorker = new Worker
            {
                WorkEmail = registrationDto.WorkEmail,
                Password = hashedPassword,
                Name = registrationDto.Name,
                Surname = registrationDto.Surname,
                Patronymic = registrationDto.Patronymic,
                BirthDate = registrationDto.BirthDate,
                Phone = registrationDto.Phone,
                IdRole = registrationDto.IdRole
            };

            _context.Workers.Add(newWorker);
            await _context.SaveChangesAsync();

            return Ok("Работник успешно зарегистрирован.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] WorkerLoginDto loginDto)
        {
            try
            {
                var worker = await _context.Workers
                    .Include(w => w.IdRoleNavigation)
                    .FirstOrDefaultAsync(w => w.WorkEmail == loginDto.WorkEmail);

                if (worker == null)
                {
                    return Unauthorized("Неверный email или пароль.");
                }

                var credentialHasher = new CredentialHasher();
                if (!credentialHasher.VerifyPassword(loginDto.Password, worker.Password, loginDto.WorkEmail))
                {
                    return Unauthorized("Неверный email или пароль.");
                }

                var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);
                var tokenHandler = new JwtSecurityTokenHandler();

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, worker.WorkEmail),
                        new Claim(ClaimTypes.NameIdentifier, worker.IdWorker.ToString()),
                        new Claim(ClaimTypes.Role, worker.IdRoleNavigation?.Name ?? "User")
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var refreshToken = Guid.NewGuid().ToString();

                worker.Token = refreshToken;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Token = tokenHandler.WriteToken(token),
                    RefreshToken = refreshToken
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var worker = await _context.Workers
                .Include(w => w.IdRoleNavigation)
                .FirstOrDefaultAsync(w => w.Token == refreshTokenDto.RefreshToken);

            if (worker == null)
            {
                return Unauthorized("Неверный рефреш-токен.");
            }

            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, worker.WorkEmail),
                    new Claim(ClaimTypes.NameIdentifier, worker.IdWorker.ToString()),
                    new Claim(ClaimTypes.Role, worker.IdRoleNavigation?.Name ?? "User")
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var newRefreshToken = Guid.NewGuid().ToString();

            worker.Token = newRefreshToken;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Token = tokenHandler.WriteToken(token),
                RefreshToken = newRefreshToken
            });
        }
    }
}