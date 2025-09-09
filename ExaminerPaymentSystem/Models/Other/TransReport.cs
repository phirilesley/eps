namespace ExaminerPaymentSystem.Models.Other
{
    public class TransReport
    {
        public string? Subject { get; set; }
        public string? Region { get; set; }
        public string? Cat {  get; set; }
        public string ? Trans {  get; set; }
        public string ? Ref {  get; set; }
        public string ? Org {  get; set; }

       public string ? Status {  get; set; }
    }

public class Ref
{
    public string? PMSShare { get; set; }
        public string? RPMSShare { get; set; }
        public string? DPMSShare { get; set; }
        public string? BMSShare { get; set; }


    }

    public class Trans
    {
        public string? PMSNum { get; set; }
        public string? RPMSNum { get; set; }
        public string? DPMSNum { get; set; }
        public string? BMSNum { get; set; }

        public string? ENum { get; set; }


    }

    public class Exa
    {
        public string? PMSNumber { get; set; }
        public string? RPMSNumber { get; set; }
        public string? DPMSNumber { get; set; }
        public string? BMSNumber { get; set; }

        public string? ENumber { get; set; }

    }
}
