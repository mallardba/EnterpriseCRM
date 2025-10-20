using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameTaskToWorkItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename the Tasks table to WorkItems
            migrationBuilder.RenameTable(
                name: "Tasks",
                newName: "WorkItems");

            // Rename all foreign key constraints
            migrationBuilder.RenameColumn(
                name: "FK_Tasks_Customers_CustomerId",
                table: "WorkItems",
                newName: "FK_WorkItems_Customers_CustomerId");

            migrationBuilder.RenameColumn(
                name: "FK_Tasks_Leads_LeadId",
                table: "WorkItems",
                newName: "FK_WorkItems_Leads_LeadId");

            migrationBuilder.RenameColumn(
                name: "FK_Tasks_Opportunities_OpportunityId",
                table: "WorkItems",
                newName: "FK_WorkItems_Opportunities_OpportunityId");

            migrationBuilder.RenameColumn(
                name: "FK_Tasks_Users_AssignedToUserId",
                table: "WorkItems",
                newName: "FK_WorkItems_Users_AssignedToUserId");

            // Rename all indexes
            migrationBuilder.RenameIndex(
                name: "IX_Tasks_AssignedToUserId",
                table: "WorkItems",
                newName: "IX_WorkItems_AssignedToUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_CustomerId",
                table: "WorkItems",
                newName: "IX_WorkItems_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_DueDate",
                table: "WorkItems",
                newName: "IX_WorkItems_DueDate");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_LeadId",
                table: "WorkItems",
                newName: "IX_WorkItems_LeadId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_OpportunityId",
                table: "WorkItems",
                newName: "IX_WorkItems_OpportunityId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_Status",
                table: "WorkItems",
                newName: "IX_WorkItems_Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rename back from WorkItems to Tasks
            migrationBuilder.RenameTable(
                name: "WorkItems",
                newName: "Tasks");

            // Rename foreign key constraints back
            migrationBuilder.RenameColumn(
                name: "FK_WorkItems_Customers_CustomerId",
                table: "Tasks",
                newName: "FK_Tasks_Customers_CustomerId");

            migrationBuilder.RenameColumn(
                name: "FK_WorkItems_Leads_LeadId",
                table: "Tasks",
                newName: "FK_Tasks_Leads_LeadId");

            migrationBuilder.RenameColumn(
                name: "FK_WorkItems_Opportunities_OpportunityId",
                table: "Tasks",
                newName: "FK_Tasks_Opportunities_OpportunityId");

            migrationBuilder.RenameColumn(
                name: "FK_WorkItems_Users_AssignedToUserId",
                table: "Tasks",
                newName: "FK_Tasks_Users_AssignedToUserId");

            // Rename indexes back
            migrationBuilder.RenameIndex(
                name: "IX_WorkItems_AssignedToUserId",
                table: "Tasks",
                newName: "IX_Tasks_AssignedToUserId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItems_CustomerId",
                table: "Tasks",
                newName: "IX_Tasks_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItems_DueDate",
                table: "Tasks",
                newName: "IX_Tasks_DueDate");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItems_LeadId",
                table: "Tasks",
                newName: "IX_Tasks_LeadId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItems_OpportunityId",
                table: "Tasks",
                newName: "IX_Tasks_OpportunityId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItems_Status",
                table: "Tasks",
                newName: "IX_Tasks_Status");
        }
    }
}
