using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using DatingApp.API.Models.DataTransferObjects;
using DatingApp.API.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers {
    [Authorize]
    [ApiController]
    [Route ("[controller]")]
    [ServiceFilter (typeof (LogUserActivity))]
    public class UserController : ControllerBase {
        private readonly IDatingRepository DatingRepository;
        private readonly IMapper mapper;
        public UserController (IDatingRepository _DatingRepository, IMapper _mapper) {
            DatingRepository = _DatingRepository;
            mapper = _mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers ([FromQuery] UserParams userParams) {
            var currentUserID = int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value);
            var currentUser = await DatingRepository.GetUser (currentUserID);
            userParams.UserId = currentUserID;
            if (string.IsNullOrEmpty (userParams.Gender)) {
                userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
            }
            var users = await DatingRepository.GetUsers (userParams);
            var usersToReturn = mapper.Map<IEnumerable<UserForListDTO>> (users);
            Response.AddPagination (users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok (usersToReturn);
        }

        [HttpGet ("{id}", Name = "GetUsers")]
        public async Task<IActionResult> GetUsers (int id) {
            var user = await DatingRepository.GetUser (id);
            var userToReturn = mapper.Map<UserForDetailsDTO> (user);
            return Ok (userToReturn);
        }

        [HttpPut ("{id}")]
        public async Task<IActionResult> UpdateUser (int id, UserForUpdateDTO userForUpdateDTO) {
            if (id != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            }

            var user = await DatingRepository.GetUser (id);
            mapper.Map (userForUpdateDTO, user);

            if (await DatingRepository.SaveAll ()) {
                return NoContent ();
            }

            throw new Exception ($"Updating user {id} failed on save.");
        }

        [HttpPost ("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser (int id, int recipientId) {
            if (id != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            }
            var like = await DatingRepository.GetLike (id, recipientId);
            if (like != null) {
                return BadRequest ("You already like this user.");
            }
            if (await DatingRepository.GetUser (recipientId) == null) {
                return NotFound ();
            }

            like = new Like {
                LikerId = id,
                LikeeId = recipientId
            };

            DatingRepository.Add<Like> (like);
            if (await DatingRepository.SaveAll ()) {
                return Ok ();
            }

            return BadRequest ("Failed to like user.");
        }
    }
}