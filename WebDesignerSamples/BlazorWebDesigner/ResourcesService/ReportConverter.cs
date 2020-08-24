using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Xml;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.PageReportModel;
using GrapeCity.ActiveReports.Rdl.Json;
using GrapeCity.ActiveReports.Rdl.Persistence;

namespace BlazorWebDesigner.ResourcesService
{
	//
	// Summary:
	//     Provide report content conversation methods
	public static class ReportConverter
	{
		/// <summary>
		/// Deserialize <see cref="Report"/> to JSON content
		/// </summary>
		/// <param name="content">Serialized <see cref="Report"/></param>
		/// <returns>JSON report content</returns>
		public static byte[] ToJson(Report report)
		{
			// TODO: add license check
			using (var memoryStream = new MemoryStream())
			using (var streamWriter = new StreamWriter(memoryStream))
			{
				var serializer = ReportJsonSerializer.CreateSerializer();
				serializer.Serialize(streamWriter, report);

				streamWriter.Flush();

				return memoryStream.ToArray();
			}
		}

		/// <summary>
		/// Deserialize <see cref="Report"/> to XML content
		/// </summary>
		/// <param name="content">Serialized <see cref="Report"/></param>
		/// <returns>XML report content</returns>
		public static byte[] ToXml(Report report)
		{
			// TODO: add license check
			using (var memoryStream = new MemoryStream())
			using (var xmlWriter = XmlWriter.Create(memoryStream))
			{
				var filter = new PersistenceFilter(ReportComponentFactory.Instance, report, new DefaultResourceLocator());
				filter.SerializeRoot(report, xmlWriter);

				xmlWriter.Flush();
				return memoryStream.ToArray();
			}
		}

		/// <summary>
		/// Serialize <see cref="Report"/> from JSON content
		/// </summary>
		/// <param name="content">JSON report content</param>
		/// <returns>Serialized <see cref="Report"/></returns>
		public static Report FromJson(byte[] content)
		{
			// TODO: add license check
			using (var memoryStream = new MemoryStream(content))
			using (var streamReader = new StreamReader(memoryStream))
			{
				var jsonString = streamReader.ReadToEnd();
				return ReportJsonSerializer.Deserialize(jsonString);
			}
		}

		/// <summary>
		/// Serialize <see cref="Report"/> from XML content
		/// </summary>
		/// <param name="content">XML report content</param>
		/// <returns>Serialized <see cref="Report"/></returns>
		public static Report FromXML(byte[] content)
		{
			// TODO: add license check
			using (var memoryStream = new MemoryStream(content))
			using (var streamReader = new StreamReader(memoryStream))
			{
				return PersistenceFilter.Load(streamReader);
			}
		}
	}
}