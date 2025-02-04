﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NewsApp.Contexts;

#nullable disable

namespace NewsApp.Migrations
{
    [DbContext(typeof(NewsAppDBContext))]
    [Migration("20240725200633_requiredRemovedOauth")]
    partial class requiredRemovedOauth
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.32")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("NewsApp.Models.Article", b =>
                {
                    b.Property<int>("ArticleID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ArticleID"), 1L, 1);

                    b.Property<DateTime>("AddedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("HashID")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImgURL")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("ImpScore")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("OldHashID")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OriginURL")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ShareCount")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("Summary")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ArticleID");

                    b.ToTable("Articles");
                });

            modelBuilder.Entity("NewsApp.Models.ArticleCategory", b =>
                {
                    b.Property<int>("ArticleID")
                        .HasColumnType("int")
                        .HasColumnOrder(0);

                    b.Property<int>("CategoryID")
                        .HasColumnType("int")
                        .HasColumnOrder(1);

                    b.HasKey("ArticleID", "CategoryID");

                    b.HasIndex("CategoryID");

                    b.ToTable("ArticleCategories");
                });

            modelBuilder.Entity("NewsApp.Models.Category", b =>
                {
                    b.Property<int>("CategoryID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CategoryID"), 1L, 1);

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CategoryID");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("NewsApp.Models.Comment", b =>
                {
                    b.Property<int>("CommentID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CommentID"), 1L, 1);

                    b.Property<int>("ArticleID")
                        .HasColumnType("int");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("CommentID");

                    b.HasIndex("ArticleID");

                    b.HasIndex("UserID");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("NewsApp.Models.SavedArticle", b =>
                {
                    b.Property<int>("SavedArticleID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SavedArticleID"), 1L, 1);

                    b.Property<int>("ArticleID")
                        .HasColumnType("int");

                    b.Property<DateTime>("SavedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("SavedArticleID");

                    b.HasIndex("ArticleID")
                        .IsUnique();

                    b.HasIndex("UserID");

                    b.ToTable("SavedArticles");
                });

            modelBuilder.Entity("NewsApp.Models.User", b =>
                {
                    b.Property<int>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserID"), 1L, 1);

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OAuthID")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OAuthToken")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("Password")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("Password_Hashkey")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.HasKey("UserID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("NewsApp.Models.ArticleCategory", b =>
                {
                    b.HasOne("NewsApp.Models.Article", "Article")
                        .WithMany("ArticleCategories")
                        .HasForeignKey("ArticleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NewsApp.Models.Category", "Category")
                        .WithMany("ArticleCategories")
                        .HasForeignKey("CategoryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Article");

                    b.Navigation("Category");
                });

            modelBuilder.Entity("NewsApp.Models.Comment", b =>
                {
                    b.HasOne("NewsApp.Models.Article", "Article")
                        .WithMany("Comments")
                        .HasForeignKey("ArticleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NewsApp.Models.User", "User")
                        .WithMany("Comments")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Article");

                    b.Navigation("User");
                });

            modelBuilder.Entity("NewsApp.Models.SavedArticle", b =>
                {
                    b.HasOne("NewsApp.Models.Article", "Article")
                        .WithOne()
                        .HasForeignKey("NewsApp.Models.SavedArticle", "ArticleID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("NewsApp.Models.User", "User")
                        .WithMany("SavedArticles")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Article");

                    b.Navigation("User");
                });

            modelBuilder.Entity("NewsApp.Models.Article", b =>
                {
                    b.Navigation("ArticleCategories");

                    b.Navigation("Comments");
                });

            modelBuilder.Entity("NewsApp.Models.Category", b =>
                {
                    b.Navigation("ArticleCategories");
                });

            modelBuilder.Entity("NewsApp.Models.User", b =>
                {
                    b.Navigation("Comments");

                    b.Navigation("SavedArticles");
                });
#pragma warning restore 612, 618
        }
    }
}
