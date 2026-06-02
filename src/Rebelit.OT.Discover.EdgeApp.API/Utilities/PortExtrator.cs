namespace Rebelit.OT.Discover.EdgeApp.API.Utilities;

internal static class PortExtractor
{
    public static int? ExtractPort(string? opcuaAddress)
    {
        if (!Uri.TryCreate(opcuaAddress, UriKind.Absolute, out var uri))
        {
            return null;
        }

        return uri.IsDefaultPort ? null : uri.Port;
    }
}