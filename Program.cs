using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SizeDB
{
    class Program
    {
        public static readonly string ClientSecret = "client_secret.json";
        public static readonly string[] ScopesSheets = { SheetsService.Scope.Spreadsheets };
        public static readonly string AppName = "SizeDB";
        public static readonly string SpreadsheetId = "1h5Q0ufdVYustYIoHlWmRqQjmuJ5mILj8bN8HDTWuHKE";
        static void Main(string[] args)
        {
            //Создание credential
            Console.WriteLine("Get Credentials");
            var credential = GetSheetCredentials();

            //Создание service
            Console.WriteLine("Get service");
            var service = GetService(credential);

            //Определяем имя сервера
            IServer server = new PostgreSQL();
            PostgreSQL serverName = new PostgreSQL(server);
            string server_name = serverName.GetServerName();

            Console.WriteLine($"Server name: {server_name}");

            //Определяем список баз данных на сервере
            IDataBase dataBase = new PostgreSQL();
            PostgreSQL dbName = new PostgreSQL(dataBase);
            List<string> db_name = dbName.GetDBList();

            //Узнаем размеры баз данных PostgreSQL на диске
            PostgreSQL dbSize = new PostgreSQL(dataBase);
            List<string> db_size = dbSize.GetDBSize(db_name);

            //Определяем свободное пространство на диске 
            PostgreSQL db_freeDiskSpace = new PostgreSQL(dataBase);
            string freeDiskSpace = db_freeDiskSpace.GetFreeDiskSpace(db_size);
            Console.WriteLine($"Free Disk space: {freeDiskSpace} Gb");

            //Проверка наличия листа с именем сервера,
            //если такого нет, создание листа
            //+ создание шапки таблицы
            CheckSheet(service, server_name);

            //Заполняем таблицу на Google Drive
            Console.WriteLine("Fill data to sheet");
            FillData fillData = new FillData();
            fillData.FillSheet(service, SpreadsheetId, db_name, db_size, server_name);

            //Выводим информацию о свободном месте на диске
            fillData.FillFreeDiskSize(service, SpreadsheetId, freeDiskSpace, server_name);

            Console.WriteLine("Done.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(); //Delay
        }

        private static UserCredential GetSheetCredentials()
        {
            using (var stream = new FileStream(ClientSecret, FileMode.Open, FileAccess.Read))
            {
                var credPath = Path.Combine(Directory.GetCurrentDirectory(), "sheetsCreds.json");
                return GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    ScopesSheets,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }
        }
        private static SheetsService GetService(UserCredential credential)
        {
            return new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = AppName
            });
        }
        private static void CheckSheet(SheetsService service, string server_name)
        {
            try
            {
                CreateSheet createSheet = new CreateSheet();
                LastRow lastRow = new LastRow();
                var ssRequest = service.Spreadsheets.Get(SpreadsheetId);
                Spreadsheet ss = ssRequest.Execute();
                List<string> sheetList = new List<string>();
                //Заполняем массив названиями листов
                foreach (Sheet sheet in ss.Sheets)
                {
                    sheetList.Add(sheet.Properties.Title);
                }

                string sn = null;
                //Проверяем наличие уже существующего листа
                foreach (string item in sheetList)
                {
                    if (item.Contains(server_name))
                    {
                        sn = item;
                    }
                }

                if (sn == null) //Создаем новый лист
                {
                    Console.WriteLine("Create sheet");
                    createSheet.Create(service, SpreadsheetId, server_name);
                    createSheet.AddTableHeader(service, SpreadsheetId, server_name);
                }
                else //Будем заполнять уже существующий лист данными
                {
                    Console.WriteLine("Go on");
                    Thread.Sleep(3000); //Ожидание заданного промежутка времени (3 сек.)

                    //Узнаем номер последней строки в листе
                    int last_row = lastRow.GetLastRow(service, SpreadsheetId, server_name);

                    //Удаляем последнюю строку с данными о свободном месте на диске(если имеется)
                    lastRow.DeleteLastRow(service, SpreadsheetId, last_row, server_name);
                }
            }
            catch(Exception)
            {
                Console.WriteLine("Невозможно подключиться к Google Sheets. Проверьте интернет соединение");
                Thread.Sleep(5000);
                Environment.Exit(1);
            }
 
        }

    }
}
