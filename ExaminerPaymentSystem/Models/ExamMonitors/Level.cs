using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.Models.ExamMonitors
{
    public class Level
    {
        [Key]
        public string LevelCode
        {
            get; set;
        }
        public string LevelName { get; set; }
        public  int Weight { get; set; }
    }
}
