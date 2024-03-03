using AutoMapper;
using DatingApp.API.Models;
using DatingApp.API.Models.DataTransferObjects;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<User, UserForListDTO>()
                .ForMember(dest => dest.PhotoUrl, opt =>
                {
                    opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
                }).ForMember(dest => dest.Age, opt =>
                {
                    opt.MapFrom(d => d.DateOfBirth.CalculateAge());
                });

                config.CreateMap<User, UserForDetailsDTO>()
                    .ForMember(dest => dest.PhotoUrl, opt =>
                    {
                        opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
                    }).ForMember(dest => dest.Age, opt =>
                    {
                        opt.MapFrom(d => d.DateOfBirth.CalculateAge());
                    });

                config.CreateMap<Photo, PhotoDetailsDTO>();

                config.CreateMap<UserForUpdateDTO, User>();
                config.CreateMap<PhotoForCreationDTO, Photo>();
                config.CreateMap<Photo, PhotoForReturnDTO>();
                config.CreateMap<UserRegisterDTO, User>();
                config.CreateMap<MessageForCreationDTO, Message>().ReverseMap();
                config.CreateMap<Message, MessageToReturnDTO>()
                .ForMember(dest => dest.SenderPhotoUrl, opt =>
                {
                    opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(p => p.IsMain).Url);
                }).ForMember(dest => dest.RecipientPhotoUrl, opt =>
                {
                    opt.MapFrom(src => src.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url);
                });

            });
            return mappingConfig;
        }
    }
}