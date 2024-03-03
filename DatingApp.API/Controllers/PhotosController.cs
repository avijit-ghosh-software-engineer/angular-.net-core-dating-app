using System.Security.Claims;
using AutoMapper;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using DatingApp.API.Models.DataTransferObjects;
using DatingApp.API.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route ("users/{userId}/photos")]
    public class PhotosController : ControllerBase 
    {
        private readonly IDatingRepository DatingRepository;
        private readonly ImagesPathSettings ImagesPathSettings;
        private readonly IMapper mapper;
        public PhotosController (IDatingRepository _DatingRepository, IMapper _mapper, ImagesPathSettings _ImagesPathSettings) {
            DatingRepository = _DatingRepository;
            mapper = _mapper;
            ImagesPathSettings = _ImagesPathSettings;
        }

        [HttpGet ("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto (int id) {
            var photo = await DatingRepository.GetPhoto (id);
            var returnPhoto = mapper.Map<PhotoForReturnDTO> (photo);
            return Ok (returnPhoto);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser (int userId, [FromForm] PhotoForCreationDTO photoForCreationDTO) {
            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            }
            var user = await DatingRepository.GetUser (userId);

            var file = photoForCreationDTO.File;

            if (file.Length > 0) {
                var imageUrl = await DatingRepository.FileUpload (userId, file);
                var domain = Environment.GetEnvironmentVariable ("ASPNETCORE_URLS").Split (";");

                photoForCreationDTO.Url = domain[0] + "/" + ImagesPathSettings.UserImages + userId + "/" + imageUrl;
            }

            var photo = mapper.Map<Photo> (photoForCreationDTO);

            if (!user.Photos.Any (x => x.IsMain)) {
                photo.IsMain = true;
            }

            user.Photos.Add (photo);

            if (await DatingRepository.SaveAll ()) {
                var photoToReturn = mapper.Map<PhotoForReturnDTO> (photo);
                //return CreatedAtRoute ("GetPhoto", new { id = photo.Id }, photoToReturn);
                return Ok (photoToReturn);
            }

            return BadRequest ("Could not add the photo.");
        }

        [HttpPost ("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto (int userId, int id) {
            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            }

            var user = await DatingRepository.GetUser (userId);

            if (!user.Photos.Any (x => x.Id == id)) {
                return Unauthorized ();
            }

            var photo = await DatingRepository.GetPhoto (id);

            if (photo.IsMain) {
                return BadRequest ("This is already the main photo.");
            }

            var currentMainPhoto = await DatingRepository.GetMainPhotoForUser (userId);
            currentMainPhoto.IsMain = false;

            photo.IsMain = true;

            if (await DatingRepository.SaveAll ()) {
                return NoContent ();
            }
            return BadRequest ("Could not set photo to main.");
        }

        [HttpDelete ("{id}")]
        public async Task<IActionResult> DeletePhoto (int userId, int id) {
            if (userId != int.Parse (User.FindFirst (ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized ();
            }

            var user = await DatingRepository.GetUser (userId);

            if (!user.Photos.Any (x => x.Id == id)) {
                return Unauthorized ();
            }

            var photo = await DatingRepository.GetPhoto (id);

            if (photo.IsMain) {
                return BadRequest ("You cannot delete your main photo.");
            }

            if (await DatingRepository.FileDelete (photo)) {
                return Ok ();
            }
            return BadRequest ("Failed to delete the photo.");
        }

    }
}