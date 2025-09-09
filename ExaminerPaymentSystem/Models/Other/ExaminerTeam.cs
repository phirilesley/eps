namespace ExaminerPaymentSystem.Models.Other
{
    public class ExaminerTeam
    {
        public string SubKey { get; set; }

        public string IdNumber { get; set; }

        public string Sex { get; set; }

        public string Station { get; set; }

        public string District { get; set; }

        public string Province { get; set; }

        public string BMSCode { get; set; }

        public string CapturingRole { get; set; }

        public string Phone { get; set; }

        public string SupervisorName { get; set; }

        public string ExaminerNumber { get; set; }

        public string Role { get; set; }

        public string MarkingRegion { get; set; }

        public List<Team> TeamMembers { get; set; }
    }

    public class Team
    {

        public string SubKey { get; set; }

        public string IdNumber { get; set; }

        public string Sex { get; set; }

        public string Station { get; set; }

        public string District { get; set; }

        public string Province { get; set; }

        public string BMSCode { get; set; }

        public string CapturingRole { get; set; }

        public string Phone { get; set; }

        public string ExaminerName { get; set; }

        public string ExaminerNumber { get; set; }

        public string Role { get; set; }

        public string MarkingRegion { get; set; }
    }

  
}
