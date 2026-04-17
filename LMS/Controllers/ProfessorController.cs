using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var students =
                from course in db.Courses
                join classOffering in db.Classes on course.CatalogId equals classOffering.Listing
                join enrolled in db.Enrolleds on classOffering.ClassId equals enrolled.Class
                join student in db.Students on enrolled.Student equals student.UId
                where course.Department == subject
                      && course.Number == (uint)num
                      && classOffering.Season == season
                      && classOffering.Year == (uint)year
                orderby student.LName, student.FName
                select new
                {
                    fname = student.FName,
                    lname = student.LName,
                    uid = student.UId,
                    dob = student.Dob.ToString(),
                    grade = enrolled.Grade
                };

            return Json(students.ToList());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            var query =
                from course in db.Courses
                join classOffering in db.Classes on course.CatalogId equals classOffering.Listing
                join cat in db.AssignmentCategories on classOffering.ClassId equals cat.InClass
                join a in db.Assignments on cat.CategoryId equals a.Category
                where course.Department == subject
                      && course.Number == (uint)num
                      && classOffering.Season == season
                      && classOffering.Year == (uint)year
                select new
                {
                    aname = a.Name,
                    cname = cat.Name,
                    due = a.Due,
                    submissions = db.Submissions.Count(s => s.Assignment == a.AssignmentId)
                };

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(x => x.cname == category);
            }

            return Json(query.ToList());
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var cats =
                from course in db.Courses
                join classOffering in db.Classes on course.CatalogId equals classOffering.Listing
                join cat in db.AssignmentCategories on classOffering.ClassId equals cat.InClass
                where course.Department == subject
                      && course.Number == (uint)num
                      && classOffering.Season == season
                      && classOffering.Year == (uint)year
                orderby cat.Name
                select new
                {
                    name = cat.Name,
                    weight = cat.Weight
                };

            return Json(cats.ToList());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            // find the class
            var cls =
                (from course in db.Courses
                 join classOffering in db.Classes on course.CatalogId equals classOffering.Listing
                 where course.Department == subject
                       && course.Number == (uint)num
                       && classOffering.Season == season
                       && classOffering.Year == (uint)year
                 select classOffering)
                .FirstOrDefault();

            if (cls == null)
            {
                return Json(new { success = false });
            }

            // check duplicate
            bool exists = db.AssignmentCategories.Any(ac => ac.InClass == cls.ClassId && ac.Name == category);
            if (exists)
            {
                return Json(new { success = false });
            }

            uint nextId = db.AssignmentCategories.Any() ? db.AssignmentCategories.Max(ac => ac.CategoryId) + 1u : 1u;

            var newCat = new AssignmentCategory
            {
                CategoryId = nextId,
                Name = category,
                Weight = (uint)catweight,
                InClass = cls.ClassId
            };

            db.AssignmentCategories.Add(newCat);
            db.SaveChanges();

            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            // find the class and category
            var query =
                 from course in db.Courses
                 join classOffering in db.Classes on course.CatalogId equals classOffering.Listing
                 join ac in db.AssignmentCategories on classOffering.ClassId equals ac.InClass
                 where course.Department == subject
                       && course.Number == (uint)num
                       && classOffering.Season == season
                       && classOffering.Year == (uint)year
                       && ac.Name == category
                 select ac;

            var cat = query.FirstOrDefault();

            if (cat == null)
            {
                return Json(new { success = false });
            }

            // check duplicate assignment name within category
            if (db.Assignments.Any(
                a => a.Category == cat.CategoryId 
                && a.Name == asgname))
            {
                return Json(new { success = false });
            }

            uint nextId = db.Assignments.Any() ? db.Assignments.Max(a => a.AssignmentId) + 1u : 1u;

            var newAsg = new Assignment
            {
                AssignmentId = nextId,
                Name = asgname,
                Contents = asgcontents,
                Due = asgdue,
                MaxPoints = (uint)asgpoints,
                Category = cat.CategoryId
            };

            // Save new assignment
            db.Assignments.Add(newAsg);
            db.SaveChanges();

            // After creating a new assignment, recalculate everyone's grade
            UpdateAllGradesInClass(cat.InClass);
            db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var subs =
                from course in db.Courses
                join classOffering in db.Classes on course.CatalogId equals classOffering.Listing
                join ac in db.AssignmentCategories on classOffering.ClassId equals ac.InClass
                join a in db.Assignments on ac.CategoryId equals a.Category
                join s in db.Submissions on a.AssignmentId equals s.Assignment
                join student in db.Students on s.Student equals student.UId
                where course.Department == subject
                      && course.Number == (uint)num
                      && classOffering.Season == season
                      && classOffering.Year == (uint)year
                      && ac.Name == category
                      && a.Name == asgname
                orderby s.Time descending
                select new
                {
                    fname = student.FName,
                    lname = student.LName,
                    uid = student.UId,
                    time = s.Time,
                    score = s.Score
                };

            return Json(subs.ToList());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var query = from course in db.Courses
                        join classOffering in db.Classes on course.CatalogId equals classOffering.Listing
                        join ac in db.AssignmentCategories on classOffering.ClassId equals ac.InClass
                        join a in db.Assignments on ac.CategoryId equals a.Category
                        join s in db.Submissions on a.AssignmentId equals s.Assignment
                        where course.Department == subject
                              && course.Number == (uint)num
                              && classOffering.Season == season
                              && classOffering.Year == (uint)year
                              && ac.Name == category
                              && a.Name == asgname
                              && s.Student == uid
                        select new { Submission = s, ClassId = classOffering.ClassId };

            var result = query.FirstOrDefault();

            if (result == null)
            {
                return Json(new { success = false });
            }

            // Update the score
            result.Submission.Score = (uint)score;

            // Recalculate this specific student's grade
            UpdateGrade(uid, result.ClassId);

            db.SaveChanges();
            return Json(new { success = true });
        }

        /// <summary>
        /// Update a students letter grade based on weights and category
        /// </summary>
        private void UpdateGrade(string uid, uint classId)
        {
            // get all categories for the class
            var categories = db.AssignmentCategories
                .Where(ac => ac.InClass == classId)
                .ToList();

            double totalWeightedScore = 0.0;
            double totalActiveWeight = 0.0;

            foreach (var category in categories)
            {
                var query = from a in db.Assignments
                            where a.Category == category.CategoryId
                            join s in db.Submissions
                            on new { A = a.AssignmentId, B = uid } equals new { A = s.Assignment, B = s.Student }
                            into joined
                            from sub in joined.DefaultIfEmpty()
                            select new
                            {
                                MaxPoints = a.MaxPoints,
                                Score = sub == null ? 0 : sub.Score // score is 0 if no submission
                            };
                var assignments = query.ToList();

                // ignore categories with no assignments
                if (!assignments.Any())
                    continue;

                double maxPointsInCategory = assignments.Sum(a => (double)a.MaxPoints);
                double totalPointsEarned = 0.0;

                // percentage for the category (earned / max)
                double categoryPercentage = maxPointsInCategory > 0 ? totalPointsEarned / maxPointsInCategory : 0;

                // multiply by category weight
                totalWeightedScore += categoryPercentage * category.Weight;
                totalActiveWeight += category.Weight;
            }

            // Compute the scaling factor to make all category weights add up to 100%.
            // This scaling factor is 100 / (sum of all category weights).
            double finalPercentage = 0.0;
            if (totalActiveWeight > 0)
            {
                double scalingFactor = 100.0 / totalActiveWeight;
                finalPercentage = totalWeightedScore * scalingFactor;
            }

            // convert percentage to letter grade
            string letterGrade = PercentToLetter(finalPercentage);

            // update letter grade in DB
            var enrollment = db.Enrolleds.FirstOrDefault(e => e.Class == classId && e.Student == uid);
            if (enrollment != null)
            {
                enrollment.Grade = letterGrade;
            }
        }

        /// <summary>
        /// Convert a numeric percentage to a letter grade based on standard syllabus for the U of U.
        /// </summary>
        private string PercentToLetter(double percentage)
        {
            if (percentage >= 93) return "A";
            if (percentage >= 90) return "A-";
            if (percentage >= 87) return "B+";
            if (percentage >= 83) return "B";
            if (percentage >= 80) return "B-";
            if (percentage >= 77) return "C+";
            if (percentage >= 73) return "C";
            if (percentage >= 70) return "C-";
            if (percentage >= 67) return "D+";
            if (percentage >= 63) return "D";
            if (percentage >= 60) return "D-";
            return "E";
        }

        /// <summary>
        /// Helper to update grades for every student enrolled in a class.
        /// </summary>
        private void UpdateAllGradesInClass(uint classId)
        {
            var students = db.Enrolleds.Where(e => e.Class == classId).Select(e => e.Student).ToList();
            foreach (var studentUid in students)
            {
                UpdateGrade(studentUid, classId);
            }
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var classes =
                from c in db.Classes
                join course in db.Courses on c.Listing equals course.CatalogId
                where c.TaughtBy == uid
                orderby c.Year descending, c.Season
                select new
                {
                    subject = course.Department,
                    number = course.Number,
                    name = course.Name,
                    season = c.Season,
                    year = c.Year
                };

            return Json(classes.ToList());
        }



        /*******End code to modify********/
    }
}