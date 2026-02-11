using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InnerG.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InnerG.Api.Data.Configurations
{
    public class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            // 🔑 Primary Key
            builder.HasKey(x => x.Id);

            // 🔒 Token
            builder.Property(x => x.Token)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.HasIndex(x => x.Token)
                   .IsUnique();

            builder.Property(x => x.Created)
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.Expires)
                   .IsRequired();

            // 🔄 Revocation
            builder.Property(x => x.IsRevoked)
                   .HasDefaultValue(false);

            // 🔗 Relation: RefreshToken → AppUser
            builder.HasOne(x => x.AppUser)
                   .WithMany(u => u.RefreshTokens)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}