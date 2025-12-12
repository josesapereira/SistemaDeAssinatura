using AutoMapper;
using Domain.Models;
using Domain.DTOs;

namespace Service.Mappings;

public class UsuarioProfile : Profile
{
    public UsuarioProfile()
    {
        CreateMap<Usuario, UsuarioDto>()
            .ReverseMap();
        
        CreateMap<Usuario, UsuarioListagemDTO>()
            .ForMember(dest => dest.RE, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Nome, opt => opt.Ignore());
        
        CreateMap<Usuario, UsuarioDetalhesDTO>()
            .ForMember(dest => dest.RE, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Nome, opt => opt.Ignore());
    }
}



