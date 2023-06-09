﻿using System.Diagnostics;
using Notatez.Models.Services;
using SendGrid.Extensions.DependencyInjection;
using SendGrid.Helpers.Mail;

var builder = WebApplication.CreateBuilder(args);


// Add Services 
builder.Services.AddSingleton<NoteService>();
builder.Services.AddSingleton<AccountService>();

//Email configuration
builder.Services.AddSendGrid(options =>
{
    options.ApiKey = builder.Configuration
    .GetSection("EmailSettings").GetValue<string>("APIKey");
});

// Add configuration to lower case urls
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Session
builder.Services.AddSession();
builder.Services.AddMemoryCache();

// Add context accessor
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Get all running processes
    Process[] processes = Process.GetProcesses();

    // Filter dotnet processes
    var dotnetProcesses = Array.FindAll(processes, p => p.ProcessName.StartsWith("dotnet", StringComparison.OrdinalIgnoreCase));

    // Print the process IDs
    foreach (var process in dotnetProcesses)
    {
        Console.WriteLine($"Process Name: {process.ProcessName}, PID: {process.Id}");
    }

    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseExceptionHandler("/Error");

app.UseHttpsRedirection();
app.UseStaticFiles();
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(
//           Path.Combine(builder.Environment.ContentRootPath, "App_Data")),
//    RequestPath = "/App_Data"
//});

app.UseRouting();

// Use Session
app.UseSession();

app.UseAuthorization();

// Create a friendly URL
//app.MapControllerRoute(
//    name: "note",
//    pattern: "note/{**slug}",
//    defaults: new { controller = "Note", action = "Details" });
//app.MapControllerRoute(
//    name: "noteDelete",
//    pattern: "note/delete/{id?}",
//    defaults: new { controller = "Note", action = "Delete" }
//);

//app.MapControllerRoute(
//    name: "noteUpdate",
//    pattern: "note/update/{id?}",
//    defaults: new { controller = "Note", action = "Update" }
//);
//app.MapControllerRoute(
//    name: "noteIndex",
//    pattern: "note",
//    defaults: new { controller = "Note", action = "Index" }
//);

// Original
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();


app.Run();

