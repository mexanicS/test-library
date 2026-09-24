namespace LibraryCatalog.Web.Models;

public static class RussianText
{
    public static string ChapterCount(int count)
    {
        var lastTwo = count % 100;
        var word = lastTwo is >= 11 and <= 14
            ? "глав"
            : (count % 10) switch
            {
                1 => "глава",
                2 or 3 or 4 => "главы",
                _ => "глав"
            };

        return $"{count} {word}";
    }
}
