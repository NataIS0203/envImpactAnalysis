using AutoMapper;
using Durable.Functions.Models;
using Durable.Service.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Visitors;
using OpenAI.Chat;

namespace Durable.Configuration.Maps
{
    public class DataExportProfile : Profile
    {
        public DataExportProfile()
        {
            CreateMap<GetBaseRequest, ReportBaseModel>()
                .ForMember(dest => dest.Percentage, options => options.MapFrom((src, dest) => int.Parse(src.Percentage??"100")));
        }
    }
}
