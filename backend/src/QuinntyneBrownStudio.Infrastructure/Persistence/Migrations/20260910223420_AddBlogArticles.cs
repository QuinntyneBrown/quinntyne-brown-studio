using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuinntyneBrownStudio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogArticles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DigitalAssets",
                columns: table => new
                {
                    DigitalAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalAssets", x => x.DigitalAssetId);
                    table.ForeignKey(
                        name: "FK_DigitalAssets_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    ArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Abstract = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeaturedImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Published = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DatePublished = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadingTimeMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.ArticleId);
                    table.ForeignKey(
                        name: "FK_Articles_DigitalAssets_FeaturedImageId",
                        column: x => x.FeaturedImageId,
                        principalTable: "DigitalAssets",
                        principalColumn: "DigitalAssetId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_CreatedAt",
                table: "Articles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_FeaturedImageId",
                table: "Articles",
                column: "FeaturedImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Published_DatePublished",
                table: "Articles",
                columns: new[] { "Published", "DatePublished" });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Slug",
                table: "Articles",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssets_ContentType",
                table: "DigitalAssets",
                column: "ContentType");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssets_CreatedBy",
                table: "DigitalAssets",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssets_StoredFileName",
                table: "DigitalAssets",
                column: "StoredFileName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "DigitalAssets");
        }
    }
}
