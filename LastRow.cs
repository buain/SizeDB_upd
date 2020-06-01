using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;

namespace SizeDB
{
    public class LastRow
    {
        public int GetLastRow(SheetsService service, string SpreadsheetId, string server_name)
        {
            //Узнаем номер последней заполненной строки
            int lastRow = 0;
            var range = $"{server_name}!A:D";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            IList<IList<object>> values = response.Values;
            //Определяем количество строк
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    lastRow++;
                }
            }
            else
            {
                Console.WriteLine("No data found.");
            }
            return lastRow;
        }
        public void DeleteLastRow(SheetsService service, string SpreadsheetId, int lastRow, string server_name)
        {
            //Удаляем строку со старыми данными о размере свободного места на диске.
            var range = $"{server_name}!A{lastRow}:D";
            var requestBody = new ClearValuesRequest();

            var deleteRequest = service.Spreadsheets.Values.Clear(requestBody, SpreadsheetId, range);
            var deleteResponse = deleteRequest.Execute();
        }
       
    }
}
