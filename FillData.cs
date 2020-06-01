using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;

namespace SizeDB
{
    public class FillData
    {
        public void FillSheet(SheetsService service, string spreadsheetId, List<string> db_name, List<string> db_size, string server_name)
        {
            DateTime update_date = DateTime.Now; //дата обновления

            var range = $"{server_name}!A:D"; //диапазон ячеек для заполнения
            var valueRange = new ValueRange();
            for (int i = 0; i < db_name.Count; i++)
            {
                var oblist = new List<object> { server_name, db_name[i], db_size[i], update_date.ToShortDateString() };
                valueRange.Values = new List<IList<object>> { oblist };
                var appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                appendRequest.Execute();
            }
        }
        public void FillFreeDiskSize(SheetsService service, string spreadsheetId, string freeDiskSpace, string server_name)
        {
            DateTime update_date = DateTime.Now; //дата обновления

            var range = $"{server_name}!A:D";
            var valueRange = new ValueRange();

            var oblist = new List<object>() { server_name, "Свободно", freeDiskSpace, update_date.ToShortDateString() };
            valueRange.Values = new List<IList<object>> { oblist };

            var appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            appendRequest.Execute();
        }
    }
}
