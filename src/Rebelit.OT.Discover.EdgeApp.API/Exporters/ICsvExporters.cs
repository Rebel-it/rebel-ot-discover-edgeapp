using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.API.Exporters;

public interface ICsvExporters
{
    string CreateVariableCsv(List<Variable> variables);
    string CreateTagCsv(List<Tag> tags);
    Task CreateVariableCsvFileAsync(List<Variable> variables, string filePath);
    Task CreateTagCsvFileAsync(List<Tag> tags, string filePath);
}
