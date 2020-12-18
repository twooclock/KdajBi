using AutoMapper;
using KdajBi.API.Controllers.Resources;
using KdajBi.Core.dtoModels;
using KdajBi.Core.Models;

namespace KdajBi.Mapping
{
    public class ResourceToModelProfile : Profile
    {
        public ResourceToModelProfile()
        {
            CreateMap<UserCredentialsResource, AppUser>();
            CreateMap<dtoClient, Client>();
            
        }
    }
}