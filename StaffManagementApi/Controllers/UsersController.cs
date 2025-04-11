//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using StaffManagement.ModelsDto;
//using StaffManagementApi.Data;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//namespace backend.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class UserController : ControllerBase
//    {
//        private readonly ContextStaffManagement _context; // Контекст базы данных
//        private readonly IConfiguration _configuration; // Конфигурация для доступа к настройкам

//        public UserController(ContextStaffManagement context, IConfiguration configuration)
//        {
//            _context = context;
//            _configuration = configuration;
//        }

//        /// <summary>
//        /// Регистрация нового пользователя.
//        /// </summary>
//        /// <param name="registrationDto">DTO с данными для регистрации пользователя.</param>
//        /// <returns>Результат выполнения операции.</returns>
//        [Authorize(Roles = "Admin")]
//        [HttpPost("register")]
//        public async Task<IActionResult> Register([FromBody] UserRegistrationDto registrationDto)
//        {
//            // Проверяем, что логин не занят
//            if (await _context.Users.AnyAsync(u => u.Login == registrationDto.Login))
//            {
//                return BadRequest("Логин уже занят.");
//            }

//            // Создаем экземпляр CredentialHasher
//            CredentialHasher credentialHasher = new CredentialHasher();
//            credentialHasher.SetCredentials(registrationDto.Login, registrationDto.Password);
//            Console.WriteLine(credentialHasher); // Логирование хешера для отладки

//            // Хешируем пароль
//            byte[] hashedPassword = credentialHasher.HashCredentials();

//            // Создаем нового пользователя
//            User newUser = new User
//            {
//                Login = registrationDto.Login,
//                Password = hashedPassword, // Преобразуем массив байт в строку для хранения
//                Name = registrationDto.Name,
//                Surname = registrationDto.Surname,
//                Patronymic = registrationDto.Patronymic,
//                IsAdmin = registrationDto.IsAdmin ?? false // По умолчанию, можно изменить по необходимости
//            };

//            // Добавляем пользователя в контекст и сохраняем изменения
//            _context.Users.Add(newUser);
//            await _context.SaveChangesAsync();

//            return Ok("Пользователь успешно зарегистрирован.");
//        }

//        /// <summary>
//        /// Авторизация пользователя.
//        /// </summary>
//        /// <param name="loginDto">DTO с данными для авторизации пользователя.</param>
//        /// <returns>Результат выполнения операции.</returns>
//        [HttpPost("login")]
//        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
//        {
//            try
//            {
//                var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == loginDto.Login);
//                if (user == null)
//                {
//                    return Unauthorized("Неверный логин или пароль.");
//                }

//                CredentialHasher credentialHasher = new CredentialHasher();
//                if (!credentialHasher.VerifyPassword(loginDto.Password, user.Password, loginDto.Login))
//                {
//                    return Unauthorized("Неверный логин или пароль.");
//                }

//                var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);
//                var tokenHandler = new JwtSecurityTokenHandler();

//                var tokenDescriptor = new SecurityTokenDescriptor
//                {
//                    Subject = new ClaimsIdentity(new Claim[]
//                    {
//                new Claim(ClaimTypes.Name, user.Login),
//                new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
//                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
//                    }),
//                    Expires = DateTime.UtcNow.AddHours(1),
//                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//                };

//                var token = tokenHandler.CreateToken(tokenDescriptor);

//                // Создание рефреш-токена
//                var refreshToken = Guid.NewGuid().ToString();
//                user.RefreshToken = refreshToken; // Сохраняем рефреш-токен в базе данных
//                await _context.SaveChangesAsync(); // Сохраняем изменения

//                return Ok(new { Token = tokenHandler.WriteToken(token), RefreshToken = refreshToken });
//            }
//            catch (Exception ex)
//            {
//                // Логирование ошибки
//                return StatusCode(500, "Внутренняя ошибка сервера.");
//            }
//        }

//        /// <summary>
//        /// Обновление токена доступа.
//        /// </summary>
//        /// <param name="refreshTokenDto">DTO с рефреш-токеном.</param>
//        /// <returns>Результат выполнения операции.</returns>
//        [HttpPost("refresh-token")]
//        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
//        {
//            // Поиск пользователя по рефреш-токену
//            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshTokenDto.RefreshToken);
//            if (user == null)
//            {
//                return Unauthorized("Неверный рефреш-токен.");
//            }

//            // Генерация нового JWT токена
//            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);
//            var tokenHandler = new JwtSecurityTokenHandler();

//            var tokenDescriptor = new SecurityTokenDescriptor
//            {
//                Subject = new ClaimsIdentity(new Claim[]
//                {
//                    new Claim(ClaimTypes.Name, user.Login),
//                    new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()), // ID пользователя
//                    new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User") // Роль пользователя
//                }),
//                Expires = DateTime.UtcNow.AddHours(1), // Установка времени жизни токена
//                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//            };

//            var token = tokenHandler.CreateToken(tokenDescriptor);

//            // Создание нового рефреш-токена
//            var newRefreshToken = Guid.NewGuid().ToString();
//            user.RefreshToken = newRefreshToken; // Обновляем рефреш-токен
//            await _context.SaveChangesAsync(); // Сохраняем изменения

//            return Ok(new { Token = tokenHandler.WriteToken(token), RefreshToken = newRefreshToken });
//        }
//    }
//}