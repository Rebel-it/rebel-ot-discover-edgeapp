namespace Rebelit.OT.Discover.EdgeApp.API.Utilities;

internal static class SlugGenerator
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

        var normalized = new string([
            .. slug.ToLowerInvariant().Select(c =>
                c is >= 'a' and <= 'z' ||
                c is >= '0' and <= '9' ||
                c is '_' ||
                c is '-'
                    ? c
                    : '_'
            ),
        ]);

        if (string.IsNullOrWhiteSpace(normalized))
        {
            normalized = "a";
        }

        if (normalized[0] is < 'a' or > 'z')
        {
            normalized = $"a{normalized}";
        }

        return normalized.Length > maxSlugLength ? normalized[..maxSlugLength] : normalized;
    }
}
