using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FootBallOne.Services;
using FootBallOne.Models; // 💡 ADDED: Necessary to resolve "IAcademyEntity could not be found"
using System.Linq;      // 💡 ADDED: Necessary for IQueryable extension methods (Where)

namespace FootBallOne.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IAcademyContext _academyContext;

        // These properties infer the type of AcademyID from IAcademyContext's method signature.
        protected int? CurrentAcademyId => _academyContext.GetCurrentAcademyId();
        protected string CurrentAcademyName => _academyContext.GetCurrentAcademyName();
        protected bool IsSuperAdmin => _academyContext.IsSuperAdmin();

        public BaseController(IAcademyContext academyContext)
        {
            _academyContext = academyContext;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Make academy info available to all views
            ViewBag.CurrentAcademyId = CurrentAcademyId;
            ViewBag.CurrentAcademyName = CurrentAcademyName;
            ViewBag.IsSuperAdmin = IsSuperAdmin;

            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Helper method to filter query by academy.
        /// </summary>
        protected IQueryable<T> FilterByAcademy<T>(IQueryable<T> query) where T : class, IAcademyEntity
        {
            if (IsSuperAdmin)
            {
                return query; // SuperAdmin sees everything
            }

            if (CurrentAcademyId.HasValue)
            {
                // This comparison now works cleanly because T.AcademyID is non-nullable 'int' 
                // and CurrentAcademyId.Value is also non-nullable 'int'.
                return query.Where(e => e.AcademyID == CurrentAcademyId.Value);
            }

            return query.Where(e => false); // Return empty set if no academy context
        }

        /// <summary>
        /// Helper method to set academy ID on new entities.
        /// </summary>
        protected void SetAcademyId<T>(T entity) where T : class, IAcademyEntity
        {
            if (entity != null && CurrentAcademyId.HasValue)
            {
                // Assign the non-nullable int value to the entity's non-nullable int property.
                entity.AcademyID = CurrentAcademyId.Value;
            }
        }
    }
}