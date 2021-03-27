using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Python.Runtime;

namespace Collision.Python
{
    public class PythonScripts : IDisposable
    {
        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~PythonScripts()
        {
            ReleaseUnmanagedResources();
        }

        #region Members

        private readonly string m_TempPath = Path.GetTempPath();
        private readonly Func<string, string> m_DbUserCheck = cityName =>
            $@"select top 1 dp.principal_id     
                from sys.server_principals sp
                left join sys.database_principals dp
                          on sp.sid = dp.sid
                where sp.name='{cityName}'
                order by sp.name;";

        private ILogger m_Logger;

        #region Props

        #endregion

        #endregion

        #region C'tor

        public PythonScripts(ILogger logger)
        {
            m_Logger = logger;
        }

        #endregion

        #region Public

        public ResponseWrapper<string> CreateCity(string cityName, int wkid)
        {
            var errors = new List<Error>();

            cityName = NormalizeCityName(cityName);
            ClearConnectionFiles();
            using var state = Py.GIL();
            
            using dynamic arcpy = Py.Import("arcpy");
            using var spatialReference = arcpy.SpatialReference(wkid);
            using var management = arcpy.management;


            var database = new DataBase("GeodataCadaster", @"C:\CadasterSystem\sde\cadaster-ad.sde");

            try
            {
                errors.AddRange(CreateCityUser(cityName, arcpy, database));
            }
            catch (Exception e)
            {
                HandleError(e, errors);
            }
            
            return new ResponseWrapper<string>(errors);
        }
        #endregion

        #region Private

        private static string NormalizeCityName(string cityName)
        {
            cityName = cityName.ToLowerInvariant().Replace(" ", "_");
            return cityName;
        }

        private void ClearConnectionFiles()
        {
            var files = Directory.GetFiles(m_TempPath, "*.sde");
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
        private List<Error> CreateCityUser(string cityName, dynamic arcpy, DataBase database)
        {
            var errors = new List<Error>();
            try
            {
                var management = arcpy.management;
                var connectionString = new PyString(database.SdePath);
                var cityAdminConnection = arcpy.ArcSDESQLExecute(connectionString);
                var dbUserCheck = m_DbUserCheck(cityName);
                PyObject userExists = cityAdminConnection.execute(dbUserCheck);

                var isUserId = int.TryParse(userExists.Repr(), out var existingUserId);
                if (isUserId)
                {
                    return errors;
                }

                var userPass = CreateUserPass(cityName);
                m_Logger.LogDebug($"City pass: {userPass}");
                using var createdUser = management.CreateDatabaseUser(connectionString,
                    "DATABASE_USER", $"{cityName}", userPass, "", "");

            }
            catch (Exception e)
            {
                HandleError(e, errors);
            }

            return errors;
        }

        private void HandleError(Exception e, List<Error> errors)
        {
            m_Logger.LogWarning(e, e.Message);
            errors.Add(new Error(e.Message));
        }

        private string CreateUserPass(string userName)
        {
            return "VErY_Strong_UseR_Pass!!";
        }
        
        #endregion
    }
}
