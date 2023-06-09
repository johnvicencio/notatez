using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Notatez.Models.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Notatez.Controllers;

public class ErrorController : Controller
{
    [HttpGet("/Error/{statusCode}")]
    public IActionResult HandleErrorCode(int statusCode)
    {
        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var errorMessage = string.Empty;


        var errorModel = new ErrorViewModel();

        switch (statusCode)
        {
            case 404:
                errorMessage = "Page not found...";
                break;
            case 500:
                errorMessage = "Internal server error...";
                break;
            case 400:
                errorMessage = "Bad requests...";
                break;
            case 403:
                errorMessage = "Forbiden error...";
                break;
            case 401:
                errorMessage = "Unauthorized...";
                break;
            default:
                errorMessage = exceptionHandlerPathFeature?.Error.Message;
                break;
        }

        errorModel.StatusCode = statusCode;
        errorModel.ErrorMessage = errorMessage;

        return View("~/Views/Shared/Error.cshtml", errorModel);
    }
}

