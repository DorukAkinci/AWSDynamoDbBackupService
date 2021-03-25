using System;
using CommandLine;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.CredentialManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DynamoDbBackupService
{
    class Program
    {
        static SharedCredentialsFile _sharedCredentialsFile;
        static AmazonDynamoDBClient _dynamodDbClient;
        static bool _debugMode = false;
        static string _dateTime;
        public class Options
        {
            [Option(Required = false, HelpText = "AWS Profile Name")]
            public string? Profile { get; set; }

            [Option(Required = true, HelpText = "AWS Region Code")]
            public string Region { get; set; }

            [Option(Default = false, HelpText = "Debug Mode ( Only Table List )")]
            public bool Debug { get; set; }
        }

        static async Task Main(string[] args)
        {
            try
            {
                _dateTime = DateTime.Now.ToUniversalTime()
                         .ToString("yyyy'-'MM'-'dd'T'HH'-'mm'Z'");
                Console.WriteLine("The DynamoDb Backup Service is started on " + _dateTime);

                _sharedCredentialsFile = new SharedCredentialsFile();

                await Parser.Default.ParseArguments<Options>(args).MapResult(async
                    x =>
                {
                    await RunOptions(x);
                },
                    errors => Task.FromResult(0)
                );

                Console.WriteLine("*****");
                Console.WriteLine("The Application is successfully finished.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERR: " + ex.Message);
            }
        }

        static async Task RunOptions(Options opts)
        {
            //handle options
            Console.WriteLine("Profile: " + opts.Profile);
            Console.WriteLine("Region: " + opts.Region);
            Console.WriteLine("DebugMode: " + opts.Debug.ToString());

            _debugMode = opts.Debug;

            InitializeDynamoDBClient(opts.Profile, opts.Region);

            await ExecuteBackupTables();
        }

        private static async Task ExecuteBackupTables()
        {
            var _tableList = await GetAllTables();

            foreach (var _tableName in _tableList )
            {

                Console.WriteLine("-------------------");
                Console.WriteLine("Table Name: " + _tableName);

                if (!_debugMode)
                    await _dynamodDbClient.CreateBackupAsync(new CreateBackupRequest { TableName = _tableName, BackupName= _tableName + "-" + _dateTime }) ;
            }
        }

        private static void InitializeDynamoDBClient(string? Profile, string Region)
        {
            if (Profile != null)
                _dynamodDbClient = new AmazonDynamoDBClient(GetAWSCredentialProfile(Profile).Options.AccessKey, GetAWSCredentialProfile(Profile).Options.SecretKey, RegionEndpoint.GetBySystemName(Region));
            else
                _dynamodDbClient = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(Region));
        }

        private static CredentialProfile GetAWSCredentialProfile(string ProfileName)
        {
            CredentialProfile _credentialProfile;
            if (_sharedCredentialsFile.TryGetProfile(ProfileName, out _credentialProfile))
            {
                return _credentialProfile;
            }
            else
                throw new Exception("There is no profile name as " + ProfileName);
        }

        public static async Task<List<string>> GetAllTables()
        {
            List<string> _tableList = new List<string>();
            ListTablesResponse _response = new ListTablesResponse();
            do
            {
                _response = (await _dynamodDbClient.ListTablesAsync(new ListTablesRequest { ExclusiveStartTableName = _response.LastEvaluatedTableName ?? null })); 
                _tableList.AddRange(_response.TableNames);
            }
            while (_response.LastEvaluatedTableName != null);

            return _tableList;
        }

    }
}
