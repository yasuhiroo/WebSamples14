using GrapeCity.ActiveReports.Aspnetcore.Designer.Services;

namespace BlazorWebDesigner.ResourcesService
{
    public class ReportInfo : IReportInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}