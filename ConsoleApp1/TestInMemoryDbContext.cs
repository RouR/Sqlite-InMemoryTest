using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Test
{
    public class TestInMemoryDbContext: DbContext
    {
        internal const string SqlConVariableName = "InMemorySqlCon";
        public DbSet<SomeEntity> SomeEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            //don`t call SaveChanges() - it will throw exception:
            // A DbContext instance cannot be used inside OnModelCreating

            builder.ApplyConfiguration(new SomeEntity.Configuration());
        }


        /// <summary>
        /// Used for console commands - dotnet ef migrations add MigrationName
        /// </summary>
        public TestInMemoryDbContext()
        {
            //don`t delete
        }

        /// <summary>
        /// Used in runtime for EF Migrations
        /// </summary>
        /// <param name="options"></param>
        public TestInMemoryDbContext(DbContextOptions<TestInMemoryDbContext> options)
            : base(options)
        {
            //don`t delete
        }

        /// <summary>
        /// for unit tests
        /// </summary>
        private readonly ILoggerFactory _loggerFactory;
        /// <summary>
        /// for unit tests
        /// </summary>
        /// <param name="options"></param>
        /// <param name="loggerFactory"></param>
        public TestInMemoryDbContext(DbContextOptions<TestInMemoryDbContext> options, ILoggerFactory loggerFactory)
            : base(options)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Used for console commands - dotnet ef migrations add MigrationName
        /// </summary>
        /// <param name="optionsBuilder"></param>
        /// <exception cref="Exception"></exception>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_loggerFactory != null)
            {
                optionsBuilder.UseLoggerFactory(_loggerFactory);
            }
            optionsBuilder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));

            if (optionsBuilder.IsConfigured)
            {
                base.OnConfiguring(optionsBuilder);
            }
            else
            {
                //var connection = GetSqlConn(SqlConVariableName);
            }
        }


      
    }
}