using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            bool duplictate = false;
            var query = from d in db.Departments
                        where d.Subject == subject
                        select d;

            duplictate = query.Count() >= 1;

            if(!duplictate)
            {
                var newDepart = new Department
                {
                    Name = name,
                    Subject = subject
                };

                db.Departments.Add(newDepart);

                db.SaveChanges();
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var result = db.Courses
                .Where(c => c.Department == subject)
                .OrderBy(c => c.Number)
                .Select(c => new
                {
                    number = c.Number,
                    name = c.Name
                })
                .ToList();

            return Json(result);
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var result = db.Professors
                .Where(p => p.WorksIn == subject)
                .OrderBy(p => p.LName)
                .ThenBy(p => p.FName)
                .Select(p => new
                {
                    lname = p.LName,
                    fname = p.FName,
                    uid = p.UId
                })
                .ToList();

            return Json(result);
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            // Check for existing course with same department + number
            if (db.Courses.Any(c => c.Department == subject && c.Number == (uint)number))
            {
                return Json(new { success = false });
            }

            // Determine next CatalogId (CatalogId is the primary key)
            uint nextCatalogId = db.Courses.Any() ? db.Courses.Max(c => c.CatalogId) + 1u : 1u;

            var newCourse = new Course
            {
                CatalogId = nextCatalogId,
                Number = (uint)number,
                Name = name,
                Department = subject
            };

            db.Courses.Add(newCourse);
            db.SaveChanges();

            return Json(new { success = true });
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            var course = db.Courses
                .FirstOrDefault(c => c.Department == subject && c.Number == (uint)number);

            if (course == null)
            {
                return Json(new { success = false });
            }

            uint listing = course.CatalogId;
            uint uYear = (uint)year;


            bool classExists = db.Classes.Any(
            c => c.Listing == listing 
            && c.Season == season 
            && c.Year == uYear);

            if (classExists)
            {
                return Json(new { success = false });
            }

            // Convert times to TimeOnly for comparison with stored times
            var newStart = TimeOnly.FromDateTime(start);
            var newEnd = TimeOnly.FromDateTime(end);

            // Check for location/time conflicts
            bool locationConflict = db.Classes
                .Where(c => c.Season == season && c.Year == uYear && c.Location == location)
                .Any(c => (newStart < c.EndTime) && (c.StartTime < newEnd));

            if (locationConflict)
            {
                return Json(new { success = false });
            }

            // Determine next ClassId
            uint nextClassId = db.Classes.Any() ? db.Classes.Max(c => c.ClassId) + 1u : 1u;

            var newClass = new Class
            {
                ClassId = nextClassId,
                Season = season,
                Year = uYear,
                Location = location,
                StartTime = newStart,
                EndTime = newEnd,
                Listing = listing,
                TaughtBy = instructor
            };

            db.Classes.Add(newClass);
            db.SaveChanges();

            return Json(new { success = true});
        }


        /*******End code to modify********/

    }
}

