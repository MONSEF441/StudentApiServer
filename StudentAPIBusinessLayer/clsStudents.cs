using System;
using System.Collections.Generic;
using StudentApi.Data;

namespace StudentApi.Models
{
    public class Student
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode = enMode.AddNew;

        public int StudentID { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public decimal Grade { get; set; }

        // Authentication-related fields
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }

        public Student()
        {
            StudentID = -1;
            FullName = string.Empty;
            Age = 0;
            Grade = 0.0m;
            Email = string.Empty;
            PasswordHash = string.Empty;
            Role = string.Empty;
            Mode = enMode.AddNew;
        }

        private Student(int studentID, string fullName, int age, decimal grade, string email, string passwordHash, string role)
        {
            StudentID = studentID;
            FullName = fullName ?? string.Empty;
            Age = age;
            Grade = grade;
            Email = email ?? string.Empty;
            PasswordHash = passwordHash ?? string.Empty;
            Role = role ?? string.Empty;
            Mode = enMode.Update;
        }

        public static Student? Find(int studentID)
        {
            string fullName = string.Empty;
            int age = -1;
            decimal grade = -1;
            string email = string.Empty;
            string passwordHash = string.Empty;
            string role = string.Empty;

            if (clsStudentData.GetStudentByID(studentID, ref fullName, ref age, ref grade, ref email, ref passwordHash, ref role))
            {
                return new Student(studentID, fullName, age, grade, email, passwordHash, role);
            }

            return null;
        }

        public static List<clsStudentDTO> GetAllStudents()
        {
            return clsStudentData.GetAllStudents();
        }

        public static List<clsStudentDTO> GetPassedStudents(decimal passingGrade = 50.0m)
        {
            return clsStudentData.GetPassedStudents(passingGrade);
        }

        private bool _AddNewStudent()
        {
            _EnsurePasswordHashed();
            StudentID = clsStudentData.AddNewStudent(FullName, Age, Grade, Email, PasswordHash, Role);
            return StudentID != -1;
        }

        private bool _UpdateStudent()
        {
            _EnsurePasswordHashed();
            return clsStudentData.UpdateStudent(StudentID, FullName, Age, Grade, Email, PasswordHash, Role);
        }

        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewStudent())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return _UpdateStudent();
            }

            return false;
        }

        public static bool Delete(int studentID)
        {
            return clsStudentData.DeleteStudent(studentID);
        }
        private static bool _IsBcryptHash(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   (value.StartsWith("$2a$") || value.StartsWith("$2b$") || value.StartsWith("$2y$"));
        }

        private void _EnsurePasswordHashed()
        {
            if (string.IsNullOrWhiteSpace(PasswordHash))
                return;

            PasswordHash = PasswordHash.Trim();

            if (!_IsBcryptHash(PasswordHash))
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordHash);
        }

       
    }
}