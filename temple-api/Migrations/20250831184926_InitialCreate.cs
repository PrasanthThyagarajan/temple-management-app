using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TempleApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BaseEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    Discriminator = table.Column<string>(type: "TEXT", maxLength: 21, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Gender = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    TempleId = table.Column<int>(type: "INTEGER", nullable: true),
                    Donation_TempleId = table.Column<int>(type: "INTEGER", nullable: true),
                    DevoteeId = table.Column<int>(type: "INTEGER", nullable: true),
                    DonorName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DonationType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Purpose = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true, defaultValue: "Pending"),
                    DonationDate = table.Column<DateTime>(type: "TEXT", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ReceiptNumber = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Event_TempleId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Event_Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true, defaultValue: "Scheduled"),
                    Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Organizer = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Event_Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Event_Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    MaxAttendees = table.Column<int>(type: "INTEGER", nullable: true),
                    EntryFee = table.Column<decimal>(type: "TEXT", nullable: true),
                    EventId = table.Column<int>(type: "INTEGER", nullable: true),
                    EventRegistration_DevoteeId = table.Column<int>(type: "INTEGER", nullable: true),
                    AttendeeName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EventRegistration_Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    EventRegistration_Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    EventRegistration_Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true, defaultValue: "Registered"),
                    RegistrationDate = table.Column<DateTime>(type: "TEXT", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    SpecialRequirements = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AmountPaid = table.Column<decimal>(type: "TEXT", nullable: true),
                    PaymentMethod = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Service_TempleId = table.Column<int>(type: "INTEGER", nullable: true),
                    Service_Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ServiceType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Service_Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: true, defaultValue: true),
                    Duration = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Requirements = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    MaxBookingsPerDay = table.Column<int>(type: "INTEGER", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    Temple_Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Temple_Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Temple_City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Temple_State = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Temple_PostalCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Temple_Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Temple_Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Temple_Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Deity = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EstablishedDate = table.Column<DateTime>(type: "TEXT", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_DevoteeId",
                        column: x => x.DevoteeId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_Donation_TempleId",
                        column: x => x.Donation_TempleId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_EventId",
                        column: x => x.EventId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_EventRegistration_DevoteeId",
                        column: x => x.EventRegistration_DevoteeId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_Event_TempleId",
                        column: x => x.Event_TempleId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_Service_TempleId",
                        column: x => x.Service_TempleId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseEntity_BaseEntity_TempleId",
                        column: x => x.TempleId,
                        principalTable: "BaseEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_DevoteeId",
                table: "BaseEntity",
                column: "DevoteeId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_Donation_TempleId",
                table: "BaseEntity",
                column: "Donation_TempleId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_Event_TempleId",
                table: "BaseEntity",
                column: "Event_TempleId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_EventId",
                table: "BaseEntity",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_EventRegistration_DevoteeId",
                table: "BaseEntity",
                column: "EventRegistration_DevoteeId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_Service_TempleId",
                table: "BaseEntity",
                column: "Service_TempleId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEntity_TempleId",
                table: "BaseEntity",
                column: "TempleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaseEntity");
        }
    }
}
