using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DMO.Alpha.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminAssociations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderUserId = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    AssociatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminAssociations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tools",
                columns: table => new
                {
                    ToolId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: false),
                    Lot = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tools", x => x.ToolId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    OperatorId = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    ProviderUserId = table.Column<string>(type: "text", nullable: false),
                    RequiresPasswordChange = table.Column<bool>(type: "boolean", nullable: false),
                    IsStandBy = table.Column<bool>(type: "boolean", nullable: false),
                    TemplateName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobOns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProductionNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MachineId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobOns_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BqContexts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobOnId = table.Column<int>(type: "integer", nullable: false),
                    ToolId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BqContexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BqContexts_JobOns_JobOnId",
                        column: x => x.JobOnId,
                        principalTable: "JobOns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CmContexts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobOnId = table.Column<int>(type: "integer", nullable: false),
                    ToolId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmContexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmContexts_JobOns_JobOnId",
                        column: x => x.JobOnId,
                        principalTable: "JobOns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MfContexts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobOnId = table.Column<int>(type: "integer", nullable: false),
                    ToolId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MfContexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MfContexts_JobOns_JobOnId",
                        column: x => x.JobOnId,
                        principalTable: "JobOns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BqRepairTraces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ToolId = table.Column<string>(type: "text", nullable: false),
                    BqContextId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BqRepairTraces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BqRepairTraces_BqContexts_BqContextId",
                        column: x => x.BqContextId,
                        principalTable: "BqContexts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BqRepairTraces_Tools_ToolId",
                        column: x => x.ToolId,
                        principalTable: "Tools",
                        principalColumn: "ToolId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BqMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BqRepairTraceId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Discrepancy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BqMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BqMovements_BqRepairTraces_BqRepairTraceId",
                        column: x => x.BqRepairTraceId,
                        principalTable: "BqRepairTraces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminAssociations_Email",
                table: "AdminAssociations",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BqContexts_JobOnId",
                table: "BqContexts",
                column: "JobOnId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BqMovements_BqRepairTraceId",
                table: "BqMovements",
                column: "BqRepairTraceId");

            migrationBuilder.CreateIndex(
                name: "IX_BqRepairTraces_BqContextId",
                table: "BqRepairTraces",
                column: "BqContextId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BqRepairTraces_ToolId",
                table: "BqRepairTraces",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_CmContexts_JobOnId",
                table: "CmContexts",
                column: "JobOnId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobOns_MachineId",
                table: "JobOns",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOns_ProductionNumber",
                table: "JobOns",
                column: "ProductionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MfContexts_JobOnId",
                table: "MfContexts",
                column: "JobOnId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_OperatorId",
                table: "Users",
                column: "OperatorId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminAssociations");

            migrationBuilder.DropTable(
                name: "BqMovements");

            migrationBuilder.DropTable(
                name: "CmContexts");

            migrationBuilder.DropTable(
                name: "MfContexts");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "BqRepairTraces");

            migrationBuilder.DropTable(
                name: "BqContexts");

            migrationBuilder.DropTable(
                name: "Tools");

            migrationBuilder.DropTable(
                name: "JobOns");

            migrationBuilder.DropTable(
                name: "Machines");
        }
    }
}
