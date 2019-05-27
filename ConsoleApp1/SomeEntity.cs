using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Test
{
    public class SomeEntity
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid? SomeField { get; private set; } = Guid.NewGuid();

        public SomeEntity() 
        {
        }


        internal class Configuration : IEntityTypeConfiguration<SomeEntity>
        {
            public void Configure(EntityTypeBuilder<SomeEntity> builder)
            {
                builder.ToTable("Test");
                builder.HasKey(x => x.Id);
            }
        }
    }
}