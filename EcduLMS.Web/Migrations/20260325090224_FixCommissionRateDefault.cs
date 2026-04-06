using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduLMS.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixCommissionRateDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix: Set CommissionRate to 0.30 for all users who still have the old default of 0
            migrationBuilder.Sql(
                "UPDATE AspNetUsers SET CommissionRate = 0.30 WHERE CommissionRate = 0 AND Id IN (SELECT DISTINCT InstructorId FROM Courses);"
            );

            // Fix: Recalculate InstructorEarning records where PlatformFeeRate was 0 (bad data from old migration default)
            // We recalculate based on the now-corrected CommissionRate of the instructor
            migrationBuilder.Sql(@"
                UPDATE InstructorEarnings ie
                JOIN AspNetUsers u ON u.Id = ie.InstructorId
                SET
                    ie.PlatformFeeRate = u.CommissionRate,
                    ie.PlatformFee     = ROUND(ie.GrossAmount * u.CommissionRate, 0),
                    ie.NetAmount       = ie.GrossAmount - ROUND(ie.GrossAmount * u.CommissionRate, 0)
                WHERE ie.PlatformFeeRate = 0 AND ie.GrossAmount > 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reset to 0 (original broken state — not recommended to rollback)
            migrationBuilder.Sql(
                "UPDATE AspNetUsers SET CommissionRate = 0 WHERE CommissionRate = 0.30;"
            );
            migrationBuilder.Sql(
                "UPDATE InstructorEarnings SET PlatformFeeRate = 0, PlatformFee = 0, NetAmount = GrossAmount WHERE PlatformFeeRate > 0;"
            );
        }
    }
}
