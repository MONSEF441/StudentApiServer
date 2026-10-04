using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentsBusinessLogic;
using StudentsDataAccess;

namespace StudentsAPI.Controllers
{
    [ApiController]
    [Route("api/Students")]
    public class StudentsController : ControllerBase
    {
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(ILogger<StudentsController> logger)
        {
            _logger = logger;
        }

        // GET: api/Students/all
        [HttpGet("all", Name = "GetAllStudents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAllStudents()
        {
            try
            {
                List<clsStudentDTO> students = clsStudent.GetAllStudents();

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPassedStudents([FromQuery] decimal grade = 50.0m)
        {
            if (grade < 0 || grade > 100)
                return BadRequest(new { message = "Invalid passing grade value. Must be between 0 and 100." });

            try
            {
                List<clsStudentDTO> students = clsStudent.GetPassedStudents(grade);

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

        // GET: api/Students/{id}
        [HttpGet("{id:int}", Name = "GetStudentById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetStudentById(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = $"Invalid ID: {id}. Student ID must be greater than 0." });

            try
            {
                clsStudent? student = clsStudent.Find(id);

                if (student == null)
                    return NotFound(new { message = $"Student with ID {id} was not found." });

                return Ok(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving student with ID {ID}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = ex.Message });
            }
        }

        // POST: api/Students
        [HttpPost(Name = "AddStudent")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult AddStudent([FromBody] clsStudent newStudent)
        {
            if (newStudent == null)
                return BadRequest(new { message = "Student data is null." });

            if (string.IsNullOrWhiteSpace(newStudent.FullName))
                return BadRequest(new { message = "Student name cannot be empty." });

            if (newStudent.Age < 5 || newStudent.Age > 120)
                return BadRequest(new { message = "Invalid age. Age must be between 5 and 120." });

            if (newStudent.Grade < 0 || newStudent.Grade > 100)
                return BadRequest(new { message = "Invalid grade. Grade must be between 0 and 100." });

            try
            {
                clsStudent student = new clsStudent
                {
                    FullName = newStudent.FullName.Trim(),
                    Age = newStudent.Age,
                    Grade = newStudent.Grade
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

        // PUT: api/Students/{id}
        [HttpPut("{id:int}", Name = "UpdateStudent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateStudent(int id, [FromBody] clsStudent updatedStudent)
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

            try
            {
                clsStudent? student = clsStudent.Find(id);

                if (student == null)
                    return NotFound(new { message = $"Student with ID {id} was not found." });

                student.FullName = updatedStudent.FullName.Trim();
                student.Age = updatedStudent.Age;
                student.Grade = updatedStudent.Grade;

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

        // DELETE: api/Students/{id}
        [HttpDelete("{id:int}", Name = "DeleteStudent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteStudent(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = $"Invalid ID: {id}." });

            try
            {
                clsStudent? student = clsStudent.Find(id);
                if (student == null)
                    return NotFound(new { message = $"Student with ID {id} was not found." });

                if (clsStudent.Delete(id))
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