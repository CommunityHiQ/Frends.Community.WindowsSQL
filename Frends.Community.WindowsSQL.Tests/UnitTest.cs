using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Frends.Community.WindowsSQL.Tests
{
    [TestFixture]
    public class UnitTests
    {

        /*
        docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Salakala123!" -p 1433:1433 --name sql1 --hostname sql1 -d mcr.microsoft.com/mssql/server:2019-CU18-ubuntu-20.04
        with Git bash add winpty to the start of
        docker exec -it sql1 "bash"
        /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "Salakala123!"
        
        Check rows before CleanUp:
        SELECT * FROM TestTable
        GO
    
        Optional queries:
        SELECT Name FROM sys.Databases;
        GO
        SELECT * FROM INFORMATION_SCHEMA.TABLES;
        GO
    */

        private static readonly string _connString = "Server=127.0.0.1,1433;Database=Master;User Id=SA;Password=Salakala123!";
        private static readonly string _tableName = "TestTable";
        private static readonly string _procedureName = "TestProcedure";

        [SetUp]
        public void Init()
        {
            using var connection = new SqlConnection(_connString);
            connection.Open();
            var createTable = connection.CreateCommand();
            createTable.CommandText = $@"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='{_tableName}') BEGIN CREATE TABLE {_tableName} ( Id int, LastName varchar(255), FirstName varchar(255) ); END";
            createTable.ExecuteNonQuery();
            connection.Close();
            connection.Dispose();

            InsertStoredProcedure(_procedureName);
        }

        [TearDown]
        public void CleanUp()
        {
            using var connection = new SqlConnection(_connString);
            connection.Open();
            var createTable = connection.CreateCommand();
            createTable.CommandText = $@"IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='{_tableName}') BEGIN DROP TABLE IF EXISTS {_tableName}; END";
            createTable.ExecuteNonQuery();
            connection.Close();
            connection.Dispose();

            DropStoredProcedure(_procedureName);
        }

        [Test]
        public async Task TestQueryWithParameters()
        {
            var dataToQuery = new List<TestRow>()
            {
                new TestRow() {FirstName = "Etu", Id = 1, LastName = "Suku"},
                new TestRow() {FirstName = "First", Id = 2, LastName = "Last"},
                new TestRow() {FirstName = "Some", Id = 3, LastName = "Name"},
            };
            await AddDataToTable(dataToQuery);

            var result = (JArray)
                await
                    Sql.ExecuteQuery(
                        new InputQuery()
                        {
                            ConnectionString = _connString,
                            Query = $"select * FROM {_tableName} where LastName = @name",
                            Parameters = new[] { new Parameter() { Name = "Name", Value = "Last" } }
                        },
                        new Options()
                        {
                            CommandTimeoutSeconds = 60,
                            SqlTransactionIsolationLevel = SqlTransactionIsolationLevel.Default
                        }, CancellationToken.None);

            Assert.That(result, Has.Exactly(1).Items);
            Assert.AreEqual(2, result.First()["Id"].Value<int>());
        }

        public async Task TestSimpleInsertQueryWithParameters()
        {
            var result = (JArray)
                await
                    Sql.ExecuteQuery(
                        new InputQuery()
                        {
                            ConnectionString = _connString,
                            Query = $"INSERT INTO {_tableName} VALUES(@Id, @LastName, @FirstName); ",
                            Parameters = new[]
                            {
                                new Parameter() {Name = "LastName", Value = "Last"},
                                new Parameter() {Name = "FirstName", Value = "First"},
                                new Parameter() {Name = "Id", Value = "15"}
                            }
                        },
                        new Options()
                        {
                            CommandTimeoutSeconds = 60,
                            SqlTransactionIsolationLevel = SqlTransactionIsolationLevel.Default
                        }, CancellationToken.None);

            Assert.IsEmpty(result);
            var query = await GetAllResults();

            Assert.That(query, Has.Exactly(1).Items);
            Assert.AreEqual(15, query[0]["Id"].Value<int>());
        }

        [Test]
        public async Task TestBatchOperationInsert()
        {
            var result =
                await
                    Sql.BatchOperation(
                        new InputBatchOperation()
                        {
                            ConnectionString = _connString,
                            Query = $"insert {_tableName}(Id,FirstName,LastName) VALUES(1,@FirstName,'LastName')",
                            InputJson = "[{\"FirstName\":\"Onunous\"},{\"FirstName\":\"Doosshits\"}]",
                        },
                        new Options()
                        {
                            CommandTimeoutSeconds = 60,
                            SqlTransactionIsolationLevel = SqlTransactionIsolationLevel.Serializable
                        }, CancellationToken.None);
            Assert.AreEqual(2, result);
            var query = await GetAllResults();

            Assert.AreEqual(2, query.Count());
            Assert.AreEqual("Onunous", query[0]["FirstName"].ToString());
        }

        [Test]
        public async Task TestStoredProcedureThatInsertsRow()
        {
            var result = (JArray)
                await
                    Sql.ExecuteProcedure(
                        new InputProcedure()
                        {
                            ConnectionString = _connString,
                            Execute = _procedureName,
                            Parameters = new[]
                            {
                                new Parameter() {Name = "FirstName", Value = "First"},
                                new Parameter() {Name = "LastName", Value = "Last"}
                            }
                        },
                        new Options()
                        {
                            CommandTimeoutSeconds = 60,
                            SqlTransactionIsolationLevel = SqlTransactionIsolationLevel.ReadCommitted
                        }, CancellationToken.None);
            Assert.NotNull(result);
            Assert.IsEmpty(result);

            var query = await GetAllResults();

            Assert.That(query, Has.Exactly(1).Items);
            Assert.AreEqual("First", query[0]["FirstName"].ToString());
        }

        [Test]
        public async Task TestBulkInsert()
        {
            var dataToInsert = new List<TestRow>()
            {
                new TestRow() {FirstName = "Etu", Id = 1, LastName = "Suku"},
                new TestRow() {FirstName = "First", Id = 2, LastName = "Last"},
                new TestRow() {FirstName = "Eka", Id = 3, LastName = "Name"}
            };
            var inputJson = JsonConvert.SerializeObject(dataToInsert);

            var result =
                await
                    Sql.BulkInsert(
                        new BulkInsertInput()
                        {
                            ConnectionString = _connString,
                            TableName = _tableName,
                            InputData = inputJson
                        },
                        new BulkInsertOptions()
                        {
                            CommandTimeoutSeconds = 60,
                            FireTriggers = true,
                            KeepIdentity = false,
                            SqlTransactionIsolationLevel = SqlTransactionIsolationLevel.ReadCommitted
                        }, CancellationToken.None);
            Assert.AreEqual(3, result);

            var query = await GetAllResults();
            Assert.AreEqual(3, query.Count());
            Assert.AreEqual("Suku", query[0]["LastName"].ToString());
        }

        [Test]
        public async Task BulkInsertWithEmptyPropertyValuesShouldBeNull()
        {
            var dataToInsert = new List<TestRow>()
            {
                new TestRow() {FirstName = "Etu", Id = 1, LastName = "Suku"},
                new TestRow() {FirstName = "First", Id = 2, LastName = "Last"},
                new TestRow() {FirstName = "", Id = 3, LastName = "Name"}
            };
            var inputJson = JsonConvert.SerializeObject(dataToInsert);

            var result =
                await
                    Sql.BulkInsert(
                        new BulkInsertInput()
                        {
                            ConnectionString = _connString,
                            TableName = _tableName,
                            InputData = inputJson
                        },
                        new BulkInsertOptions()
                        {
                            CommandTimeoutSeconds = 60,
                            FireTriggers = true,
                            KeepIdentity = false,
                            ConvertEmptyPropertyValuesToNull = true,
                            SqlTransactionIsolationLevel = SqlTransactionIsolationLevel.Default
                        }, CancellationToken.None);
            Assert.AreEqual(3, result);

            var query = await GetAllResults();
            Assert.AreEqual(3, query.Count());
            Assert.Null(query[2]["FirstName"].Value<DBNull>());
        }

        private async Task AddDataToTable(IEnumerable<TestRow> tastTableData)
        {
            await
                Sql.BatchOperation(
                    new InputBatchOperation()
                    {
                        ConnectionString = _connString,
                        Query = $"insert {_tableName}(Id,FirstName,LastName) VALUES(@Id,@FirstName,@LastName)",
                        InputJson = JsonConvert.SerializeObject(tastTableData),
                    },
                    new Options()
                    {
                        CommandTimeoutSeconds = 60,
                        SqlTransactionIsolationLevel = SqlTransactionIsolationLevel.Serializable
                    },
                    CancellationToken.None);
        }

        private async Task<JToken> GetAllResults()
        {
            return (JArray)await
                Sql.ExecuteQuery(
                    new InputQuery()
                    {
                        ConnectionString = _connString,
                        Query = $"select * FROM {_tableName}",
                        Parameters = new Parameter[0]
                    }, new Options() { CommandTimeoutSeconds = 60 }, CancellationToken.None);
        }

        private static void InsertStoredProcedure(string procedureName)
        {
            string procedure = $@"CREATE PROCEDURE [{procedureName}]
                @FirstName varchar(255),@LastName varchar(255) as BEGIN
                SET NOCOUNT ON; 
                INSERT INTO {_tableName} (FirstName, LastName, Id) VALUES (@FirstName,@LastName,1) 
                END";
            using (var connection = new SqlConnection(_connString))
            {
                using (var cmd = new SqlCommand(procedure, connection))
                {
                    connection.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        private static void DropStoredProcedure(string procedureName)
        {
            string procedure = $@"DROP PROCEDURE {procedureName};";

            using (var connection = new SqlConnection(_connString))
            {
                using (var cmd = new SqlCommand(procedure, connection))
                {
                    connection.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

    }

        public class TestRow
        {
            public int Id { get; set; }
            public string LastName { get; set; }

            public string FirstName { get; set; }
        }
    }
