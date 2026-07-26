using HermesZoneTorba.Domain.Common;
using HermesZoneTorba.Domain.HermesManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HermesZoneTorba.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the HermesInstance aggregate to the "hermes_instances" table. Configuration (a value object)
/// is stored as a jsonb column — see docs/12-database.md#conventions for when jsonb is the right call
/// versus a normalized column/table.
/// </summary>
public sealed class HermesInstanceConfiguration : IEntityTypeConfiguration<HermesInstance>
{
    public void Configure(EntityTypeBuilder<HermesInstance> builder)
    {
        builder.ToTable("hermes_instances");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(i => i.InstallPath).HasMaxLength(1024).IsRequired();
        builder.Property(i => i.LastFaultReason).HasMaxLength(2000);

        builder.Property(i => i.Version)
            .HasConversion(
                v => v == null ? null : v.ToString(),
                v => v == null ? null : SemVer.Parse(v))
            .HasColumnName("version")
            .HasMaxLength(64);

        builder.OwnsOne(i => i.Configuration, config =>
        {
            config.ToJson();
            config.OwnsOne(c => c.ResourceLimits);
        });

        // Postgres' native xmin system column backs optimistic concurrency instead of a manual counter —
        // the domain-level ConcurrencyVersion property is intentionally not mapped 1:1 to it.
        builder.Ignore(i => i.ConcurrencyVersion);
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();

        builder.HasIndex(i => i.Status);
    }
}
