using AutoMapper;
using ExaminerPaymentSystem.Models.ExamMonitors;
using ExaminerPaymentSystem.Models.ExamMonitors.Dtos;

namespace ExaminerPaymentSystem.Services
{
    public class AutomapperProfileService : Profile
    {
  
    }

    public class ExamMonitorProfile : Profile
    {
        public ExamMonitorProfile()
        {
            CreateMap<ExamMonitor, ExamMonitorDTO>().ReverseMap();
            CreateMap<ExamMonitorCreateDTO, ExamMonitor>();
            CreateMap<ExamMonitorUpdateDTO, ExamMonitor>();
            CreateMap<ExamMonitorDTO, ExamMonitorUpdateDTO>();
        }
    }
}
