using System;

namespace DuAnTotNghiep.Models.ViewModels.Teacher
{
    public class StudentAttendanceViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentEmail { get; set; } = "";
        public string Status { get; set; } = "PRESENT";
        public string? Remarks { get; set; }
    }
}
