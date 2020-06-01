using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;

namespace SizeDB
{
    public class PostgreSQL : IServer, IDataBase
    {
        private IServer Server;
        private IDataBase DataBase;

        public PostgreSQL()
        {
        }

        public PostgreSQL(IServer server)
        {
            this.Server = server;
        }

        public PostgreSQL(IDataBase dataBase)
        {
            this.DataBase = dataBase;
        }

        List<string> DBName { get; set; } //список имен баз данных
        List<string> DBSize { get; set; } //список размеров баз данных
        public string GetServerName()
        {
            string server_name = null;
            try
            {
                //Подключение к серверу PostgreSQL
                var connectionString = ConfigurationManager.ConnectionStrings["ConnectToDB"].ConnectionString;
                //Создаем соединение с Npgsql provider
                var conn = new NpgsqlConnection(connectionString);
                server_name = conn.Host.ToString();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Ошибка получения имени сервера: {ex}");

            }
            return server_name;
        }
        public List<string> GetDBList()
        {
            try
            {
                //Определим список баз данных на сервере
                //Подключение к серверу PostgreSQL
                var connectionString = ConfigurationManager.ConnectionStrings["ConnectToDB"].ConnectionString;
                //Создаем соединение с Npgsql provider
                NpgsqlConnection conn = new NpgsqlConnection(connectionString);
                //Открываем соединение к БД
                conn.Open();
                //Текстовый запрос к БД
                string sql = "SELECT datname FROM pg_database";
                //Определяем запрос
                NpgsqlCommand da = new NpgsqlCommand(sql, conn);
                //Выполняем запрос и получем результат в виде списка
                NpgsqlDataReader reader = da.ExecuteReader();

                Console.WriteLine("Список БД на сервере:");
                List<string> db_name = new List<string>();
                //Заносим результат в список
                while (reader.Read())
                {
                    Console.WriteLine(reader.GetString(0));
                    db_name.Add(reader.GetString(0));
                }
                return db_name;
            }
            catch(Exception)
            {
                Console.WriteLine("Не удалось подключиться к серверу. Проверьте имя сервера");
                Thread.Sleep(5000);
                Environment.Exit(1);
            }
            
            return null;
        }
        public List<string> GetDBSize(List<string> db_name)
        {
            //Подключение к серверу PostgreSQL
            var connectionString = ConfigurationManager.ConnectionStrings["ConnectToDB"].ConnectionString;
            //Создаем соединение с Npgsql provider
            NpgsqlConnection conn = new NpgsqlConnection(connectionString);
            //Открываем соединение к БД
            conn.Open();
            Console.WriteLine("Список размеров БД на сервере:");
            List<string> db_size = new List<string>();
            //Для каждой базы данных узнаем размер
            foreach (var item in db_name)
            {
                string sql = $"SELECT pg_database_size( '{item}' )";
                var da = new NpgsqlCommand(sql, conn);
                //NpgsqlDataReader reader = da.ExecuteReader();
                double result = double.Parse(da.ExecuteScalar().ToString());
                var sizedb = Math.Round((result / 1024 / 1024 / 1024), 2);
                db_size.Add(sizedb.ToString());
                Console.WriteLine(sizedb);
            }

            //Закрываем соединение
            conn.Close();

            return db_size;
        }
        public string GetFreeDiskSpace(List<string> db_size)
        {
            double DBsize = 0; //Общий размер всех БД
            //Подключение к строке о размере диска
            var connectionString_ds = ConfigurationManager.ConnectionStrings["DiskSpace"].ConnectionString;

            double DiskSpace = double.Parse(connectionString_ds);//Размер диска

            foreach (var item in db_size)
            {
                DBsize += Convert.ToDouble(item);
            }

            double freeDiskSpace = DiskSpace - DBsize; //Свободное место на диске в Гб.
            return freeDiskSpace.ToString();
        }


    }
}
