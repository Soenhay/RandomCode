using FastMember;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class DB
    {
        #region Members
        //See appsettings.json and appsettings.Development.json for connection strings.
    
        string _connectionString;
        #endregion

        /*
         Powershell test:
          $connectionString = "Server=;Database=;Trusted_Connection=True;";
          $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString);
          $connection.Open();
          $connection.Close();
         */

        #region Initialization And Helper Methods

        public DB(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlCommand GetSqlCommand(SqlConnection conn, string storedProcedureName, SqlParameter[] sqlParameters = null)
        {
            SqlCommand cmd = new SqlCommand(storedProcedureName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddRange(sqlParameters);

            return cmd;
        }

        public async void ExecuteNonQueryAsync(string storedProcedureName, SqlParameter[] sqlParameters = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlCommand sqlCommand = GetSqlCommand(conn, storedProcedureName, sqlParameters);
                await sqlCommand.ExecuteNonQueryAsync();//TODO: cancellation token?
            }
        }

        public async Task<List<T>> ExecuteReaderAsync<T>(string storedProcedureName, SqlParameter[] sqlParameters = null) where T : class, new()
        {
            var newListObject = new List<T>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlCommand sqlCommand = GetSqlCommand(conn, storedProcedureName, sqlParameters);
                using (var dataReader = await sqlCommand.ExecuteReaderAsync(CommandBehavior.Default))
                {
                    if (dataReader.HasRows)
                    {
                        while (await dataReader.ReadAsync())
                        {
                            var newObject = new T();
                            dataReader.MapDataToObject(newObject);
                            newListObject.Add(newObject);
                        }
                    }
                }
            }
            return newListObject;
        }

        #endregion
    }

    public static class DBExtensions
    {
        /// <summary>
        /// Maps a SqlDataReader record to an object. Ignoring case.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="newObject"></param>
        /// <remarks>https://stackoverflow.com/a/52918088</remarks>
        public static void MapDataToObject<T>(this SqlDataReader dataReader, T newObject)
        {
            if (newObject == null) throw new ArgumentNullException(nameof(newObject));

            // Fast Member Usage
            var objectMemberAccessor = TypeAccessor.Create(newObject.GetType());
            var propertiesHashSet =
                    objectMemberAccessor
                    .GetMembers()
                    .Select(mp => mp.Name)
                    .ToHashSet(StringComparer.InvariantCultureIgnoreCase);

            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                var name = propertiesHashSet.FirstOrDefault(a => a.Equals(dataReader.GetName(i), StringComparison.InvariantCultureIgnoreCase));
                if (!String.IsNullOrEmpty(name))
                {
                    objectMemberAccessor[newObject, name]
                        = dataReader.IsDBNull(i) ? null : dataReader.GetValue(i);
                }
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="rd"></param>
        ///// <returns></returns>
        ///// <remarks>https://stackoverflow.com/a/44853182/1339704</remarks>
        //public static T ConvertToObject<T>(this SqlDataReader rd) where T : class, new()
        //{
        //    Type type = typeof(T);
        //    var accessor = TypeAccessor.Create(type);
        //    var members = accessor.GetMembers();
        //    var t = new T();

        //    for (int i = 0; i < rd.FieldCount; i++)
        //    {
        //        if (!rd.IsDBNull(i))
        //        {
        //            string fieldName = rd.GetName(i);

        //            if (members.Any(m => string.Equals(m.Name, fieldName, StringComparison.OrdinalIgnoreCase)))
        //            {
        //                accessor[t, fieldName] = rd.GetValue(i);
        //            }
        //        }
        //    }

        //    return t;
        //}
    }
}
