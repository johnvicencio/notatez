using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notatez.Models.Helpers;
using Notatez.Models.Interfaces;

namespace Notatez.Models.Services
{
    public class NoteService : IDataCrudService<Note>
    {
        private readonly string xmlFilePath;
        private readonly string xmlRoot = "Notes";
        private readonly string xmlItem = "Note";
        private readonly AccountService _accountServices;

        public NoteService(AccountService accountService)
        {
            xmlFilePath = $"wwwroot/data/{xmlRoot.ToLower()}.xml";
            if (!File.Exists(xmlFilePath))
            {
                // create xml file if it doesn't exist
                XDocument xmlDocument = new XDocument(
                    new XDeclaration("1.0", "UTF-8", null),
                    new XElement(xmlRoot)
                );

                xmlDocument.Save(xmlFilePath);
            }

            _accountServices = accountService;
        }

        public async Task<IEnumerable<Note>> GetAllAsync()
        {
            var xmlDocument = await LoadXmlDocumentAsync();

            var notes = xmlDocument?.Root?
                .Elements(xmlItem)
                .Select(noteElement => new Note
                {
                    Id = Convert.ToInt32(noteElement.Element("Id")?.Value),
                    Title = noteElement.Element("Title")?.Value ?? "",
                    Slug = noteElement.Element("Slug")?.Value ?? "",
                    Content = DecodeContent(noteElement.Element("Content")?.Value ?? ""),
                    DateCreated = Convert.ToDateTime(noteElement.Element("DateCreated")?.Value),
                    DateModified = Convert.ToDateTime(noteElement.Element("DateModified")?.Value),
                    AccountId = Convert.ToInt32(noteElement.Element("AccountId")?.Value),
                    Account = new Account
                    {
                        AccountId = Convert.ToInt32(noteElement.Element("AccountId")?.Value),
                        Name = noteElement.Element("Account")?.Element("Name")?.Value ?? ""
                    }
                });

            return notes ?? Enumerable.Empty<Note>();
        }

        public async Task<Note> GetByIdAsync(int id)
        {
            var xmlDocument = await LoadXmlDocumentAsync();

            var noteElement = xmlDocument?.Root?
                .Elements(xmlItem)
                .FirstOrDefault(n => Convert.ToInt32(n.Element("Id")?.Value) == id);

            var note = new Note
            {
                Id = Convert.ToInt32(noteElement?.Element("Id")?.Value),
                Title = noteElement?.Element("Title")?.Value ?? "",
                Slug = noteElement?.Element("Slug")?.Value ?? "",
                Content = DecodeContent(noteElement?.Element("Content")?.Value ?? ""),
                DateCreated = Convert.ToDateTime(noteElement?.Element("DateCreated")?.Value),
                DateModified = Convert.ToDateTime(noteElement?.Element("DateModified")?.Value),
                AccountId = Convert.ToInt32(noteElement?.Element("AccountId")?.Value),
                Account = new Account
                {
                    AccountId = Convert.ToInt32(noteElement?.Element("AccountId")?.Value),
                    Name = noteElement?.Element("Account")?.Element("Name")?.Value ?? ""
                }
            };

            return note;
        }

        public async Task<IEnumerable<Note>> GetByAccountIdAsync(int accountId)
        {
            var xmlDocument = await LoadXmlDocumentAsync();

            var noteElements = xmlDocument?.Root?
                .Elements(xmlItem)
                .Where(n => Convert.ToInt32(n.Element("AccountId")?.Value) == accountId);

            if (noteElements == null || !noteElements.Any())
            {
                return Enumerable.Empty<Note>(); // No notes found for the specified AccountId
            }

            var notes = noteElements.Select(noteElement => new Note
            {
                Id = Convert.ToInt32(noteElement.Element("Id")?.Value),
                Title = noteElement.Element("Title")?.Value ?? "",
                Slug = noteElement.Element("Slug")?.Value ?? "",
                Content = DecodeContent(noteElement.Element("Content")?.Value ?? ""),
                DateCreated = Convert.ToDateTime(noteElement.Element("DateCreated")?.Value),
                DateModified = Convert.ToDateTime(noteElement.Element("DateModified")?.Value),
                AccountId = Convert.ToInt32(noteElement.Element("AccountId")?.Value),
                Account = new Account
                {
                    AccountId = Convert.ToInt32(noteElement.Element("AccountId")?.Value),
                    Name = noteElement.Element("Account")?.Element("Name")?.Value ?? ""
                }
            });

            return notes;
        }

