using Core.Dto;

namespace Core.Import;

public static class MixedCsvImporter
{
    public static List<object> Load(string path)
    {
        var items = new List<object>();

        foreach (string line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split(
                ';',
                StringSplitOptions.TrimEntries);

            if (parts.Length == 0)
                continue;

            object? item = parts[0] switch
            {
                "B" when parts.Length == 5 =>
                    new BookDto(
                        parts[1],
                        parts[2],
                        parts[3],
                        int.Parse(parts[4])),

                "R" when parts.Length == 4 =>
                    new ReaderDto(
                        parts[1],
                        parts[2],
                        parts[3]),

                _ => null
            };

            if (item is not null)
                items.Add(item);
        }

        return items;
    }
}