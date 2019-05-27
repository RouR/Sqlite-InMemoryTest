using System;
using System.Data;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Test
{
    public class Tests
    {
        private TestInMemoryDbContext context;
        private TestLogsKeeper logs;

        [SetUp]
        public void Setup()
        {
            /*var c = new SqliteConnectionStringBuilder()
            {
                BinaryGUID  = ":memory:",
                Mode = 
            };*/
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<TestInMemoryDbContext>()
                .UseSqlite(connection)
                .EnableSensitiveDataLogging()
                .Options;

            logs = new TestLogsKeeper();
            ILoggerFactory logFactory = new LoggerFactory();
            logFactory.AddProvider(new UnitTestLoggerProvider(logs));

            context = new TestInMemoryDbContext(options, logFactory);

            context.Database.EnsureCreated();
        }

        [TearDown]
        public void Down()
        {
            context.Dispose();
            foreach (var log in logs.GetLogs())
                Console.WriteLine(log + Environment.NewLine);
        }

        [Test]
        public void Tests_Should_LINQ_PK()
        {
            //Given
            var record = new SomeEntity();
            var id = record.Id;
            var fVal = record.SomeField;
            context.SomeEntities.Add(record);
            context.SaveChanges();
            context.SomeEntities.Local.Clear();
            context.Entry(record).State = EntityState.Detached;
            //When
            var data = context.SomeEntities.SingleOrDefault(_ => _.Id == id);
            //Then
            Assert.NotNull(data);
            Assert.AreEqual(id, data.Id);
        }

        [Test]
        public void Tests_Should_LINQ_NotPK()
        {
            //Given
            var record = new SomeEntity();
            var id = record.Id;
            var fVal = record.SomeField;
            context.SomeEntities.Add(record);
            context.SaveChanges();
            context.SomeEntities.Local.Clear();
            context.Entry(record).State = EntityState.Detached;
            //When
            var data = context.SomeEntities.SingleOrDefault(_ => _.SomeField == fVal);
            //Then
            Assert.NotNull(data);
            Assert.AreEqual(id, data.Id);
        }

        [Test]
        public void Tests_Should_LINQ_Both()
        {
            //Given
            var record = new SomeEntity();
            var id = record.Id;
            var fVal = record.SomeField;
            context.SomeEntities.Add(record);
            context.SaveChanges();
            context.SomeEntities.Local.Clear();
            context.Entry(record).State = EntityState.Detached;
            //When
            var data = context.SomeEntities.SingleOrDefault(_ => _.Id == id && _.SomeField == fVal);
            //Then
            Assert.NotNull(data);
            Assert.AreEqual(id, data.Id);
        }


        [Test]
        public void Tests_Should_DbCommand()
        {
            //Given
            var record = new SomeEntity();
            var id = record.Id;
            var fVal = record.SomeField;
            var newVal = Guid.NewGuid();
            context.SomeEntities.Add(record);
            context.SaveChanges();
            context.SomeEntities.Local.Clear();
            context.Entry(record).State = EntityState.Detached;

            foreach (var entity in context.SomeEntities.AsNoTracking().ToList())
            {
                Console.WriteLine($"1 {entity.Id} {entity.SomeField}");
            }

            var conn = context.Database.GetDbConnection();
            using (var createCommand = conn.CreateCommand())
            {
                //createCommand.CommandText = $"update public.Locks SET Lock = @newLock  where {columnName} = '@id' AND Lock IS NULL";

                //createCommand.Parameters.Add(CreateParam(createCommand, "@id", uid1));
                //createCommand.Parameters.Add(CreateParam(createCommand, "@newLock", lockValue));

                createCommand.CommandText = $"update Test"
                                            + $" SET "
                                            + $" SomeField = '{newVal.ToString("D")}'  "
                                            + $" where "
                                            + $" Id = '{id.ToString("D")}' ";

                if (conn.State != ConnectionState.Open)
                    conn.Open();
                createCommand.ExecuteScalar();
            }
            //When
            var data = context.SomeEntities.SingleOrDefault(_ => _.Id == id && _.SomeField == fVal);
            //Then

            var d = context.SomeEntities.FromSql($"SELECT * FROM Test WHERE Id = '{id.ToString("D")}'").SingleOrDefault();

            foreach (var entity in context.SomeEntities.AsNoTracking().ToList())
            {
                Console.WriteLine($"2 {entity.Id} {entity.SomeField}");
            }

            Assert.NotNull(data);
            Assert.AreEqual(id, data.Id);
            Assert.AreEqual(newVal, data.SomeField);

        }


        [Test]
        public void Tests_Should_FromSql()
        {
            //Given
            var record = new SomeEntity();
            var id = record.Id;
            var fVal = record.SomeField;
            var newVal = Guid.NewGuid();
            context.SomeEntities.Add(record);
            context.SaveChanges();
            context.SomeEntities.Local.Clear();
            context.Entry(record).State = EntityState.Detached;

            foreach (var entity in context.SomeEntities.AsNoTracking().ToList())
            {
                Console.WriteLine($"1 {entity.Id} {entity.SomeField}");
            }

            var conn = context.Database.GetDbConnection();
            using (var createCommand = conn.CreateCommand())
            {
                //createCommand.CommandText = $"update public.Locks SET Lock = @newLock  where {columnName} = '@id' AND Lock IS NULL";

                //createCommand.Parameters.Add(CreateParam(createCommand, "@id", uid1));
                //createCommand.Parameters.Add(CreateParam(createCommand, "@newLock", lockValue));

                createCommand.CommandText = $"update Test"
                                            + $" SET "
                                            + $" SomeField = '{newVal.ToString("D")}'  "
                                            + $" where "
                                            + $" Id = '{id.ToString("D")}' ";

                if (conn.State != ConnectionState.Open)
                    conn.Open();
                createCommand.ExecuteScalar();
            }
            //When
            var data =  context.SomeEntities.FromSql($"SELECT * FROM Test WHERE Id = '{id.ToString("D")}'").SingleOrDefault();
            //Then

            foreach (var entity in context.SomeEntities.AsNoTracking().ToList())
            {
                Console.WriteLine($"2 {entity.Id} {entity.SomeField}");
            }

            Assert.NotNull(data);
            Assert.AreEqual(id, data.Id);
            Assert.AreEqual(newVal, data.SomeField);

        }

        [Test]
        public void EFCatch_ClientQuery()
        {
            //Given
            var record = new SomeEntity();
            var id = record.Id;
            var fVal = record.SomeField;
            context.SomeEntities.Add(record);
            context.SaveChanges();
            context.SomeEntities.Local.Clear();
            context.Entry(record).State = EntityState.Detached;

            //WHEN
            try
            {
                var filtered = context.SomeEntities.Where(_ => _.Id.ToString() == id.ToString()).ToArray();
                //THEN
                Assert.Fail("use optionsBuilder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Assert.Pass();
            }
        }
    }
}