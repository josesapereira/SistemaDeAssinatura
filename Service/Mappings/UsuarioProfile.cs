using AutoMapper;
using Domain.Models;
using Domain.DTOs;

namespace Service.Mappings;

public class UsuarioProfile : Profile
{
    public UsuarioProfile()
    {
        CreateMap<Usuario, UsuarioListagemDTO>()
            .ForMember(dest => dest.RE, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Nome, opt => opt.Ignore());
        
        CreateMap<Usuario, UsuarioDetalhesDTO>()
            .ForMember(dest => dest.RE, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Nome, opt => opt.Ignore());

        // Mapeamento Usuario -> UsuarioDTO (completo)
        CreateMap<Usuario, UsuarioDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => src.Ativo))
            .ForMember(dest => dest.NomeDoArquivo, opt => opt.MapFrom(src => src.NomeDoArquivo))
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.Roles != null && src.Roles.Count > 0 ? src.Roles[0].Role.Id : (long?)null))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Roles != null && src.Roles.Count > 0 ? src.Roles[0].Role.Name : string.Empty))
            .ForMember(dest => dest.RoleIdString, opt => opt.Ignore()) // Propriedade calculada, não precisa mapear
            .ForMember(dest => dest.ArquivoUpload, opt => opt.Ignore());

        // Mapeamento UsuarioDTO -> Usuario (reverso completo)
        CreateMap<UsuarioDTO, Usuario>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => src.Ativo))
            .ForMember(dest => dest.NomeDoArquivo, opt => opt.MapFrom(src => src.NomeDoArquivo))
            .ForMember(dest => dest.PrimeiroAcesso, opt => opt.Ignore())
            .ForMember(dest => dest.DoisFatoresAtivo, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.Roles, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
            .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
            .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
            .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore());
    }
}



