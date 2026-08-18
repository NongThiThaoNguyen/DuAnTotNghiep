using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels.Student
{
    /// <summary>
    /// ViewModel cho trang chọn lớp - hiển thị danh sách lớp phù hợp với level của Student
    /// </summary>
    public class ClassSelectionViewModel
    {
        public int StudentId { get; set; }
        public string StudentLevel { get; set; } = "";
        public int StudentLevelId { get; set; }
        public List<AvailableClassViewModel> AvailableClasses { get; set; } = new();
        public bool AlreadyEnrolled { get; set; }
        public EnrolledClassViewModel? CurrentEnrollment { get; set; }
    }

    /// <summary>
    /// ViewModel cho 1 lớp trong danh sách chọn
    /// </summary>
    public class AvailableClassViewModel
    {
        public int ClassroomId { get; set; }
        public string ClassName { get; set; } = "";
        public string LevelName { get; set; } = "";
        public string LevelCode { get; set; } = "";
        public string TeacherName { get; set; } = "";
        public string ScheduleDisplay { get; set; } = "";
        public int CurrentStudents { get; set; }
        public int MaxStudents { get; set; }
        public int RemainingSlots => MaxStudents - CurrentStudents;
        public bool IsFull => CurrentStudents >= MaxStudents;
        public DateTime StartDate { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// ViewModel cho lớp mà Student đã đăng ký
    /// </summary>
    public class EnrolledClassViewModel
    {
        public int EnrollmentId { get; set; }
        public int ClassroomId { get; set; }
        public string ClassName { get; set; } = "";
        public string LevelName { get; set; } = "";
        public string TeacherName { get; set; } = "";
        public string ScheduleDisplay { get; set; } = "";
        public DateTime EnrolledAt { get; set; }
    }

    /// <summary>
    /// ViewModel cho trang xác nhận đăng ký lớp
    /// </summary>
    public class ClassEnrollConfirmViewModel
    {
        public AvailableClassViewModel ClassInfo { get; set; } = null!;
        public string StudentName { get; set; } = "";
        public string StudentLevel { get; set; } = "";
    }

    /// <summary>
    /// ViewModel cho trang thành công sau khi đăng ký
    /// </summary>
    public class EnrollmentSuccessViewModel
    {
        public string ClassName { get; set; } = "";
        public string LevelName { get; set; } = "";
        public string TeacherName { get; set; } = "";
        public string ScheduleDisplay { get; set; } = "";
        public DateTime EnrolledAt { get; set; }
    }
}
