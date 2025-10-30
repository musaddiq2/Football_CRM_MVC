using FootBallOne.Data;
using FootBallOne.Models;
using FootBallOne.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootBallOne.Controllers
{
    public class TrainingList : BaseController
    {
        private readonly ApplicationDbContext _context;
        public TrainingList(ApplicationDbContext context, IAcademyContext academyContext)
        : base(academyContext)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var trainings = await FilterByAcademy(_context.Trainings)
            .Include(t => t.CourseSchedules)
            .Select(t => new TrainingListview
            {
                    TrainingId = t.TrainingId,
                    ActivityImage = t.ActivityImage,
                    ActivityName = t.ActivityName,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    ActivityType = t.ActivityType,
                    MaximumCapacity = t.MaximumCapacity,
                    TotalCourseCost = t.TotalCourseCost,
                    Status = t.IsActive ? "Active" : "Inactive",
                    Trainers = t.Trainers,
                    Facilities = t.Facilities,
                    CourseSchedules = t.CourseSchedules.Select(cs => new CourseScheduleViewModel
                    {
                        CourseScheduleId = cs.CourseScheduleId,
                        Day = cs.Day,
                        StartTime = cs.StartTime
                    }).ToList()
                }).ToListAsync();

            foreach (var training in trainings)
            {
                training.SetDurationHours(training.StartDate, training.EndDate);
            }

            return View(trainings);
        }

        // GET: TrainingList/EditTraining/{id}
        public async Task<IActionResult> EditTraining(int id)
        {
            var training = await _context.Trainings
                .Include(t => t.CourseSchedules)
                .FirstOrDefaultAsync(t => t.TrainingId == id);

            if (training == null)
            {
                return NotFound();
            }
            if (!IsSuperAdmin && training.AcademyID != CurrentAcademyId)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this training.";
                return RedirectToAction(nameof(Index));
            }

            // Map entity to ViewModel
            var viewModel = new TrainingManagementViewModel
            {
                TrainingId = training.TrainingId,
                ActivityImage = training.ActivityImage,
                ActivityName = training.ActivityName,
                ActivityType = training.ActivityType,
                Description = training.Description,
                TrainingLevel = training.TrainingLevel,
                AgeGroup = training.AgeGroup,
                Branch = training.Branch,
                Gender = training.Gender,
                Facilities = training.Facilities,
                StartDate = training.StartDate,
                EndDate = training.EndDate,
                MaximumCapacity = training.MaximumCapacity,
                MinimumSubscribers = training.MinimumSubscribers,
                CostType = training.CostType,
                TotalCourseCost = training.TotalCourseCost,
                ProfitMargin = training.ProfitMargin,
                TermsConditions = training.TermsConditions,
                Trainers = training.Trainers,
                CourseSchedules = training.CourseSchedules?.Select(cs => new CourseScheduleViewModel
                {
                    CourseScheduleId = cs.CourseScheduleId,
                    Day = cs.Day,
                    StartTime = cs.StartTime
                }).ToList() ?? new List<CourseScheduleViewModel>(),
                IsActive = training.IsActive,
                Status = training.IsActive ? "Active" : "Inactive"
            };

            return View(viewModel);
        }

        // POST: TrainingList/EditTraining/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTraining(int id, TrainingManagementViewModel viewModel)
        {
            if (id != viewModel.TrainingId)
            {
                return NotFound();
            }

            try
            {
                var training = await _context.Trainings
                    .Include(t => t.CourseSchedules)
                    .FirstOrDefaultAsync(t => t.TrainingId == id);

                if (training == null)
                {
                    return NotFound();
                }

                // Map ViewModel back to entity
                training.ActivityName = viewModel.ActivityName;
                training.ActivityType = viewModel.ActivityType;
                training.Description = viewModel.Description;
                training.TrainingLevel = viewModel.TrainingLevel;
                training.AgeGroup = viewModel.AgeGroup;
                training.Branch = viewModel.Branch;
                training.Gender = viewModel.Gender;
                training.Facilities = viewModel.Facilities;
                training.StartDate = viewModel.StartDate;
                training.EndDate = viewModel.EndDate;
                training.MaximumCapacity = viewModel.MaximumCapacity;
                training.MinimumSubscribers = viewModel.MinimumSubscribers;
                training.CostType = viewModel.CostType;
                training.TotalCourseCost = viewModel.TotalCourseCost;
                training.ProfitMargin = viewModel.ProfitMargin;
                training.TermsConditions = viewModel.TermsConditions;
                training.Trainers = viewModel.Trainers;
                training.IsActive = viewModel.IsActive;

                // Handle ActivityImage if uploaded
                if (!string.IsNullOrEmpty(viewModel.ActivityImage))
                {
                    training.ActivityImage = viewModel.ActivityImage;
                }

                // Update course schedules
                if (training.CourseSchedules != null && training.CourseSchedules.Any())
                {
                    _context.CourseSchedules.RemoveRange(training.CourseSchedules);
                }

                if (viewModel.CourseSchedules != null && viewModel.CourseSchedules.Any())
                {
                    foreach (var schedule in viewModel.CourseSchedules)
                    {
                        training.CourseSchedules.Add(new CourseSchedule
                        {
                            Day = schedule.Day,
                            StartTime = schedule.StartTime,
                            TrainingId = training.TrainingId
                        });
                    }
                }

                _context.Update(training);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency issues
                TempData["ErrorMessage"] = "The record was modified by another user. Please refresh and try again.";
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log exception 
                TempData["ErrorMessage"] = "An error occurred while updating the training. Please try again.";
                return View(viewModel);
            }
        }

        public async Task<IActionResult> TrainingDetails(int id)
        {
            var training = await _context.Trainings
                .Include(t => t.CourseSchedules)
                .FirstOrDefaultAsync(t => t.TrainingId == id);

            if (training == null)
                return NotFound();

            // Map to ViewModel
            var viewModel = new TrainingManagementViewModel
            {
                TrainingId = training.TrainingId,
                ActivityImage = training.ActivityImage,
                ActivityName = training.ActivityName,
                ActivityType = training.ActivityType,
                Description = training.Description,
                TrainingLevel = training.TrainingLevel,
                AgeGroup = training.AgeGroup,
                Branch = training.Branch,
                Gender = training.Gender,
                Facilities = training.Facilities,
                StartDate = training.StartDate,
                EndDate = training.EndDate,
                MaximumCapacity = training.MaximumCapacity,
                MinimumSubscribers = training.MinimumSubscribers,
                CostType = training.CostType,
                TotalCourseCost = training.TotalCourseCost,
                ProfitMargin = training.ProfitMargin,
                TermsConditions = training.TermsConditions,
                Trainers = training.Trainers,
                CourseSchedules = training.CourseSchedules.Select(cs => new CourseScheduleViewModel
                {
                    CourseScheduleId = cs.CourseScheduleId,
                    Day = cs.Day,
                    StartTime = cs.StartTime
                }).ToList(),
                IsActive = training.IsActive,
                Status = training.IsActive ? "Active" : "Inactive"
            };

            return View(viewModel);
        }

        // GET: TrainingList/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var training = await _context.Trainings
                .Include(t => t.CourseSchedules) // Include schedules for display
                .FirstOrDefaultAsync(t => t.TrainingId == id);

            if (training == null)
            {
                return NotFound();
            }

            // Map to ViewModel (optional)
            var viewModel = new TrainingManagementViewModel
            {
                TrainingId = training.TrainingId,
                ActivityName = training.ActivityName,
                ActivityType = training.ActivityType,
                StartDate = training.StartDate,
                EndDate = training.EndDate,
                CourseSchedules = training.CourseSchedules.Select(cs => new CourseScheduleViewModel
                {
                    CourseScheduleId = cs.CourseScheduleId,
                    Day = cs.Day,
                    StartTime = cs.StartTime
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: TrainingList/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var training = await _context.Trainings
                .Include(t => t.CourseSchedules) // Include schedules to delete
                .FirstOrDefaultAsync(t => t.TrainingId == id);

            if (training == null)
            {
                return NotFound();
            }

            // Remove schedules first if cascade delete not enabled
            if (training.CourseSchedules != null && training.CourseSchedules.Any())
            {
                _context.CourseSchedules.RemoveRange(training.CourseSchedules);
            }

            // Remove the training
            _context.Trainings.Remove(training);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Training deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
