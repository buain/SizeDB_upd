using System.Collections.Generic;

namespace SizeDB
{
    public interface IDataBase
    {
        List<string> GetDBList(); //получаем список баз данных
        List<string> GetDBSize(List<string> db_name); //получаем список размеров баз данных
        string GetFreeDiskSpace(List<string> db_size); //получаем размер свободного места на диске
    }
}
