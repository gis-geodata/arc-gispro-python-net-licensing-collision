namespace Collision.Python
{
    class DataBase
    {
        public string DbName
        {
            get;
            set;
        }

        public string SdePath
        {
            get;
        }


        public DataBase(string dbName, string sdePath)
        {
            DbName = dbName;
            SdePath = sdePath;
        }
    }
}
