using AutoMapper;
using Durable.Service.Models;
using OpenAI.Chat;

namespace Durable.Configuration.Maps
{
    public class DataExportProfile : Profile
    {
        public DataExportProfile()
        {
            CreateMap<PromptModel, UserChatMessage>()
                .ForMember(dest => dest.Content, options => options.MapFrom(src => src.Questions));

            CreateMap<List<string>, ChatMessageContent>();
        }
    }
}
