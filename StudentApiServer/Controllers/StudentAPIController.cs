using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentApi.Data;
using StudentApi.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
namespace StudentApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Students")]
    public class StudentAPIController : ControllerBase
    {
        private readonly ILogger<StudentAPIController> _logger;

        public StudentAPIController(ILogger<StudentAPIController> logger)
        {
            _logger = logger;
        }


        // GET: api/Students/all
        [Authorize(Roles = "Admin")]
        [HttpGet("all", Name = "GetAllStudents")]
        public IActionResult GetAllStudents()
        {
            try
            {
                List<clsStudentDTO> students = Student.GetAllStudents(); ;

                if (students == null || students.Count == 0)
                    return NotFound(new { message = "No students found." });

                return Ok(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all students.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = ex.Message });
            }
        }


        // GET: api/Students/passed?grade=50
        [HttpGet("passed", Name = "GetPassedStudents")]
        [AllowAnonymous]
        public IActionResult GetPassedStudents([FromQuery] decimal grade = 50.0m)
        {
            if (grade < 0 || grade > 100)
                return BadRequest(new { message = "Invalid passing grade value. Must be between 0 and 100." });

            try
            {
                List<clsStudentDTO> students = Student.GetPassedStudents(grade);

                if (students == null || students.Count == 0)
                    return NotFound(new { message = $"No passed students found with grade >= {grade}." });

                return Ok(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving passed students.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = ex.Message });
            }
        }


        [HttpGet("{id}", Name = "GetStudentById")]
        public async Task<ActionResult<Student>> GetStudentById( int id , [FromServices] IAuthorizationService authorizationService)
        {
            if (id < 1)
                return BadRequest("Invalid student id.");

            try
            {
                Student? student = Student.Find(id);


                if (student == null)
                    return NotFound("Student not found.");

                var authResult = await authorizationService.AuthorizeAsync(User, id, "StudentOwnerOrAdmin");

                if (!authResult.Succeeded)
                    return Forbid();

                return Ok(student);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving student with ID {ID}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        // POST: api/Students
        [HttpPost(Name = "AddStudent")]
        public IActionResult AddStudent([FromBody] Student newStudent)
        {
            if (newStudent == null)
                return BadRequest(new { message = "Student data is null." });

            if (string.IsNullOrWhiteSpace(newStudent.FullName))
                return BadRequest(new { message = "Student name cannot be empty." });

            if (newStudent.Age < 5 || newStudent.Age > 120)
                return BadRequest(new { message = "Invalid age. Age must be between 5 and 120." });

            if (newStudent.Grade < 0 || newStudent.Grade > 100)
                return BadRequest(new { message = "Invalid grade. Grade must be between 0 and 100." });
            if (string.IsNullOrWhiteSpace(newStudent.Email))
                return BadRequest(new { message = "Email cannot be empty." });

            if (string.IsNullOrWhiteSpace(newStudent.PasswordHash))
                return BadRequest(new { message = "PasswordHash cannot be empty." });

            if (string.IsNullOrWhiteSpace(newStudent.Role))
                return BadRequest(new { message = "Role cannot be empty." });

       
            
            try
            {
                Student student = new Student
                {
                    FullName = newStudent.FullName.Trim(),
                    Age = newStudent.Age,
                    Grade = newStudent.Grade,
                    Email = newStudent.Email.Trim(),
                    PasswordHash = newStudent.PasswordHash.Trim(),
                    Role = newStudent.Role.Trim()
                };

                if (student.Save())
                {
                    return CreatedAtRoute("GetStudentById", new { id = student.StudentID }, student);
                }

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Failed to add the student. Database operation did not succeed." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding a new student.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = ex.Message });
            }
        }


        [Authorize(Roles = "Admin")]
        // PUT: api/Students/{id}
        [HttpPut("{id:int}", Name = "UpdateStudent")]
        public IActionResult UpdateStudent(int id, [FromBody] Student updatedStudent)
        {
            if (id <= 0)
                return BadRequest(new { message = $"Invalid ID: {id}." });

            if (updatedStudent == null)
                return BadRequest(new { message = "Student update data is null." });

            if (string.IsNullOrWhiteSpace(updatedStudent.FullName))
                return BadRequest(new { message = "Student name cannot be empty." });

            if (updatedStudent.Age < 5 || updatedStudent.Age > 120)
                return BadRequest(new { message = "Invalid age. Age must be between 5 and 120." });

            if (updatedStudent.Grade < 0 || updatedStudent.Grade > 100)
                return BadRequest(new { message = "Invalid grade. Grade must be between 0 and 100." });
            if (string.IsNullOrWhiteSpace(updatedStudent.Email))
                return BadRequest(new { message = "Email cannot be empty." });

            if (string.IsNullOrWhiteSpace(updatedStudent.PasswordHash))
                return BadRequest(new { message = "PasswordHash cannot be empty." });

            if (string.IsNullOrWhiteSpace(updatedStudent.Role))
                return BadRequest(new { message = "Role cannot be empty." });

           
            try
            {
                Student? student = Student.Find(id);

                if (student == null)
                    return NotFound(new { message = $"Student with ID {id} was not found." });


                student.FullName = updatedStudent.FullName.Trim();
                student.Age = updatedStudent.Age;
                student.Grade = updatedStudent.Grade;
                student.Email = updatedStudent.Email.Trim();
                student.PasswordHash = updatedStudent.PasswordHash.Trim();
                student.Role = updatedStudent.Role.Trim();

                if (student.Save())
                    return Ok(student);

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Failed to update the student record." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating student with ID {ID}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = ex.Message });
            }
        }


        [Authorize(Roles = "Admin")]
        // DELETE: api/Students/{id}
        [HttpDelete("{id:int}", Name = "DeleteStudent")]
        public IActionResult DeleteStudent(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = $"Invalid ID: {id}." });

            try
            {
                Student? student = Student.Find(id);
                if (student == null)
                    return NotFound(new { message = $"Student with ID {id} was not found." });

                if (Student.Delete(id))
                    return Ok(new { message = $"Student with ID {id} has been deleted successfully." });

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Failed to delete student from the database." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting student with ID {ID}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = ex.Message });
            }
        }


    }
}