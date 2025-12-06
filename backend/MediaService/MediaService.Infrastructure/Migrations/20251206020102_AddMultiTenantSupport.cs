using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenantSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "media_assets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DealerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Context = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProcessingError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StorageKey = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CdnUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MediaType = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    PageCount = table.Column<int>(type: "integer", nullable: true),
                    Author = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsSearchable = table.Column<bool>(type: "boolean", nullable: true),
                    Language = table.Column<string>(type: "text", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    HashSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: true),
                    AltText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    VideoMedia_Width = table.Column<int>(type: "integer", nullable: true),
                    VideoMedia_Height = table.Column<int>(type: "integer", nullable: true),
                    VideoCodec = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AudioCodec = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FrameRate = table.Column<double>(type: "double precision", nullable: true),
                    Bitrate = table.Column<int>(type: "integer", nullable: true),
                    StorageKeyInput = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    StorageKeyHls = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    HasHlsStream = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "media_variants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MediaAssetId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CdnUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Format = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Quality = table.Column<int>(type: "integer", nullable: true),
                    VideoProfile = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Bitrate = table.Column<int>(type: "integer", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_variants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_media_variants_media_assets_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "media_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_ContentType",
                table: "media_assets",
                column: "ContentType");

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_Context",
                table: "media_assets",
                column: "Context");

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_CreatedAt",
                table: "media_assets",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_Duration",
                table: "media_assets",
                column: "Duration");

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_HasHlsStream",
                table: "media_assets",
                column: "HasHlsStream");

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_Height",
                table: "media_assets",
                column: "Height");

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_IsPrimary",
                table: "media_assets",
                column: "IsPrimary");

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_OwnerId",
                table: "media_assets",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_OwnerId_Context",
                table: "media_assets",
                columns: new[] { "OwnerId", "Context" });

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_Status",
                table: "media_assets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_Type",
                table: "media_assets",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_Width",
                table: "media_assets",
                column: "Width");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAsset_DealerId",
                table: "media_assets",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_media_variants_Height",
                table: "media_variants",
                column: "Height");

            migrationBuilder.CreateIndex(
                name: "IX_media_variants_MediaAssetId",
                table: "media_variants",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_media_variants_MediaAssetId_Name",
                table: "media_variants",
                columns: new[] { "MediaAssetId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_variants_Quality",
                table: "media_variants",
                column: "Quality");

            migrationBuilder.CreateIndex(
                name: "IX_media_variants_StorageKey",
                table: "media_variants",
                column: "StorageKey");

            migrationBuilder.CreateIndex(
                name: "IX_media_variants_Width",
                table: "media_variants",
                column: "Width");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media_variants");

            migrationBuilder.DropTable(
                name: "media_assets");
        }
    }
}
