using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels.Teacher
{
    public class ReportDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalLessons { get; set; }
        public int TotalSubmissions { get; set; }

        public List<string> AttendanceLabels { get; set; } = new List<string>();
        public List<int> AttendanceValues { get; set; } = new List<int>();

        public List<string> SkillLabels { get; set; } = new List<string>();
        public List<double> SkillValues { get; set; } = new List<double>();

        public List<string> NodeLabels { get; set; } = new List<string>();
        public List<int> NodeValues { get; set; } = new List<int>();
    }
}
