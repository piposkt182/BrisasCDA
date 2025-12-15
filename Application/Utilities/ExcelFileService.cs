using Application.Utilities.Interfaces;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using System.Data;

namespace Application.Utilities
{
    public class ExcelFileService : IExcelFileService
    {
        public async Task<List<Dictionary<string, string>>> UploadExcel(IFormFile file)
        {
            var result = new List<Dictionary<string, string>>();

            using var stream = file.OpenReadStream();

            using var reader = ExcelReaderFactory.CreateReader(stream);

            var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true 
                }
            });

            var table = dataSet.Tables[0]; 

            foreach (DataRow row in table.Rows)
            {
                var rowData = new Dictionary<string, string>();

                foreach (DataColumn column in table.Columns)
                {
                    rowData[column.ColumnName] = row[column]?.ToString();
                }

                result.Add(rowData);
            }

            return result;
        }
    }
}
