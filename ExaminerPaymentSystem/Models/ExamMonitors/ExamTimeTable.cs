using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace ExaminerPaymentSystem.Models.ExamMonitors
{
    public class ExamTimeTable
    {
        [Key]
        public int Id { get; set; }
        public string CentreCode { get; set; }
        public DateTime Exam_date { get; set; }

    }



}