        public async Task<Note> GetBySlugAsync(string slug)
        {
            var xmlDocument = await LoadXmlDocumentAsync();

            var noteElement = xmlDocument?.Root?
                .Elements(xmlItem)
                .FirstOrDefault(n => n.Element("Slug")?.Value == slug);

            var note = new Note
            {
                Id = Convert.ToInt32(noteElement?.Element("Id")?.Value),
                Title = noteElement?.Element("Title")?.Value ?? "",
                Slug = noteElement?.Element("Slug")?.Value ?? "",
                Content = DecodeContent(noteElement?.Element("Content")?.Value ?? ""),
                DateCreated = Convert.ToDateTime(noteElement?.Element("DateCreated")?.Value),
                DateModified = Convert.ToDateTime(noteElement?.Element("DateModified")?.Value),
                AccountId = Convert.ToInt32(noteElement?.Element("AccountId")?.Value),
                Account = new Account
                {
                    AccountId = Convert.ToInt32(noteElement?.Element("AccountId")?.Value),
                    Name = noteElement?.Element("Account")?.Element("Name")?.Value ?? ""
                }
            };

            return note;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<bool> CreateAsync(Note note)
        {
            var xmlDocument = await LoadXmlDocumentAsync();
            if (xmlDocument == null)
            {
                return false;
            }

            var id = GetNextId(xmlDocument);

            var accountId = await _accountServices.GetSessionAccountIdAsync();
            var account = await _accountServices.GetByIdAsync(accountId);

            var accountIdElement = new XElement("AccountId", accountId.ToString());


            var noteElement = new XElement(xmlItem,
                new XElement("Id", id), // Add Id as the first element
                new XElement("Title", note?.Title?.Trim() ?? ""),
                new XElement("Slug", GenerateSlug(note?.Title?.Trim() ?? "")),
                new XElement("Content", EncodeContent(note?.Content?.Trim() ?? "")),
                new XElement("DateCreated", DateTime.Now.ToString()),
                new XElement("DateModified", DateTime.Now.ToString()),
                accountIdElement,
                new XElement("Account",
                    accountIdElement,
                    new XElement("Name", account?.Name?.Trim() ?? "")
                )
            );

            xmlDocument.Root?.Add(noteElement);

            await SaveXmlDocumentAsync(xmlDocument);

            return true;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<bool> UpdateAsync(Note note)
        {
            var xmlDocument = await LoadXmlDocumentAsync();
            if (xmlDocument == null)
            {
                return false;
            }

            var noteElement = xmlDocument.Root?
                .Elements(xmlItem)
                .FirstOrDefault(n => Convert.ToInt32(n.Element("Id")?.Value) == note.Id);

            if (noteElement != null)
            {
                noteElement.Element("Title")?.SetValue(note?.Title?.Trim() ?? "");
                noteElement.Element("Slug")?.SetValue(GenerateSlug(note?.Title?.Trim() ?? ""));
                noteElement.Element("Content")?.SetValue(DecodeContent(note?.Content?.Trim() ?? ""));
                noteElement.Element("DateModified")?.SetValue(DateTime.Now.ToString());
                noteElement.Element("AccountId")?.SetValue(note?.AccountId.ToString() ?? "");
                // Create the Account element
                var accountElement = new XElement("Account",
                    new XElement("AccountId", note?.AccountId.ToString() ?? ""),
                    new XElement("Name", note?.Account?.Name ?? "")
                );
                noteElement.Element("Account")?.ReplaceWith(accountElement);
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

            var noteElement = xmlDocument?.Root?
                .Elements(xmlItem)
                .FirstOrDefault(n => Convert.ToInt32(n.Element("Id")?.Value) == id);

            if (noteElement == null)
            {
                return false;
            }

            noteElement.Remove();

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
            var lastNoteElement = xmlDocument?.Root?
                .Elements(xmlItem)
                .OrderByDescending(n => Convert.ToInt32(n.Element("Id")?.Value))
                .FirstOrDefault();

            var lastId = lastNoteElement != null ? Convert.ToInt32(lastNoteElement.Element("Id")?.Value) : 0;

            return lastId + 1;
        }

        private string GenerateSlug(string title)
        {
            return SlugHelper.GenerateSlug(title);
        }

        private string EncodeContent(string content)
        {
            return DataHelper.EncodeData(content);
        }

        private string DecodeContent(string content)
        {
            return DataHelper.DecodeData(content);
        }
    }
}