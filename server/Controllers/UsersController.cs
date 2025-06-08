using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using server.Data;
using server.Dtos;
using server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace server.Controllers
{
    /// <summary>
    /// Manages user registration, authentication, profile and role management.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtSettings _jwtSettings;

        /// <summary>
        /// Initializes a new instance of <see cref="UsersController"/>.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="jwtOptions">JWT settings (issuer, audience, key, expiration).</param>
        public UsersController(AppDbContext context, IOptions<JwtSettings> jwtOptions)
        {
            _context = context;
            _jwtSettings = jwtOptions.Value;
        }

        /// <summary>
        /// Registers a new user with role "user".
        /// </summary>
        /// <param name="userDto">Contains Username, Email and Password.</param>
        /// <returns>Ok with creation message.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto userDto)
        {
            var user = new User
            {
                Username     = userDto.Username,
                Email        = userDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                Role         = "user"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User created" });
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="userDto">Contains Username and Password.</param>
        /// <returns>Ok with token, or Unauthorized on failure.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto userDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(userDto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid credentials" });

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("role", user.Role ?? "user")
            };

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer:             _jwtSettings.Issuer,
                audience:           _jwtSettings.Audience,
                claims:             claims,
                expires:            DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
                signingCredentials: creds
            );

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        /// <summary>
        /// Returns information about the currently authenticated user.
        /// </summary>
        /// <returns>User Id, Username and Email, or 401/404.</returns>
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var idClaim = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _));
            if (idClaim == null)
                return Unauthorized(new { message = "Token invalide ou manquant" });

            if (!int.TryParse(idClaim.Value, out int userId))
                return Unauthorized(new
                {
                    message = "Token invalide (userId non numérique)",
                    claims  = User.Claims.Select(c => new { c.Type, c.Value })
                });

            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new { u.Id, u.Username, u.Email })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "Utilisateur introuvable" });

            return Ok(user);
        }

        /// <summary>
        /// Public endpoint accessible without authentication.
        /// </summary>
        /// <returns>Public message.</returns>
        [HttpGet("public")]
        public IActionResult GetPublicData()
        {
            return Ok(new { message = "Donnée publique accessible à tous" });
        }

        /// <summary>
        /// Returns the role claim of the authenticated user.
        /// </summary>
        /// <returns>User role.</returns>
        [Authorize]
        [HttpGet("role")]
        public IActionResult GetRole()
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            return Ok(new { role });
        }

        /// <summary>
        /// Admin-only endpoint to verify admin access.
        /// </summary>
        /// <returns>String confirming admin status.</returns>
        [Authorize(Roles = "admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnlyRoute()
        {
            return Ok("Tu es admin !");
        }

        /// <summary>
        /// Updates a user's role (admin only).
        /// </summary>
        /// <param name="id">User identifier.</param>
        /// <param name="newRole">New role to assign.</param>
        /// <returns>Ok with update message or 404.</returns>
        [Authorize(Roles = "admin")]
        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] string newRole)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Utilisateur introuvable" });

            user.Role = newRole;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Rôle de {user.Username} mis à jour en {newRole}" });
        }

        /// <summary>
        /// Returns a list of all users (admin only).
        /// </summary>
        /// <returns>List of <see cref="UserDto"/>.</returns>
        [Authorize(Roles = "admin")]
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto { Username = u.Username, Email = u.Email, Role = u.Role })
                .ToListAsync();
            return Ok(users);
        }

        /// <summary>
        /// Retrieves the authenticated user's profile details.
        /// </summary>
        /// <returns><see cref="ProfileDto"/> containing Username and Email.</returns>
        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<ProfileDto>> GetProfile()
        {
            var idClaim = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _));
            if (idClaim == null)
                return Unauthorized(new { message = "ID utilisateur introuvable dans le token" });

            var userId = int.Parse(idClaim.Value);
            var user   = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(new ProfileDto { Username = user.Username!, Email = user.Email! });
        }

        /// <summary>
        /// Updates the authenticated user's profile information.
        /// </summary>
        /// <param name="dto">Contains optional new Username, Email, and password change info.</param>
        /// <returns>Ok with update message or error.</returns>
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        {
            var idClaim = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _));
            if (idClaim == null)
                return Unauthorized(new { message = "ID utilisateur introuvable dans le token" });

            var userId = int.Parse(idClaim.Value);
            var user   = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            if (!string.IsNullOrEmpty(dto.Username))
                user.Username = dto.Username;
            if (!string.IsNullOrEmpty(dto.Email))
                user.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                if (string.IsNullOrEmpty(dto.CurrentPassword) ||
                    !BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                {
                    return BadRequest(new { message = "Mot de passe actuel incorrect" });
                }
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Profil mis à jour" });
        }

        /// <summary>
        /// Deletes the authenticated user's account.
        /// </summary>
        /// <returns>Ok with deletion message or error.</returns>
        [Authorize]
        [HttpDelete("profile")]
        public async Task<IActionResult> DeleteProfile()
        {
            var idClaim = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _));
            if (idClaim == null)
                return Unauthorized(new { message = "ID utilisateur introuvable dans le token" });

            var userId = int.Parse(idClaim.Value);
            var user   = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Compte supprimé" });
        }
    }
}
