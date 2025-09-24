using System.ComponentModel.DataAnnotations;
using ExaminerPaymentSystem.Models.Major;

namespace ExaminerPaymentSystem.ViewModels.Examiners;

public class AddExaminerViewModel
{
    [Required(ErrorMessage = "Exam code is required.")]
    public string ExamCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Activity is required.")]
    public string Activity { get; set; } = string.Empty;

    [Required(ErrorMessage = "Examiner code is required.")]
    public string EMS_EXAMINER_CODE { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subject code is required.")]
    public string EMS_SUB_SUB_ID { get; set; } = string.Empty;

    [Required(ErrorMessage = "Paper code is required.")]
    public string EMS_PAPER_CODE { get; set; } = string.Empty;

    [Required(ErrorMessage = "Examiner category is required.")]
    public string EMS_ECT_EXAMINER_CAT_CODE { get; set; } = string.Empty;

    [Required(ErrorMessage = "National ID is required.")]
    public string EMS_NATIONAL_ID { get; set; } = string.Empty;

    [Required(ErrorMessage = "Examiner number is required.")]
    public string EMS_EXAMINER_NUMBER { get; set; } = string.Empty;

    [Required(ErrorMessage = "Supervisor code is required.")]
    public string EMS_EXM_SUPERORD { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required.")]
    public string EMS_EXAMINER_NAME { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    public string EMS_LAST_NAME { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gender is required.")]
    public string EMS_SEX { get; set; } = string.Empty;

    [Required(ErrorMessage = "Region is required.")]
    public string EMS_REGION_CODE { get; set; } = string.Empty;

    [Required(ErrorMessage = "Marking region is required.")]
    public string EMS_MARKING_REG_CODE { get; set; } = string.Empty;

    [Required(ErrorMessage = "Level of exam marked is required.")]
    public string EMS_LEVEL_OF_EXAM_MARKED { get; set; } = string.Empty;

    public Examiner ToExaminer()
    {
        return new Examiner
        {
            EMS_EXAMINER_CODE = EMS_EXAMINER_CODE,
            EMS_SUB_SUB_ID = EMS_SUB_SUB_ID,
            EMS_PAPER_CODE = EMS_PAPER_CODE,
            EMS_ECT_EXAMINER_CAT_CODE = EMS_ECT_EXAMINER_CAT_CODE,
            EMS_NATIONAL_ID = EMS_NATIONAL_ID,
            EMS_EXAMINER_NUMBER = EMS_EXAMINER_NUMBER,
            EMS_EXM_SUPERORD = EMS_EXM_SUPERORD,
            EMS_EXAMINER_NAME = EMS_EXAMINER_NAME,
            EMS_LAST_NAME = EMS_LAST_NAME,
            EMS_SEX = EMS_SEX,
            EMS_REGION_CODE = EMS_REGION_CODE,
            EMS_MARKING_REG_CODE = EMS_MARKING_REG_CODE,
            EMS_LEVEL_OF_EXAM_MARKED = EMS_LEVEL_OF_EXAM_MARKED
        };
    }
}

public class ReplaceExaminerViewModel
{
    [Required(ErrorMessage = "Activity is required.")]
    public string Activity { get; set; } = string.Empty;

    [Required(ErrorMessage = "Examiner code is required.")]
    public string EMS_EXAMINER_CODE { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subject code is required.")]
    public string EMS_SUB_SUB_ID { get; set; } = string.Empty;

    [Required(ErrorMessage = "Paper code is required.")]
    public string EMS_PAPER_CODE { get; set; } = string.Empty;

    [Required(ErrorMessage = "Examiner category is required.")]
    public string EMS_ECT_EXAMINER_CAT_CODE { get; set; } = string.Empty;

    [Required(ErrorMessage = "National ID is required.")]
    public string EMS_NATIONAL_ID { get; set; } = string.Empty;

    [Required(ErrorMessage = "Examiner number is required.")]
    public string EMS_EXAMINER_NUMBER { get; set; } = string.Empty;

    [Required(ErrorMessage = "Supervisor code is required.")]
    public string EMS_EXM_SUPERORD { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required.")]
    public string EMS_EXAMINER_NAME { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    public string EMS_LAST_NAME { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gender is required.")]
    public string EMS_SEX { get; set; } = string.Empty;

    [Required(ErrorMessage = "Region is required.")]
    public string EMS_REGION_CODE { get; set; } = string.Empty;

    [Required(ErrorMessage = "Marking region is required.")]
    public string EMS_MARKING_REG_CODE { get; set; } = string.Empty;

    [Required(ErrorMessage = "Level of exam marked is required.")]
    public string EMS_LEVEL_OF_EXAM_MARKED { get; set; } = string.Empty;

    public Examiner ToExaminer()
    {
        return new Examiner
        {
            EMS_EXAMINER_CODE = EMS_EXAMINER_CODE,
            EMS_SUB_SUB_ID = EMS_SUB_SUB_ID,
            EMS_PAPER_CODE = EMS_PAPER_CODE,
            EMS_ECT_EXAMINER_CAT_CODE = EMS_ECT_EXAMINER_CAT_CODE,
            EMS_NATIONAL_ID = EMS_NATIONAL_ID,
            EMS_EXAMINER_NUMBER = EMS_EXAMINER_NUMBER,
            EMS_EXM_SUPERORD = EMS_EXM_SUPERORD,
            EMS_EXAMINER_NAME = EMS_EXAMINER_NAME,
            EMS_LAST_NAME = EMS_LAST_NAME,
            EMS_SEX = EMS_SEX,
            EMS_REGION_CODE = EMS_REGION_CODE,
            EMS_MARKING_REG_CODE = EMS_MARKING_REG_CODE,
            EMS_LEVEL_OF_EXAM_MARKED = EMS_LEVEL_OF_EXAM_MARKED
        };
    }
}

public class ExaminerTransactionViewModel
{
    [Required(ErrorMessage = "Examiner code is required.")]
    public string EMS_EXAMINER_CODE { get; set; } = string.Empty;

    [Required(ErrorMessage = "National ID is required.")]
    public string EMS_NATIONAL_ID { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required.")]
    public string EMS_EXAMINER_NAME { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    public string EMS_LAST_NAME { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subject code is required.")]
    public string EMS_SUB_SUB_ID { get; set; } = string.Empty;

    [Required(ErrorMessage = "Paper code is required.")]
    public string EMS_PAPER_CODE { get; set; } = string.Empty;

    [Required(ErrorMessage = "Examiner number is required.")]
    public string EMS_EXAMINER_NUMBER { get; set; } = string.Empty;

    [Required(ErrorMessage = "Supervisor code is required.")]
    public string EMS_EXM_SUPERORD { get; set; } = string.Empty;

    [Required(ErrorMessage = "Examiner category is required.")]
    public string EMS_ECT_EXAMINER_CAT_CODE { get; set; } = string.Empty;

    [Required(ErrorMessage = "Marking region is required.")]
    public string EMS_MARKING_REG_CODE { get; set; } = string.Empty;

    [Required(ErrorMessage = "Activity is required.")]
    public string Activity { get; set; } = string.Empty;
}
