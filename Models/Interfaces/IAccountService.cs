using System;
using System.Security.Claims;

namespace Notatez.Models.Interfaces;

public interface IAccountService<T>
{
    Task<int> GetSessionAccountIdAsync();
    Task<string> GetSessionUsernameAsync();
    Task<int> HandleAuthenticateAsync(string username, string password, HttpContext httpContext);
    Task HandleLogoutAsync();
}