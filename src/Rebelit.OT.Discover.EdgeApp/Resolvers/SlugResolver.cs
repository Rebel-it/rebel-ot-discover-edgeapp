namespace Rebelit.OT.Discover.EdgeApp.Resolvers;

internal static class SlugResolver
{
    /// <summary>
    /// Converts the specified input string into a normalized, URL-friendly slug.
    /// </summary>
    /// <remarks>The normalization process replaces spaces, underscores, and periods with hyphens, removes all
    /// characters except letters, digits, and hyphens, and trims leading and trailing hyphens. This method is useful
    /// for generating identifiers or URLs from arbitrary strings.</remarks>
    /// <param name="input">The input string to normalize.</param>
    /// <returns>A normalized slug consisting of lowercase letters, digits, and hyphens. Returns "opcua" if the input is null,
    /// empty, or contains no valid characters.</returns>
    public static string Resolve(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "opcua";
        var slug = input.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Replace(".", "-");
        slug = new string(slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

        slug = slug.Trim('-');
        return string.IsNullOrEmpty(slug) ? "opcua" : slug;
    }
}
