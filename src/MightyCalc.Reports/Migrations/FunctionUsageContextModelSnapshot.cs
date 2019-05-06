﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MightyCalc.Reports.DatabaseProjections;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace MightyCalc.Reports.Migrations
{
    [DbContext(typeof(FunctionUsageContext))]
    partial class FunctionUsageContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.3-servicing-35854")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("MightyCalc.Reports.DatabaseProjections.FunctionTotalUsage", b =>
                {
                    b.Property<string>("FunctionName")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("InvocationsCount");

                    b.HasKey("FunctionName");

                    b.ToTable("FunctionsTotalUsage");
                });

            modelBuilder.Entity("MightyCalc.Reports.DatabaseProjections.FunctionUsage", b =>
                {
                    b.Property<string>("CalculatorName");

                    b.Property<string>("FunctionName");

                    b.Property<DateTimeOffset>("PeriodStart");

                    b.Property<DateTimeOffset>("PeriodEnd");

                    b.Property<int>("InvocationsCount");

                    b.Property<TimeSpan>("Period");

                    b.HasKey("CalculatorName", "FunctionName", "PeriodStart", "PeriodEnd");

                    b.ToTable("FunctionsUsage");
                });

            modelBuilder.Entity("MightyCalc.Reports.DatabaseProjections.KnownFunction", b =>
                {
                    b.Property<string>("CalculatorId");

                    b.Property<string>("Name");

                    b.Property<int>("Arity");

                    b.Property<string>("Description");

                    b.Property<string>("Expression");

                    b.Property<string>("Parameters");

                    b.HasKey("CalculatorId", "Name");

                    b.ToTable("KnownFunctions");
                });

            modelBuilder.Entity("MightyCalc.Reports.Projection", b =>
                {
                    b.Property<string>("Name");

                    b.Property<string>("Projector");

                    b.Property<string>("Event");

                    b.Property<long>("Sequence");

                    b.HasKey("Name", "Projector", "Event");

                    b.ToTable("Projections");
                });
#pragma warning restore 612, 618
        }
    }
}
