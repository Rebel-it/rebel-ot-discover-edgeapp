using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rebelit.OT.Discover.EdgeApp.Exporters;

public interface ICsvExporters
{
    string CreateVariableCsv(List<Variable> variables);
    string CreateTagCsv(List<Tag> tags);
    Task CreateVariableCsvFileAsync(List<Variable> variables, string filePath);
    Task CreateTagCsvFileAsync(List<Tag> tags, string filePath);
}
