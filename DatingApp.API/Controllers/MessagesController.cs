using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
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
    [Route ("/users/{userId}/[controller]")]
    [ServiceFilter (typeof (LogUserActivity))]
    public class MessagesController : ControllerBase {
        private readonly IDatingRepository DatingRepository;
        private readonly IMapper mapper;
        public MessagesController (IDatingRepository _DatingRepository, IMapper _mapper) {
            DatingRepository = _DatingRepository;
            mapper = _mapper;
        }

        [HttpGet ("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage (int userId, int id) {
            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            }

            var message = await DatingRepository.GetMessage (id);
            if (message == null) {
                return NotFound ();
            }

            return Ok (message);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser (int userId, [FromQuery] MessageParams messageParams) {
            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            }
            messageParams.UserId = userId;
            var userMessages = await DatingRepository.GetMessagesForUser (messageParams);
            var messages = mapper.Map<IEnumerable<MessageToReturnDTO>> (userMessages);

            Response.AddPagination (userMessages.CurrentPage, userMessages.PageSize, userMessages.TotalCount, userMessages.TotalPages);

            return Ok (messages);
        }

        [HttpGet ("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread (int userId, int recipientId) {
            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            }
            var message = await DatingRepository.GetMessageThread (userId, recipientId);
            var messageToReturn = mapper.Map<IEnumerable<MessageToReturnDTO>> (message);
            return Ok (messageToReturn);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage (int userId, MessageForCreationDTO messageForCreationDTO) {
            var sender = await DatingRepository.GetUser (userId);

            if (sender.Id != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            }
            messageForCreationDTO.SenderId = userId;
            var recipient = await DatingRepository.GetUser (messageForCreationDTO.RecipientId);
            if (recipient == null) {
                return BadRequest ("Could not find user.");
            }

            var message = mapper.Map<Message> (messageForCreationDTO);

            DatingRepository.Add (message);

            if (await DatingRepository.SaveAll ()) {
                var messageToReturn = mapper.Map<MessageToReturnDTO> (message);
                return CreatedAtRoute ("GetMessage", new { userId = userId, id = message.Id }, messageToReturn);
            }

            throw new Exception ("Create the message failed on save.");
        }

        [HttpPost ("{id}")]
        public async Task<IActionResult> DeleteMessage (int id, int userId) {
            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            }

            var message = await DatingRepository.GetMessage (id);
            if (message.SenderId == userId) {
                message.SenderDeleted = true;
            }
            if (message.RecipientId == userId) {
                message.RecipientDeleted = true;
            }
            if (message.SenderDeleted && message.RecipientDeleted) {
                DatingRepository.Delete (message);
            }

            if (await DatingRepository.SaveAll ()) {
                return NoContent ();
            }

            throw new Exception ("Error deleting the message.");
        }

        [HttpPost ("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead (int userId, int id) {
            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            }

            var message = await DatingRepository.GetMessage (id);
            if (message.RecipientId != userId) {
                return Unauthorized ();
            }

            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await DatingRepository.SaveAll ();
            return NoContent ();
        }
    }
}