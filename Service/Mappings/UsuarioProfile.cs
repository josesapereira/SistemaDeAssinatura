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
    }
}



