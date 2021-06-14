using Microsoft.EntityFrameworkCore.Migrations;

namespace FFRaidAnalytics.Migrations
{
    public partial class CreateFightIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ReportFights_EncounterId",
                table: "ReportFights",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFights_StartTime",
                table: "ReportFights",
                column: "StartTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReportFights_EncounterId",
                table: "ReportFights");

            migrationBuilder.DropIndex(
                name: "IX_ReportFights_StartTime",
                table: "ReportFights");
        }
    }
}
