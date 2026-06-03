namespace Rebelit.OT.Discover.EdgeApp.API.Utilities;

public static class SlugGenerator
{
    public static string CreateFromNameAndAddress(string? name, string? address)
    {
        var baseSlug = new string([
            .. (name ?? string.Empty).Where(char.IsLetterOrDigit),
        ]).ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(address))
        {
            var separatorIndex = address.IndexOf(';');
            if (separatorIndex >= 0 && separatorIndex + 1 < address.Length)
            {
                var nodeIdPart = address[(separatorIndex + 1)..].Replace("=", string.Empty);
                if (!string.IsNullOrWhiteSpace(nodeIdPart))
                {
                    return Normalize($"{baseSlug}_{nodeIdPart}");
                }
            }
        }

        return Normalize(baseSlug);
    }

    private static string Normalize(string slug)
    {
        const int maxSlugLength = 64;

        var normalized = new string(
            slug.ToLowerInvariant()
                .Select(c => IsAllowedSlugChar(c) ? c : '_')
                .ToArray());

        if (string.IsNullOrWhiteSpace(normalized))
        {
            normalized = "a";
        }

        if (!char.IsAsciiLetter(normalized[0]))
        {
            normalized = $"a{normalized}";
        }

        if (normalized.Length > maxSlugLength)
        {
            normalized = normalized[..maxSlugLength];
        }

        return normalized;
    }

    private static bool IsAllowedSlugChar(char c)
    {
        return char.IsAsciiLetterOrDigit(c) || c is '_' or '-';
    }
}
