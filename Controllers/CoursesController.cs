﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webapi.Models;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ContosouniversityContext _context;

        public CoursesController(ContosouniversityContext context)
        {
            _context = context;
        }

        // GET: api/Courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetCourse()
        {
            return await _context.Course.Where(x => !x.IsDeleted).ToListAsync();
        }

        // GET: api/Courses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(int id)
        {
            var course = await _context.Course.Where(c => !c.IsDeleted && c.CourseId == id).FirstOrDefaultAsync(c=> c.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }

            return course;
        }

        // PUT: api/Courses/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(int id, Course course)
        {
            if (id != course.CourseId)
            {
                return BadRequest();
            }
            if (!CourseExists(id))
            {
                return NotFound();
            }

            _context.Entry(course).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }

        // POST: api/Courses
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Course>> PostCourse(Course course)
        {
            _context.Course.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCourse", new { id = course.CourseId }, course);
        }

        // DELETE: api/Courses/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Course>> DeleteCourse(int id)
        {
            if (!CourseExists(id))
            {
                return NotFound();
            }
            
            var course = await _context.Course.FindAsync(id);
            
            //_context.Course.Remove(course);
            course.IsDeleted = true;
            await _context.SaveChangesAsync();

            return course;
        }

        // GET: api/Courses/Students
        [HttpGet("Students")]
        public async Task<ActionResult<IEnumerable<VwCourseStudents>>> GetCourseStudents()
        {
            return await _context.VwCourseStudents.ToListAsync();
        } 
        
        // GET: api/Courses/Students/1
        [HttpGet("Students/{id}")]
        public async Task<ActionResult<IEnumerable<VwCourseStudents>>> GetCourseIdStudents(int id)
        {
            return await _context.VwCourseStudents.Where(e => e.CourseId == id).ToListAsync();
        }

        // GET: api/Courses/StudentsCount
        [HttpGet("StudentsCount")]
        public async Task<ActionResult<IEnumerable<VwCourseStudentCount>>> GetCourseStudentCount()
        {
            return await _context.VwCourseStudentCount.ToListAsync();
        }

        // GET: api/Courses/StudentsCount/1
        [HttpGet("StudentsCount/{id}")]
        public async Task<ActionResult<IEnumerable<VwCourseStudentCount>>> GetCourseStudentCount(int id)
        {
            if (!CourseExists(id))
            {
                return NotFound();
            }
            else
            {
                return await _context.VwCourseStudentCount.Where(e => e.CourseId == id).ToListAsync();
            }
        }

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.CourseId == id && !e.IsDeleted);
        }
    }
}
