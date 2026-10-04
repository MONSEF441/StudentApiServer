using System;
using System.Collections.Generic;
using StudentsDataAccess;

namespace StudentsBusinessLogic
{
    public class clsStudent
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode = enMode.AddNew;

        public int StudentID { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public decimal Grade { get; set; }

        public clsStudent()
        {
            StudentID = -1;
            FullName = "";
            Age = 0;
            Grade = 0.0m;
            Mode = enMode.AddNew;
        }

        private clsStudent(int studentID, string fullName   , int age, decimal grade)
        {
            StudentID = studentID;
            FullName = fullName ?? string.Empty; Age = age;
            Grade = grade;
            Mode = enMode.Update;
        }

        public static clsStudent? Find(int studentID)
        {
            string fullName = "";
            int age = -1;
            decimal grade = -1;

            if (clsStudentData.GetStudentByID(studentID, ref fullName, ref age, ref grade))
            {
                return new clsStudent(studentID, fullName, age, grade);
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
            StudentID = clsStudentData.AddNewStudent(FullName, Age, Grade);
            return StudentID != -1;
        }

        private bool _UpdateStudent()
        {
            return clsStudentData.UpdateStudent(StudentID, FullName, Age, Grade);
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
    }
}