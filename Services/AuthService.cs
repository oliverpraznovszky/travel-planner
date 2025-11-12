using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using travel_planner.Data;
using travel_planner.DTOs;
using travel_planner.Models;

namespace travel_planner.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            // Ellenőrizzük, létezik-e már a felhasználónév vagy email
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return null; // Username már foglalt
            }

            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return null; // Email már foglalt
            }

            // Jelszó hashelése BCrypt-tel
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Új user létrehozása
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // JWT token generálása
            string token = GenerateJwtToken(user.Id, user.Username, user.Email);

            var expiryHours = _configuration.GetValue<int>("JwtSettings:ExpiryInHours");

            return new AuthResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(expiryHours)
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // User keresése email vagy username alapján
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.EmailOrUsername ||
                                         u.Username == loginDto.EmailOrUsername);

            if (user == null)
            {
                return null; // User nem található
            }

            // Jelszó ellenőrzése
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return null; // Hibás jelszó
            }

            // JWT token generálása
            string token = GenerateJwtToken(user.Id, user.Username, user.Email);

            var expiryHours = _configuration.GetValue<int>("JwtSettings:ExpiryInHours");

            return new AuthResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(expiryHours)
            };
        }

        public string GenerateJwtToken(int userId, string username, string email)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryHours = jwtSettings.GetValue<int>("ExpiryInHours");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiryHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}