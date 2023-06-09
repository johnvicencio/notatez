using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Notatez.Models.Helpers;
using Notatez.Models.Interfaces;

namespace Notatez.Models.Services;

public class AccountService : IDataCrudService<Account>, IAccountService<Account>
{
    private readonly string xmlFilePath;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string xmlRoot = "Accounts";
    private readonly string xmlItem = "Account";

    public AccountService(IHttpContextAccessor httpContextAccessor)
    {
        xmlFilePath = $"wwwroot/data/{xmlRoot.ToLower()}.xml";
        if (!File.Exists(xmlFilePath))
        {
            XDocument xmlDocument = new XDocument(
           new XDeclaration("1.0", "UTF-8", null),
               new XElement(xmlRoot)
           );

            xmlDocument.Save(xmlFilePath);
        }

        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<Account>> GetAllAsync()
    {
        var xmlDocument = await LoadXmlDocumentAsync();

        var accounts = xmlDocument?.Root?
            .Elements($"{xmlItem}")
            .Select(accountElement => new Account
            {
                AccountId = Convert.ToInt32(accountElement.Element("AccountId")?.Value),
                Name = accountElement.Element("Name")?.Value ?? "",
                Email = accountElement.Element("Email")?.Value ?? "",
                Password = accountElement.Element("Password")?.Value ?? "",
                DateCreated = Convert.ToDateTime(accountElement.Element("DateCreated")?.Value)
                
            });

        return accounts ?? Enumerable.Empty<Account>();
    }

    //public async Task<Account> GetByIdAsync(int id)
    //{
    //    var xmlDocument = await LoadXmlDocumentAsync();

    //    var accountElement = xmlDocument?.Root?
    //        .Elements($"{xmlItem}")
    //        .FirstOrDefault(element => element.Element("AccountId")?.Value == id.ToString());

    //    var account = new Account();
    //    foreach (var property in typeof(Account).GetProperties())
    //    {
    //        var element = accountElement?.Element(property.Name);
    //        if (element != null)
    //        {
    //            var value = Convert.ChangeType(element.Value, property.PropertyType);
    //            property.SetValue(account, value);
    //        }
    //    }

    //    return account;
    //}


    public async Task<Account> GetByIdAsync(int id)
    {
        var xmlDocument = await LoadXmlDocumentAsync();

        var accountElement = xmlDocument?.Root?
            .Elements(xmlItem)
            .FirstOrDefault(n => Convert.ToInt32(n.Element("AccountId")?.Value) == id);

        var account = new Account
        {
            AccountId = Convert.ToInt32(accountElement?.Element("AccountId")?.Value),
            Name = accountElement?.Element("Name")?.Value ?? ""
        };

        return account;
    }

    public async Task<string> GetAccountNameByIdAsync(int id)
    {
        var xmlDocument = await LoadXmlDocumentAsync();

        var accountElement = xmlDocument?.Root?
            .Elements($"{xmlItem}")
            .FirstOrDefault(element => element.Element("AccountId")?.Value == id.ToString());

        if (accountElement != null)
        {
            var accountNameElement = accountElement.Element("Name");
            if (accountNameElement != null)
            {
                return accountNameElement.Value;
            }
        }

        return null!; // Account name not found or account not found
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<bool> CreateAsync(Account account)
    {
        var xmlDocument = await LoadXmlDocumentAsync();
        if (xmlDocument == null)
        {
            return false;
        }

        var accountId = GetNextId(xmlDocument);

        var encryptedPassword = EncryptData(account?.Password?.Trim() ?? "");
        var encodeEmail = EncodeEmail(account?.Email?.Trim() ?? "");
        var name = ExtractNameFromEmail(account?.Email?.Trim() ?? "");

        var accountElement = new XElement($"{xmlItem}",
            new XElement("AccountId", accountId), // Add AccountId as the first element
            account?.GetType().GetProperties()
                .Where(property => property.Name != "AccountId" && property.Name != "Password" && property.Name != "Name" && property.Name != "Email") // Excludes to assign new value
                .Select(property =>
                {
                    var name = property.Name;
                    var value = property.PropertyType == typeof(string)
                        ? property.GetValue(account)?.ToString()?.Trim() ?? ""
                        : property.GetValue(account);
                    return new XElement(name, value);
                }),
            //Add these elements that were excluded
            // Add the encrypted password and email as separate XML elements
            new XElement("Name", name),
            new XElement("Email", encodeEmail),
            new XElement("Password", encryptedPassword) 
        );

        xmlDocument.Root?.Add(accountElement);

        await SaveXmlDocumentAsync(xmlDocument);

        return true;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<bool> UpdateAsync(Account account)
    {
        var xmlDocument = await LoadXmlDocumentAsync();
        if (xmlDocument == null)
        {
            return false;
        }

        var accountElement = xmlDocument.Root?
            .Elements($"{xmlItem}")
            .FirstOrDefault(n => Convert.ToInt32(n.Element("AccountId")?.Value) == account.AccountId);

        if (accountElement != null)
        {
            accountElement.Element("Email")?.SetValue(account.Email ?? "");
            accountElement.Element("Password")?.SetValue(EncryptData(account.Password ?? ""));

            var dateModifiedElement = accountElement.Element("DateModified");
            if (dateModifiedElement != null)
            {
                dateModifiedElement.SetValue(DateTime.Now.ToString());
            }
            else
            {
                accountElement.Add(new XElement("DateModified", DateTime.Now.ToString()));
            }

            // Update Roles
            var rolesElement = accountElement.Element("Roles");
            if (rolesElement != null)
            {
                rolesElement.Remove(); // Remove existing Roles element

                // Add updated Roles
                //accountElement.Add(new XElement("Roles", account.Roles.Select(role => new XElement("Role", role))));
            }

            await SaveXmlDocumentAsync(xmlDocument);

            return true;
        }

        return false;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var xmlDocument = await LoadXmlDocumentAsync();
        if (xmlDocument == null)
        {
            return false;
        }

        var AccountElement = xmlDocument?.Root?
            .Elements($"{xmlItem}")
            .FirstOrDefault(n => Convert.ToInt32(n.Element("Id")?.Value) == id);

        if (AccountElement == null)
        {
            return false;
        }

        AccountElement.Remove();

        if (xmlDocument != null)
        {
            await SaveXmlDocumentAsync(xmlDocument);
        }

        return true;
    }

    private async Task<XDocument> LoadXmlDocumentAsync()
    {
        using var stream = new FileStream(xmlFilePath, FileMode.OpenOrCreate);
        return await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
    }

    private async Task SaveXmlDocumentAsync(XDocument xmlDocument)
    {
        using var stream = new FileStream(xmlFilePath, FileMode.Truncate);
        await xmlDocument.SaveAsync(stream, SaveOptions.None, CancellationToken.None);
    }

    private int GetNextId(XDocument xmlDocument)
    {
        var accountElements = xmlDocument?.Root?.Elements("Account");
        if (accountElements == null || !accountElements.Any())
        {
            return 1; // Start with ID 1 if no existing accounts are found
        }

        var lastId = accountElements
            .Select(accountElement => Convert.ToInt32(accountElement.Element("AccountId")?.Value))
            .Max();

        return lastId + 1;
    }


    private void InitializeXmlFile()
    {
        XDocument xmlDocument = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(xmlRoot)
        );

        xmlDocument.Save(xmlFilePath);
    }

    private IEnumerable<Account> DeserializeNotes(XDocument xmlDocument)
    {
        if (xmlDocument?.Root == null)
        {
            return Enumerable.Empty<Account>();
        }

        var accounts = xmlDocument.Root
            .Elements(xmlItem)
            .Select(accountElement => DeserializeNoteElement(accountElement));

        return accounts;
    }


    private Account DeserializeNoteElement(XElement accountElement)
    {
        return new Account
        {
            AccountId = Convert.ToInt32(accountElement.Element("AccountId")?.Value),
            Name = accountElement.Element("Name")?.Value ?? "",
            Email = accountElement.Element("Email")?.Value ?? "",
            Password = accountElement.Element("Password")?.Value ?? "",
            DateCreated = Convert.ToDateTime(accountElement.Element("DateCreated")?.Value)
        };
    }

    private XElement SerializeNoteToElement(Account account)
    {
        return new XElement(xmlItem,
            new XElement("Accountid", account.AccountId),
            new XElement("Name", account?.Name),
            new XElement("Email", account?.Email),
            new XElement("Password", account?.Password),
            new XElement("DateCreated", account?.DateCreated.ToString())
        );
    }

    public async Task<int> HandleAuthenticateAsync(string email, string password, HttpContext httpContext)
    {
        var accounts = await GetAllAsync();

        var authenticatedAccount = accounts.FirstOrDefault(a => a.Email != null &&
         a.Email == email && a.Password != null && MatchEncryptedData(password.Trim(), a.Password));

        if (authenticatedAccount != null)
        {
            httpContext.Session.SetInt32("AccountId", authenticatedAccount.AccountId);
            httpContext.Session.SetString("Name", authenticatedAccount.Name ?? "");
            if (email != null)
            {
                httpContext.Session.SetString("Email", email);
            }
            return authenticatedAccount.AccountId;
        }

        return 0;
    }

    public Task<int> GetSessionAccountIdAsync()
    {
        int result = 0;

        if (_httpContextAccessor?.HttpContext?.Session != null)
        {
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            if (accountId != null)
            {
                result = accountId.Value;
            }
        }

        return Task.FromResult(result);
    }

    public Task<string> GetSessionUsernameAsync()
    {
        string result = string.Empty;

        if (_httpContextAccessor?.HttpContext?.Session != null)
        {
            var name = _httpContextAccessor.HttpContext.Session.GetString("Name");
            if (!string.IsNullOrWhiteSpace(name))
            {
                result = name;
            }
        }

        return Task.FromResult(result);
    }

    public int GetSessionAccountId()
    {
        int result = 0;

        if (_httpContextAccessor?.HttpContext?.Session != null)
        {
            var accountId = _httpContextAccessor.HttpContext.Session.GetInt32("AccountId");
            if (accountId != null)
            {
                result = accountId.Value;
            }
        }

        return result;
    }

    public string GetSessionUsername()
    {
        string result = string.Empty;

        if (_httpContextAccessor?.HttpContext?.Session != null)
        {
            var name = _httpContextAccessor.HttpContext.Session.GetString("Name");
            if (!string.IsNullOrWhiteSpace(name))
            {
                result = name;
            }
        }

        return result;
    }


    public async Task HandleLogoutAsync()
    {
        if (_httpContextAccessor?.HttpContext?.Session != null)
        {
            // Get all session keys
            var sessionKeys = _httpContextAccessor.HttpContext.Session.Keys.ToList();
            // Remove all session variables
            foreach (var key in sessionKeys)
            {
                _httpContextAccessor.HttpContext.Session.Remove(key);
            }
        }

        await Task.CompletedTask;
    }

    private string EncryptData(string data)
    {
        return EncryptionHelper.EncryptData(data);
    }

    private bool MatchEncryptedData(string inputData, string encryptedData)
    {
        return EncryptionHelper.MatchData(inputData, encryptedData);
    }

    private string ExtractNameFromEmail(string data)
    {
        return DataHelper.GetUsernameFromEmail(data);
    }

    public Task<Account> GetBySlugAsync(string slug)
    {
        throw new NotImplementedException();
    }

    private string EncodeEmail(string content)
    {
        return DataHelper.EncodeData(content);
    }

    private string DecodeEmail(string content)
    {
        return DataHelper.DecodeData(content);
    }
}