using System.ComponentModel.DataAnnotations;
using System.Xml;
using System.Xml.Linq;
using Ganss.Xss;
using LibraryCatalog.Web.Models;

namespace LibraryCatalog.Web.Services;

public sealed class ContentsXmlService
{
    private const int MaxChapters = 100;
    private const int MaxBodyLength = 10_000;
    private const long MaxXmlBytes = 1_000_000;
    private readonly HtmlSanitizer _sanitizer;

    public ContentsXmlService()
    {
        _sanitizer = new HtmlSanitizer();
        _sanitizer.AllowedTags.Clear();
        foreach (var tag in new[] { "p", "br", "strong", "b", "em", "i", "u", "ul", "ol", "li" })
            _sanitizer.AllowedTags.Add(tag);
        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedCssProperties.Clear();
        _sanitizer.AllowedSchemes.Clear();
    }

    public async Task<string> BuildAsync(BookFormModel form, CancellationToken cancellationToken)
    {
        IReadOnlyList<ChapterInput> chapters = form.Chapters;

        if (form.ContentsFile is { Length: > 0 } file)
        {
            if (file.Length > MaxXmlBytes)
                throw new ValidationException("XML-файл должен быть меньше 1 МБ.");

            await using var stream = file.OpenReadStream();
            using var reader = XmlReader.Create(stream, ReaderSettings());
            var document = await XDocument.LoadAsync(reader, LoadOptions.None, cancellationToken);
            chapters = ReadChapters(document);
        }

        return Build(chapters);
    }

    public string Build(IReadOnlyList<ChapterInput> chapters)
    {
        if (chapters.Count is < 1 or > MaxChapters)
            throw new ValidationException("Оглавление должно содержать от 1 до 100 глав.");

        var elements = new List<XElement>(chapters.Count);
        for (var index = 0; index < chapters.Count; index++)
        {
            var chapter = chapters[index];
            var title = chapter.Title?.Trim() ?? "";
            if (title.Length is < 1 or > 200)
                throw new ValidationException($"Название главы {index + 1} должно содержать от 1 до 200 символов.");

            var body = chapter.BodyHtml ?? "";
            if (body.Length > MaxBodyLength)
                throw new ValidationException($"Текст главы {index + 1} слишком длинный (максимум 10 000 символов). ");

            elements.Add(new XElement("Chapter",
                new XAttribute("number", index + 1),
                new XAttribute("title", title),
                new XElement("BodyHtml", _sanitizer.Sanitize(body))));
        }

        return new XDocument(new XElement("Contents", elements))
            .ToString(SaveOptions.DisableFormatting);
    }

    public IReadOnlyList<ChapterInput> ReadChapters(string xml)
    {
        using var textReader = new StringReader(xml);
        using var reader = XmlReader.Create(textReader, ReaderSettings());
        return ReadChapters(XDocument.Load(reader));
    }

    private IReadOnlyList<ChapterInput> ReadChapters(XDocument document)
    {
        var root = document.Root;
        if (root?.Name != "Contents" || root.Attributes().Any() || root.Elements().Any(e => e.Name != "Chapter"))
            throw new ValidationException("Ожидался XML с корнем Contents и элементами Chapter.");

        var chapters = new List<ChapterInput>();
        foreach (var element in root.Elements("Chapter"))
        {
            var bodyElement = element.Element("BodyHtml");
            if (bodyElement is null || bodyElement.HasElements || element.Elements().Count() != 1)
                throw new ValidationException("Каждая глава должна содержать один элемент BodyHtml с HTML-текстом.");

            chapters.Add(new ChapterInput
            {
                Title = (string?)element.Attribute("title") ?? "",
                BodyHtml = _sanitizer.Sanitize(bodyElement.Value)
            });
        }

        if (chapters.Count is < 1 or > MaxChapters)
            throw new ValidationException("Оглавление должно содержать от 1 до 100 глав.");

        return chapters;
    }

    private static XmlReaderSettings ReaderSettings() => new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        MaxCharactersInDocument = MaxXmlBytes,
        Async = true
    };
}
