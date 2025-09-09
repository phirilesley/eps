using ExaminerPaymentSystem.Data;
using Microsoft.EntityFrameworkCore;
using ExaminerPaymentSystem.Interfaces.Other;
using ExaminerPaymentSystem.Models.Other;

namespace ExaminerPaymentSystem.Repositories.Examiners
{
    public class ExamCodesRepository: IExamCodesRepository
    {

        private readonly ApplicationDbContext _examcodescontext; 

        public ExamCodesRepository(ApplicationDbContext examcodescontext) 
        {
            _examcodescontext = examcodescontext;
        }

        public async Task<IEnumerable<ExamCodes>> GetAllExamCodes()  
        {
            return await _examcodescontext.CAN_EXAM.ToListAsync(); 
        }

        public async Task<ExamCodes> GetExamCodesById(string examcode)
        {
            var data = await _examcodescontext.CAN_EXAM.FirstOrDefaultAsync(e => e.EXM_EXAM_CODE == examcode); 
            return data;
        }
    }
}
