namespace DataAccessLayer.Services
{
    public abstract class DalServiceBase
    {
        protected DB db;

        public DalServiceBase(string connectionString)
        {
            db = new DB(connectionString);
        }
    }
}
