using AutoMapper;
using ExaminerPaymentSystem.Interfaces.ExamMonitors;
using ExaminerPaymentSystem.Models.ExamMonitors;
using ExaminerPaymentSystem.Models.ExamMonitors.Dtos;

namespace ExaminerPaymentSystem.Services.ExamMonitors
{
    // IExamMonitorService.cs
    public interface IExamMonitorService
    {
        Task<IEnumerable<ExamMonitorDTO>> GetAllAsync();
        Task<ExamMonitorDTO> GetByIdAsync(int id);
        Task CreateAsync(ExamMonitorCreateDTO dto);
        Task UpdateAsync(int id, ExamMonitorUpdateDTO dto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }

    // ExamMonitorService.cs
    public class ExamMonitorService : IExamMonitorService
    {
        private readonly IExamMonitorRepository _repository;
        private readonly IMapper _mapper;

        public ExamMonitorService(IExamMonitorRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ExamMonitorDTO>> GetAllAsync()
        {
            var monitors = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ExamMonitorDTO>>(monitors);
        }

        public async Task<ExamMonitorDTO> GetByIdAsync(int id)
        {
            var monitor = await _repository.GetByIdAsync(id);
            return _mapper.Map<ExamMonitorDTO>(monitor);
        }

        public async Task CreateAsync(ExamMonitorCreateDTO dto)
        {
            var monitor = _mapper.Map<ExamMonitor>(dto);
            await _repository.AddAsync(monitor);
            await _repository.SaveAsync();
        }

        public async Task UpdateAsync(int id, ExamMonitorUpdateDTO dto)
        {
            var monitor = await _repository.GetByIdAsync(id);
            _mapper.Map(dto, monitor);
            _repository.Update(monitor);
            await _repository.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var monitor = await _repository.GetByIdAsync(id);
            _repository.Delete(monitor);
            await _repository.SaveAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _repository.ExistsAsync(id);
        }
    }
}
