using AutoMapper;
using ProductGrpcService.Domain;
using static ProductGrpcService.Domain.SampleDb;

namespace ProductGrpcService.Config
{
    public class AutoMapperConfig: Profile
    {
        public  AutoMapperConfig()
        {
            CreateMap<Product, ProductResponse>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => (double)src.Price));

            CreateMap<AddProduct, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) 
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => (int)src.Price));
        }

    }
}
