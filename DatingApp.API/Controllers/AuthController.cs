using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Models;
using DatingApp.API.Models.DataTransferObjects;
using DatingApp.API.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers {
    [ApiController]
    [Route ("[controller]")]
    public class AuthController : ControllerBase {
        private readonly IAuthRepository AuthRepository;
        private readonly IConfiguration Configuration;
        private readonly IMapper mapper;
        public AuthController (IAuthRepository _AuthRepository, IConfiguration _configuration, IMapper _mapper) {
            AuthRepository = _AuthRepository;
            Configuration = _configuration;
            mapper = _mapper;
        }

        [HttpPost ("register")]
        public async Task<IActionResult> Register (UserRegisterDTO user) {
            user.Username = user.Username.ToLower ();
            if (await AuthRepository.IsUserExists (user.Username)) {
                return BadRequest ("User name already exists.");
            }
            var userToCreate = mapper.Map<User> (user);
            var userCreate = await AuthRepository.Register (userToCreate, user.Password);
            var userToReturn = mapper.Map<UserForDetailsDTO> (userCreate);

            return CreatedAtRoute ("GetUsers", new { controller = "User", id = userCreate.Id }, userToReturn);
        }

        [HttpPost ("login")]
        public async Task<IActionResult> Login (UserLoginDTO user) {

            var authUser = await AuthRepository.Login (user.UserName.ToLower (), user.Password);
            if (authUser == null)
                return Unauthorized ();

            var claims = new [] {
                new Claim (ClaimTypes.NameIdentifier, authUser.Id.ToString ()),
                new Claim (ClaimTypes.Name, authUser.UserName)
            };

            var key = new SymmetricSecurityKey (Encoding.UTF8.GetBytes (Configuration.GetSection ("AppSettings:Token").Value));

            var creds = new SigningCredentials (key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity (claims),
                Expires = DateTime.Now.AddDays (1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler ();

            var token = tokenHandler.CreateToken (tokenDescriptor);
            var returnUser = mapper.Map<UserForListDTO> (authUser);
            return Ok (new {
                token = tokenHandler.WriteToken (token),
                    user = returnUser
            });
        }
    }
}