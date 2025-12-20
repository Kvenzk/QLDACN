using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using QLDACN.Data;

namespace QLDACN.Migrations
{
    [DbContext(typeof(RecyclingDbContext))]
    [Migration("20251212050000_AddWasteTypeCategorySeed")]
    public partial class AddWasteTypeCategorySeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "WasteTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Khác");

            var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            // Seed types if not exist
            migrationBuilder.Sql($@"
IF NOT EXISTS(SELECT 1 FROM WasteTypes WHERE Name = 'PET (Nhựa số 1)')
INSERT INTO WasteTypes(Name, Category, Description, PointPerUnit, Unit, Status, CreatedAt)
VALUES('PET (Nhựa số 1)', 'Nhựa', 'Chai nước suối, nước ngọt, dầu ăn', 20, 'kg', 'Active', '{now}');

IF NOT EXISTS(SELECT 1 FROM WasteTypes WHERE Name = 'HDPE (Nhựa số 2)')
INSERT INTO WasteTypes(Name, Category, Description, PointPerUnit, Unit, Status, CreatedAt)
VALUES('HDPE (Nhựa số 2)', 'Nhựa', 'Chai sữa, sữa tắm, dầu gội', 30, 'kg', 'Active', '{now}');

IF NOT EXISTS(SELECT 1 FROM WasteTypes WHERE Name = 'PP (Nhựa số 5)')
INSERT INTO WasteTypes(Name, Category, Description, PointPerUnit, Unit, Status, CreatedAt)
VALUES('PP (Nhựa số 5)', 'Nhựa', 'Hộp nhựa cứng, cốc nhựa', 30, 'kg', 'Active', '{now}');

IF NOT EXISTS(SELECT 1 FROM WasteTypes WHERE Name = 'Giấy văn phòng (Loại 1)')
INSERT INTO WasteTypes(Name, Category, Description, PointPerUnit, Unit, Status, CreatedAt)
VALUES('Giấy văn phòng (Loại 1)', 'Giấy & Bìa', 'Giấy in, photo, nháp', 100, 'kg', 'Active', '{now}');

IF NOT EXISTS(SELECT 1 FROM WasteTypes WHERE Name = 'Báo và tạp chí')
INSERT INTO WasteTypes(Name, Category, Description, PointPerUnit, Unit, Status, CreatedAt)
VALUES('Báo và tạp chí', 'Giấy & Bìa', 'Báo, tạp chí cũ, sách giáo khoa', 50, 'kg', 'Active', '{now}');

IF NOT EXISTS(SELECT 1 FROM WasteTypes WHERE Name = 'Bìa cát-tông')
INSERT INTO WasteTypes(Name, Category, Description, PointPerUnit, Unit, Status, CreatedAt)
VALUES('Bìa cát-tông', 'Giấy & Bìa', 'Thùng, hộp carton sóng', 60, 'kg', 'Active', '{now}');

IF NOT EXISTS(SELECT 1 FROM WasteTypes WHERE Name = 'Lon nhôm')
INSERT INTO WasteTypes(Name, Category, Description, PointPerUnit, Unit, Status, CreatedAt)
VALUES('Lon nhôm', 'Kim loại', 'Lon bia, lon nước ngọt', 60, 'kg', 'Active', '{now}');

IF NOT EXISTS(SELECT 1 FROM WasteTypes WHERE Name = 'Lon thiếc')
INSERT INTO WasteTypes(Name, Category, Description, PointPerUnit, Unit, Status, CreatedAt)
VALUES('Lon thiếc', 'Kim loại', 'Lon thực phẩm, hộp sữa bột', 70, 'kg', 'Active', '{now}');

IF NOT EXISTS(SELECT 1 FROM WasteTypes WHERE Name = 'Chai và lọ thủy tinh')
INSERT INTO WasteTypes(Name, Category, Description, PointPerUnit, Unit, Status, CreatedAt)
VALUES('Chai và lọ thủy tinh', 'Thủy tinh', 'Chai rượu, nước tương, lọ mứt', 20, 'kg', 'Active', '{now}');

IF NOT EXISTS(SELECT 1 FROM WasteTypes WHERE Name = 'Túi ni-lông')
INSERT INTO WasteTypes(Name, Category, Description, PointPerUnit, Unit, Status, CreatedAt)
VALUES('Túi ni-lông', 'Khác', 'Túi sạch, không dính bẩn', 70, 'kg', 'Active', '{now}');

IF NOT EXISTS(SELECT 1 FROM WasteTypes WHERE Name = 'Hộp sữa giấy')
INSERT INTO WasteTypes(Name, Category, Description, PointPerUnit, Unit, Status, CreatedAt)
VALUES('Hộp sữa giấy', 'Khác', 'Hộp sữa tươi, nước trái cây', 80, 'kg', 'Active', '{now}');








            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM WasteTypes WHERE Name IN (
    'PET (Nhựa số 1)', 'HDPE (Nhựa số 2)', 'PP (Nhựa số 5)',
    'Giấy văn phòng (Loại 1)', 'Báo và tạp chí', 'Bìa cát-tông',
    'Lon nhôm', 'Lon thiếc', 'Chai và lọ thủy tinh', 'Túi ni-lông', 'Hộp sữa giấy'
);
            ");
            migrationBuilder.DropColumn(
                name: "Category",
                table: "WasteTypes");
        }
    }
}
