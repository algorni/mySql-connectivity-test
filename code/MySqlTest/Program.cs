using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Serilog;
using SerilogTimings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MySqlTest
{
    class Program
    {
        private static IConfigurationRoot _configuration = null;

        private static string _mySqlConnectionString;

        private static MySqlConnection _mySqlConnection = null;

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appSettings.json", optional: true)
            .AddEnvironmentVariables();

            _configuration = builder.Build();

            _mySqlConnectionString = _configuration["mySqlConnectionString"].ToString();


            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.Console()
               .CreateLogger();

            Log.Logger.Information("Hello MySql!");


            while (true)
            {
                using (Operation.Time("Creation of MySqlConnection object"))
                {
                    _mySqlConnection = new MySqlConnection(_mySqlConnectionString);
                }

                string sql = "SELECT * FROM testschema.testtable;";

                using (Operation.Time("End to End Query Execution"))
                {
                    SqlMapper.GridReader queryResult = null;

                    using (Operation.Time("Dapper QueryMultipleAsync"))
                    {
                        queryResult = await _mySqlConnection.QueryMultipleAsync(sql);
                    }

                    IEnumerable<dataStructure> data = null;

                    using (Operation.Time("Dapper Read"))
                    {
                        data = queryResult.Read<dataStructure>().ToList(); 
                    }

                    foreach (var item in data)
                    {
                        Log.Logger.Information($"id: {item.id} - testdata: {item.testdata}");
                    }
                }

                Task.Delay(1000).Wait();
            }
        }
    }

    public class dataStructure
    {
        public int id { get; set; }
        public string testdata { get; set; }
    }
}
