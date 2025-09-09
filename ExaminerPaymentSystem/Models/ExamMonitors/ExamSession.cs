using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.ExamMonitors
{
    public class ExamSession
    {
        [Key]
        public int Id { get; set; }
        public string SessionCode { get; set; }
        public string SessionName { get; set; }

        public string SessionYear { get; set; } 
    }
}
