using GrapeCity.ActiveReports.Aspnetcore.Designer.Services;
using GrapeCity.ActiveReports.PageReportModel;
using GrapeCity.ActiveReports.Rdl.Tools;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace BlazorWebDesigner.ResourcesService
{
    public class SQLiteResourcesService : IResourcesService
    {
        private string ConnString { get; set; }

        public SQLiteResourcesService(string dbPath)
        {
            ConnString = $"Filename={dbPath};Mode=ReadWriteCreate";
            using (var conn = new SqliteConnection(ConnString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE  IF NOT EXISTS reports(id INTEGER PRIMARY KEY, name TEXT, layout BLOB, temp INTEGER, reportType TEXT);";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteReport(string id)
        {
            using (var conn = new SqliteConnection(ConnString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"Delete from reports where id={id}";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Uri GetBaseUri()
        {
            throw new NotImplementedException();
        }

        public byte[] GetImage(string id, out string mimeType)
        {
            throw new NotImplementedException();
        }

        public IImageInfo[] GetImagesList()
        {
            return new IImageInfo[] { };
        }

        public Report GetReport(string id)
        {
            // work-around the report processing that automatically adds .rdlx extension
            if (id.EndsWith(".rdlx"))
            {
                id = id.Substring(0, id.IndexOf(".rdlx"));
            }
            using (var conn = new SqliteConnection(ConnString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                    $"SELECT id, layout from reports where id = $id";
                    cmd.Parameters.AddWithValue("$id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            using (var readStream = reader.GetStream(1))
                            using (var outputStream = new MemoryStream())
                            {
                                readStream.CopyTo(outputStream);
                                return ReportConverter.FromXML(outputStream.ToArray());
                            }
                        }
                    }

                }
            }
            return null;
        }

        public IReportInfo[] GetReportsList()
        {
            var reportList = new List<ReportInfo>();
            using (var conn = new SqliteConnection(ConnString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "Select id, name, reportType from reports where temp = 0";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reportList.Add(new ReportInfo() { Id = reader.GetString(0), Name = reader.GetString(1), Type = reader.GetString(2) });
                        }
                    }
                }
            }
            return reportList.ToArray();
        }

        public Theme GetTheme(string id)
        {
            throw new NotImplementedException();
        }

        public IThemeInfo[] GetThemesList()
        {
            return new IThemeInfo[] { };
        }

        public string SaveReport(string name, Report report, bool isTemporary = false)
        {
            var rdlBytes = ReportConverter.ToXml(report);
            string rptType = report.Body.ReportItems.Count > 0 && report.Body.ReportItems[0].GetReportItemTypeName() == "FixedPage" ? "FPL" : "CPL";
            using (var conn = new SqliteConnection(ConnString))
            using (var repStream = new MemoryStream(rdlBytes))
            {
                conn.Open();
                var insertCommand = conn.CreateCommand();
                insertCommand.CommandText =
                $"INSERT INTO reports(name, layout, temp, reportType) VALUES (\"{name}\", zeroblob($length), {(isTemporary ? 1 : 0)}, \"{rptType}\"); SELECT last_insert_rowid();";
                insertCommand.Parameters.AddWithValue("$length", repStream.Length);
                var rowid = (long)insertCommand.ExecuteScalar();
                using (var writeStream = new SqliteBlob(conn, "reports", "layout", rowid))
                {
                    repStream.CopyTo(writeStream);
                }
                return rowid.ToString();
            }

        }

        public string UpdateReport(string id, Report report)
        {
            var rdlBytes = ReportConverter.ToXml(report);
            using (var conn = new SqliteConnection(ConnString))
            using (var repStream = new MemoryStream(rdlBytes))
            {
                conn.Open();
                using (var writeStream = new SqliteBlob(conn, "reports", "layout", long.Parse(id)))
                {
                    repStream.CopyTo(writeStream);
                }
                return id;
            }

        }
    }
}