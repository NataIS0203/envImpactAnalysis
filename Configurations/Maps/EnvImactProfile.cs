using AutoMapper;
using Durable.Functions.Models;
using Durable.Models;
using Durable.Service.Models;

namespace Durable.Configuration.Maps
{
    public class DataExportProfile : Profile
    {
        public DataExportProfile()
        {
            CreateMap<GetBaseRequest, ReportBaseModel>()
                .ForMember(dest => dest.Percentage, options => options.MapFrom((src, dest) => int.Parse(src.Percentage ?? "100")));

            CreateMap<DurableResponse, GetOutputResponse>()
                .ForMember(dest => dest.Output, options => options.MapFrom((src, dest) => src.Output));
        }
    }
}
