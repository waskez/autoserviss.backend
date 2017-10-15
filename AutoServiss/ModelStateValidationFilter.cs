using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace AutoServiss
{
    public class ModelStateValidationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Keys
                    .SelectMany(key => context.ModelState[key].Errors.Select(x => x.ErrorMessage))
                    .ToList();
                context.Result = new BadRequestObjectResult(new { messages = errors });
            }

            base.OnActionExecuting(context);
        }
    }
}