using System.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Notatez.Models.Interfaces;
using Notatez.Models.Services;

var builder = WebApplication.CreateBuilder(args);


// Add Services 
builder.Services.AddSingleton<NoteService>();
builder.Services.AddSingleton<AccountService>();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

