using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Notatez.Models.Helpers;

namespace Notatez.Models.Attributes;

public class AuthorizationNeeded : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var accountId = context.HttpContext.Session.GetInt32("AccountId");

        if (accountId == null && !IsAuthorizationNotNeeded(context))
        {
            // Redirect to Login action of Account controller
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        // Continue with the action execution
        base.OnActionExecuting(context);
    }

    private static bool IsAuthorizationNotNeeded(ActionExecutingContext context)
    {
        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

        if (actionDescriptor != null)
        {
            return actionDescriptor.MethodInfo.GetCustomAttributes(inherit: true)
                .Any(attr => attr.GetType() == typeof(AuthorizationNotNeeded));
        }

        return false;
    }
}
