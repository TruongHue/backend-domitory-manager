using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_dormitory.Migrations
{
    /// <inheritdoc />
    public partial class update01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    idAccount = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    userName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    userCode = table.Column<string>(type: "varchar(65)", maxLength: 65, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    numberPhone = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password = table.Column<string>(type: "varchar(65)", maxLength: 65, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    roles = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.idAccount);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Buildings",
                columns: table => new
                {
                    idBuilding = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nameBuilding = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.idBuilding);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PriceWaterAndElectricities",
                columns: table => new
                {
                    idPrice = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    electricityPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    waterPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    waterLimit = table.Column<int>(type: "int", nullable: false),
                    waterPriceOverLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceWaterAndElectricities", x => x.idPrice);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RegistrationPeriods",
                columns: table => new
                {
                    idRegistrationPeriod = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    actionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    startDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    endDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    semesterStatus = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationPeriods", x => x.idRegistrationPeriod);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InfoStudents",
                columns: table => new
                {
                    idStudent = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idAccount = table.Column<int>(type: "int", nullable: false),
                    gender = table.Column<int>(type: "int", nullable: false),
                    picture = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nameParent = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    parentNumberPhone = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfoStudents", x => x.idStudent);
                    table.ForeignKey(
                        name: "FK_InfoStudents_Accounts_idAccount",
                        column: x => x.idAccount,
                        principalTable: "Accounts",
                        principalColumn: "idAccount",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InfoRoom",
                columns: table => new
                {
                    idRoom = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idBuilding = table.Column<int>(type: "int", nullable: false),
                    nameRoom = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    numberOfBed = table.Column<int>(type: "int", nullable: false),
                    gender = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfoRoom", x => x.idRoom);
                    table.ForeignKey(
                        name: "FK_InfoRoom_Buildings_idBuilding",
                        column: x => x.idBuilding,
                        principalTable: "Buildings",
                        principalColumn: "idBuilding",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    IdFeeedback = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdStudent = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.IdFeeedback);
                    table.ForeignKey(
                        name: "FK_Feedback_InfoStudents_IdStudent",
                        column: x => x.IdStudent,
                        principalTable: "InfoStudents",
                        principalColumn: "idStudent");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ElectricityBill",
                columns: table => new
                {
                    idElectricity = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idRoom = table.Column<int>(type: "int", nullable: false),
                    beforeIndex = table.Column<int>(type: "int", nullable: false),
                    afterIndex = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    dateOfRecord = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    total = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectricityBill", x => x.idElectricity);
                    table.ForeignKey(
                        name: "FK_ElectricityBill_InfoRoom_idRoom",
                        column: x => x.idRoom,
                        principalTable: "InfoRoom",
                        principalColumn: "idRoom",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RegisterRoom",
                columns: table => new
                {
                    IdRegister = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idStudent = table.Column<int>(type: "int", nullable: false),
                    idRoom = table.Column<int>(type: "int", nullable: false),
                    idRegistrationPeriod = table.Column<int>(type: "int", nullable: false),
                    startDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    endDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    actionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    paymentStatus = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisterRoom", x => x.IdRegister);
                    table.ForeignKey(
                        name: "FK_RegisterRoom_InfoRoom_idRoom",
                        column: x => x.idRoom,
                        principalTable: "InfoRoom",
                        principalColumn: "idRoom",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegisterRoom_InfoStudents_idStudent",
                        column: x => x.idStudent,
                        principalTable: "InfoStudents",
                        principalColumn: "idStudent",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoomBill",
                columns: table => new
                {
                    idRoomBill = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idRoom = table.Column<int>(type: "int", nullable: false),
                    dailyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    priceYear = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    dateOfRecord = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomBill", x => x.idRoomBill);
                    table.ForeignKey(
                        name: "FK_RoomBill_InfoRoom_idRoom",
                        column: x => x.idRoom,
                        principalTable: "InfoRoom",
                        principalColumn: "idRoom",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WaterBill",
                columns: table => new
                {
                    idWater = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    idRoom = table.Column<int>(type: "int", nullable: false),
                    beforeIndex = table.Column<int>(type: "int", nullable: false),
                    afterIndex = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    indexLimit = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    priceLimit = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    dateOfRecord = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    total = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaterBill", x => x.idWater);
                    table.ForeignKey(
                        name: "FK_WaterBill_InfoRoom_idRoom",
                        column: x => x.idRoom,
                        principalTable: "InfoRoom",
                        principalColumn: "idRoom",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ElectricityBill_idRoom",
                table: "ElectricityBill",
                column: "idRoom");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_IdStudent",
                table: "Feedback",
                column: "IdStudent");

            migrationBuilder.CreateIndex(
                name: "IX_InfoRoom_idBuilding",
                table: "InfoRoom",
                column: "idBuilding");

            migrationBuilder.CreateIndex(
                name: "IX_InfoStudents_email",
                table: "InfoStudents",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InfoStudents_idAccount",
                table: "InfoStudents",
                column: "idAccount",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisterRoom_idRoom",
                table: "RegisterRoom",
                column: "idRoom");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterRoom_idStudent",
                table: "RegisterRoom",
                column: "idStudent");

            migrationBuilder.CreateIndex(
                name: "IX_RoomBill_idRoom",
                table: "RoomBill",
                column: "idRoom");

            migrationBuilder.CreateIndex(
                name: "IX_WaterBill_idRoom",
                table: "WaterBill",
                column: "idRoom");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElectricityBill");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "PriceWaterAndElectricities");

            migrationBuilder.DropTable(
                name: "RegisterRoom");

            migrationBuilder.DropTable(
                name: "RegistrationPeriods");

            migrationBuilder.DropTable(
                name: "RoomBill");

            migrationBuilder.DropTable(
                name: "WaterBill");

            migrationBuilder.DropTable(
                name: "InfoStudents");

            migrationBuilder.DropTable(
                name: "InfoRoom");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Buildings");
        }
    }
}
