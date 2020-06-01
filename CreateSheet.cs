using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.Collections.Generic;

namespace SizeDB
{
    public class CreateSheet
    {
        public void Create(SheetsService service, string spreadsheetId, string server_name)
        {
            //Создание листа с именем сервера(хоста)
            var addSheetRequest = new AddSheetRequest();
            addSheetRequest.Properties = new SheetProperties();

            addSheetRequest.Properties.Title = server_name;
            BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest();
            batchUpdateSpreadsheetRequest.Requests = new List<Request>();
            batchUpdateSpreadsheetRequest.Requests.Add(new Request
            {
                AddSheet = addSheetRequest
            });

            var batchUpdateRequest =
                service.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, spreadsheetId);

            batchUpdateRequest.Execute();
        }
        public void AddTableHeader(SheetsService service, string spreadsheetId, string server_name)
        {
            //Добавление шапки таблицы
            var range = $"{server_name}!A:D";
            var valueRange = new ValueRange();

            var oblist = new List<object>() { "Сервер", "База данных", "Размер в ГБ", "Дата обновления" };
            valueRange.Values = new List<IList<object>> { oblist };

            var appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            appendRequest.Execute();
        }
        

        
    }
}
