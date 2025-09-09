using System;
using System.ComponentModel.DataAnnotations;


namespace ExaminerPaymentSystem.ViewModel.ExaminerRecutiments
{
    public class FileValidationAttribute : ValidationAttribute
    {
        private readonly int _maxFileSizeInMB; // Maximum file size in MB
        private readonly string[] _allowedExtensions; // Allowed file extensions

        public FileValidationAttribute(int maxFileSizeInMB, string[] allowedExtensions)
        {
            _maxFileSizeInMB = maxFileSizeInMB;
            _allowedExtensions = allowedExtensions;
        }

      
    }
}
