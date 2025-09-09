using System.ComponentModel.DataAnnotations;

namespace ExaminerPaymentSystem.ViewModels.Examiners
{
    public class SelectTeamViewModel
    {
        public string SubjectCode { get; set; }
        public string PaperCode { get; set; }


        public string SubKey { get; set; }
        public string RegisterStatus { get; set; }
   
        public string Name { get; set; }


        public string IdNumber { get; set; }

    
        public string Sex { get; set; }

      
        public string Category { get; set; }

    
        public string CapturingRole { get; set; }

        public string Region { get; set; }
        public string Station { get; set; }

        public string District { get; set; }
        public string Province { get; set; }

     
        public string Selected { get; set; }


        public string ExaminerNumber { get; set; }


        public string Team { get; set; }

   
        public string Status { get; set; }

        public string Phone { get; set; }

        public List<string> Subjects { get; set; }
    }

    public class SelectTeamViewModel2
    {
        public string SubjectCode { get; set; }
        public string PaperCode { get; set; }


        public string SubKey { get; set; }
        public string RegisterStatus { get; set; }

        public string Name { get; set; }


        public string IdNumber { get; set; }


        public string Sex { get; set; }


        public string Category { get; set; }


        public string CapturingRole { get; set; }

        public string Region { get; set; }
        public string Station { get; set; }

        public string District { get; set; }
        public string Province { get; set; }


        public string Selected { get; set; }


        public string ExaminerNumber { get; set; }


        public string Team { get; set; }


        public string Status { get; set; }

        public string Phone { get; set; }

        public List<string> Subjects { get; set; }
    }
}
