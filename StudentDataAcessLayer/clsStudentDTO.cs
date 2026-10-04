using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace StudentsDataAccess
{
    // Class 1: The Data Transfer Object (POCO / Data Carrier)
    public class clsStudentDTO
    {
        public int StudentID { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public decimal Grade { get; set; }

        public clsStudentDTO(int studentID, string fullName, int age, decimal grade)
        {
            StudentID = studentID;
            FullName = fullName;
            Age = age;
            Grade = grade;
        }
    }

    // Class 2: The Database Operations Engine
    public static class clsStudentData
    {
        private static readonly string _connectionString =
            "Server=.;Database=dbStudents;Trusted_Connection=True;TrustServerCertificate=True;";

        public static List<clsStudentDTO> GetAllStudents()
        {
            var studentsList = new List<clsStudentDTO>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SP_GetAllStudents", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        studentsList.Add(new clsStudentDTO(
                            Convert.ToInt32(reader["StudentID"]),
                            reader["FullName"]?.ToString() ?? string.Empty,
                            Convert.ToInt32(reader["Age"]),
                            Convert.ToDecimal(reader["Grade"])
                        ));
                    }
                }
            }

            return studentsList;
        }

        public static List<clsStudentDTO> GetPassedStudents(decimal passingGrade = 50.00m)
        {
            var studentsList = new List<clsStudentDTO>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SP_GetPassedStudents", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@PassingGrade", passingGrade);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        studentsList.Add(new clsStudentDTO(
                            Convert.ToInt32(reader["StudentID"]),
                            reader["FullName"]?.ToString() ?? string.Empty,
                            Convert.ToInt32(reader["Age"]),
                            Convert.ToDecimal(reader["Grade"])
                        ));
                    }
                }
            }

            return studentsList;
        }

        public static bool GetStudentByID(int studentID, ref string fullName, ref int age, ref decimal grade)
        {
            bool isFound = false;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SP_GetStudentByID", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@StudentID", studentID);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        isFound = true;
                        fullName = reader["FullName"]?.ToString() ?? string.Empty;
                        age = Convert.ToInt32(reader["Age"]);
                        grade = Convert.ToDecimal(reader["Grade"]);
                    }
                }
            }

            return isFound;
        }

        public static int AddNewStudent(string fullName, int age, decimal grade)
        {
            int newStudentID = -1;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SP_AddStudent", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@FullName", fullName);
                command.Parameters.AddWithValue("@Age", age);
                command.Parameters.AddWithValue("@Grade", grade);

                SqlParameter outputParam = new SqlParameter("@NewStudentID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputParam);

                connection.Open();
                command.ExecuteNonQuery();

                if (outputParam.Value != DBNull.Value)
                    newStudentID = (int)outputParam.Value;
            }

            return newStudentID;
        }

        public static bool UpdateStudent(int studentID, string fullName, int age, decimal grade)
        {
            int rowsAffected = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SP_UpdateStudent", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@StudentID", studentID);
                command.Parameters.AddWithValue("@FullName", fullName);
                command.Parameters.AddWithValue("@Age", age);
                command.Parameters.AddWithValue("@Grade", grade);

                connection.Open();
                rowsAffected = command.ExecuteNonQuery();
            }

            return rowsAffected > 0;
        }

        public static bool DeleteStudent(int studentID)
        {
            int rowsAffected = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SP_DeleteStudent", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@StudentID", studentID);

                connection.Open();
                rowsAffected = command.ExecuteNonQuery();
            }

            return rowsAffected > 0;
        }
    }
}