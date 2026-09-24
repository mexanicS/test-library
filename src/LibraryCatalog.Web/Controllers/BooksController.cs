using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml;
using LibraryCatalog.Web.Data;
using LibraryCatalog.Web.Models;
using LibraryCatalog.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LibraryCatalog.Web.Controllers;

public sealed class BooksController(IBookRepository repository, ContentsXmlService contentsXml) : Controller
{
    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        search = search?.Trim();
        ViewData["Search"] = search;
        
        return View(await repository.ListAsync(search, cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var book = await repository.GetAsync(id, cancellationToken);
        if (book is null)
        {
            return NotFound();
        }
        
        return View(new BookDetailsViewModel(book, contentsXml.ReadChapters(book.ContentsXml)));
    }

    public IActionResult Create() => View(new BookFormModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookFormModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }
        
        var xml = await BuildXmlOrAddErrorAsync(form, cancellationToken);
        
        if (xml is null)
        {
            return View(form);
        }

        var id = await repository.CreateAsync(ToBook(form, xml), cancellationToken);
        TempData["Message"] = "Книга добавлена.";
        
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var book = await repository.GetAsync(id, cancellationToken);
        if (book is null)
        {
            return NotFound();
        }
        
        return View(new BookFormModel
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            PublishedYear = book.PublishedYear,
            Isbn = book.Isbn,
            Description = book.Description,
            Chapters = contentsXml.ReadChapters(book.ContentsXml).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookFormModel form, CancellationToken cancellationToken)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }
        
        if (!ModelState.IsValid)
        {
            return View(form);
        }
        
        if (await repository.GetAsync(id, cancellationToken) is null)
        {
            return NotFound();
        }

        var xml = await BuildXmlOrAddErrorAsync(form, cancellationToken);
        
        if (xml is null)
        {
            return View(form);
        }

        try
        {
            await repository.UpdateAsync(ToBook(form, xml), cancellationToken);
        }
        catch (SqlException ex) when (ex.Number == 50002)
        {
            return NotFound();
        }

        TempData["Message"] = "Изменения сохранены.";
        
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await repository.DeleteAsync(id, cancellationToken);
        }
        catch (SqlException ex) when (ex.Number == 50002)
        {
            return NotFound();
        }

        TempData["Message"] = "Книга удалена.";
        
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> DownloadContents(int id, CancellationToken cancellationToken)
    {
        var book = await repository.GetAsync(id, cancellationToken);
        if (book is null)
        {
            return NotFound();
        }
        
        var bytes = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + book.ContentsXml);
        
        return File(bytes, "application/xml", $"book-{id}-contents.xml");
    }

    public async Task<IActionResult> Chapters(string? search, CancellationToken cancellationToken)
    {
        search = search?.Trim() ?? "";
        IReadOnlyList<ChapterMatch> results = search.Length == 0
            ? []
            : await repository.FindChaptersAsync(search, cancellationToken);
        return View(new ChapterSearchViewModel(search, results));
    }

    private async Task<string?> BuildXmlOrAddErrorAsync(BookFormModel form, CancellationToken cancellationToken)
    {
        try
        {
            return await contentsXml.BuildAsync(form, cancellationToken);
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(nameof(form.Chapters), ex.Message);
            return null;
        }
        catch (XmlException ex)
        {
            ModelState.AddModelError(nameof(form.ContentsFile), $"Некорректный XML: {ex.Message}");
            return null;
        }
    }

    private static BookRecord ToBook(BookFormModel form, string xml) => new()
    {
        Id = form.Id,
        Title = form.Title.Trim(),
        Author = form.Author.Trim(),
        PublishedYear = form.PublishedYear,
        Isbn = form.Isbn?.Trim(),
        Description = form.Description?.Trim(),
        ContentsXml = xml
    };
}
