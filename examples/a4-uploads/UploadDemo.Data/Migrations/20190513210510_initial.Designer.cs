﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UploadDemo.Data;

namespace UploadDemo.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20190513210510_initial")]
    partial class initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.3-servicing-35854")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("UploadDemo.Data.Entities.Folder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Folder");
                });

            modelBuilder.Entity("UploadDemo.Data.Entities.FolderUpload", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("FolderId");

                    b.Property<int>("UploadId");

                    b.HasKey("Id");

                    b.HasIndex("FolderId");

                    b.HasIndex("UploadId");

                    b.ToTable("FolderUpload");
                });

            modelBuilder.Entity("UploadDemo.Data.Entities.Upload", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("File");

                    b.Property<string>("FileType");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Name");

                    b.Property<string>("Path");

                    b.Property<long>("Size");

                    b.Property<DateTime>("UploadDate");

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.ToTable("Upload");
                });

            modelBuilder.Entity("UploadDemo.Data.Entities.FolderUpload", b =>
                {
                    b.HasOne("UploadDemo.Data.Entities.Folder", "Folder")
                        .WithMany("FolderUploads")
                        .HasForeignKey("FolderId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("UploadDemo.Data.Entities.Upload", "Upload")
                        .WithMany("UploadFolders")
                        .HasForeignKey("UploadId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
