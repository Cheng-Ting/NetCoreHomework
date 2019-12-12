using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using webapi.Models;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly ContosouniversityContext _context;

        public DepartmentsController(ContosouniversityContext context)
        {
            _context = context;
        }

        // GET: api/Departments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartment()
        {
            return await _context.Department.Where(d => !d.IsDeleted).ToListAsync();
        }

        // GET: api/Departments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
            if (!DepartmentExists(id))
            {
                return NotFound();
            }
            var department = await _context.Department.FindAsync(id);
            return department;
        }

        // PUT: api/Departments/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartment(int id, Department department)
        {
            if (id != department.DepartmentId)
            {
                return BadRequest();
            }
            if (!DepartmentExists(id))
            {
                return NotFound();
            }

            _context.Entry(department).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return CreatedAtAction("GetDepartment", new { id = department.DepartmentId }, department);
        }

        // POST: api/Departments
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Department>> PostDepartment(Department department)
        {
            _context.Department.Add(department);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDepartment", new { id = department.DepartmentId }, department);
        }

        // DELETE: api/Departments/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Department>> DeleteDepartment(int id)
        {
            if (!DepartmentExists(id))
            {
                return NotFound();
            }
            var department = await _context.Department.FindAsync(id);
            // _context.Department.Remove(department);
            department.IsDeleted = true;
            await _context.SaveChangesAsync();

            return department;
        }

        // GET: api/Departments/CourseCount
        [HttpGet("CourseCount")]
        public async Task<ActionResult<IEnumerable<VwDepartmentCourseCount>>> GetCourseStudentCount()
        {
            var vwDepartmentCourseCount = await _context.VwDepartmentCourseCount
                   .FromSqlRaw("SELECT * FROM [dbo].[VwDepartmentCourseCount] Where DepartmentID In (SELECT [DepartmentID] From [Department] Where [IsDeleted] = 0)")
                   .ToListAsync();
            return vwDepartmentCourseCount;
        }

        // GET: api/Departments/CourseCount/1
        [HttpGet("CourseCount/{id}")]
        public async Task<ActionResult<VwDepartmentCourseCount>> GetCourseStudentCount(int id)
        {
            if (!DepartmentExists(id))
            {
                return NotFound();
            }
            var vwDepartmentCourseCount = await _context.VwDepartmentCourseCount
                   .FromSqlRaw("SELECT * FROM [dbo].[VwDepartmentCourseCount] Where DepartmentID = {0}",id)
                   .FirstAsync();
            return vwDepartmentCourseCount;
        }

        //Stored Procedure Insert
        [HttpPost("sp")]
        public async Task<ActionResult<Department>> InsertDepartmentSP(Department department)
        {
            department.DepartmentId = (await _context.Department
                .FromSqlInterpolated($"EXEC [dbo].[Department_Insert] {department.Name}, {department.Budget}, {department.StartDate}, {department.InstructorId};")
                .Select(d => d.DepartmentId).ToListAsync()).First();

            return await _context.Department.FindAsync(department.DepartmentId);

        }

        //Stored Procedure Update
        [HttpPut("sp/{id}")]
        public async Task<ActionResult<Department>> PutDepartmentSP(int id, Department department)
        {
            if (id != department.DepartmentId)
            {
                return BadRequest();
            }
            if (!DepartmentExists(id))
            {
                return NotFound();
            }
            Department oldDepartment = await _context.Department.FindAsync(id);

            await _context.Database
                .ExecuteSqlInterpolatedAsync($"EXEC [dbo].[Department_Update] {id},{department.Name},{department.Budget},{department.StartDate},{department.InstructorId},{oldDepartment.RowVersion}; UPDATE [dbo].[Department] SET [DateModified] = {DateTime.Now} WHERE [DepartmentID] = {id}");

            return await _context.Department.FindAsync(id);
        }

        // Stored Procedure  DELETE
        [HttpDelete("sp/{id}")]
        public async Task<ActionResult<Department>> DeleteDepartmentSP(int id)
        {
            if (!DepartmentExists(id))
            {
                return NotFound();
            }
            var department = await _context.Department.FindAsync(id);
            await _context.Database
                .ExecuteSqlInterpolatedAsync($"EXEC [dbo].[Department_Delete] {id},{department.RowVersion}; UPDATE [dbo].[Department] SET [DateModified] = {DateTime.Now} WHERE [DepartmentID] = {id}");
            
            return department;
        }

        private bool DepartmentExists(int id)
        {
            return _context.Department.Any(e => e.DepartmentId == id && !e.IsDeleted);
        }
    }
}
