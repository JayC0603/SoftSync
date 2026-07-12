using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable

namespace SoftSync.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelAfterGameMerge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op migration: the DataProtectionKeys table is already created by
            // 20260710001000_AddDataProtectionKeys. This migration syncs the EF
            // target model snapshot after the dev merge so startup MigrateAsync
            // no longer throws PendingModelChangesWarning on Render.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op.
        }
    }
}
