using System;
using Microsoft.AspNetCore.Mvc;

namespace Notatez.Models.Helpers;

public static class AlertHelper
{
    public static void SetAlert(Controller controller, string message, string alertType)
    {
        controller.ViewBag.AlertMessage = message;
        controller.ViewBag.AlertType = alertType;
    }
}

