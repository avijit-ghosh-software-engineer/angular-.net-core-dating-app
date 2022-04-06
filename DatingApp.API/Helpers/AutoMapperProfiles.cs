using System.Linq;
using AutoMapper;
using DatingApp.API.Models;
using DatingApp.API.Models.DataTransferObjects;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDTO>()
                .ForMember(dest => dest.PhotoUrl, opt =>
                {
                    opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
                }).ForMember(dest => dest.Age, opt =>
                {
                    opt.ResolveUsing(d => d.DateOfBirth.CalculateAge());
                });

            CreateMap<User, UserForDetailsDTO>()
                .ForMember(dest => dest.PhotoUrl, opt =>
                {
                    opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
                }).ForMember(dest => dest.Age, opt =>
                {
                    opt.ResolveUsing(d => d.DateOfBirth.CalculateAge());
                });

            CreateMap<Photo, PhotoDetailsDTO>();

            CreateMap<UserForUpdateDTO, User>();
            CreateMap<PhotoForCreationDTO, Photo>();
            CreateMap<Photo, PhotoForReturnDTO>();
            CreateMap<UserRegisterDTO, User>();
            CreateMap<MessageForCreationDTO, Message>().ReverseMap();
            CreateMap<Message, MessageToReturnDTO>()
            .ForMember(dest => dest.SenderPhotoUrl, opt =>
            {
                opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(p => p.IsMain).Url);
            }).ForMember(dest => dest.RecipientPhotoUrl, opt =>
            {
                opt.MapFrom(src => src.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url);
            });
        }
    }
}