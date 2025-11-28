using AutoMapper;
using TaskManager.Web.DTOs;
using TaskManager.Web.Models;
using TaskManager.Web.Services;

namespace TaskManager.Web.Mappings;

/// <summary>
/// Perfil de mapeamento do AutoMapper
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // TaskItem -> TaskDto
        CreateMap<TaskItem, TaskDto>();

        // CreateTaskDto -> TaskItem
        CreateMap<CreateTaskDto, TaskItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Completed, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore());

        // UpdateTaskDto -> TaskItem
        CreateMap<UpdateTaskDto, TaskItem>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

        // TaskStatistics -> TaskStatisticsDto
        CreateMap<TaskStatistics, TaskStatisticsDto>();
    }
}
