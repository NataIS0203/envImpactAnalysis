using AutoMapper;
using Durable.Service.Models;
using OpenAI.Chat;

namespace Durable.Configuration.Maps
{
    public class DataExportProfile : Profile
    {
        public DataExportProfile()
        {
           // CreateMap<string, ChatMessage>()
            //    .ForMember(dest => dest, options => options.MapFrom(src =>new UserChatMessage( src)));
        }
    }
}
