using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ServiceDeskSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ContactTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactTypes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MiddleName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TechStacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechStacks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ContactInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    ContactTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactInfos_ContactTypes_ContactTypeId",
                        column: x => x.ContactTypeId,
                        principalTable: "ContactTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContactInfos_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Login = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PersonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrentVersion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TechStackId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_TechStacks_TechStackId",
                        column: x => x.TechStackId,
                        principalTable: "TechStacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StepsToReproduce = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AffectedVersion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Environment = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    DeveloperId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_DeveloperId",
                        column: x => x.DeveloperId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FileName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FilePath = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TicketId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Message = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsInternal = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "ContactTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Email" },
                    { 2, "Phone" },
                    { 3, "Telegram" }
                });

            migrationBuilder.InsertData(
                table: "People",
                columns: new[] { "Id", "FirstName", "LastName", "MiddleName" },
                values: new object[,]
                {
                    { 1, "System", "Administrator", null },
                    { 2, "Олександр", "Коваленко", "Петрович" },
                    { 3, "Марія", "Шевченко", "Іванівна" },
                    { 4, "John", "Smith", null },
                    { 5, "Андрій", "Бондаренко", "Олегович" }
                });

            migrationBuilder.InsertData(
                table: "TechStacks",
                columns: new[] { "Id", "Name", "Type" },
                values: new object[,]
                {
                    { 1, "C# / .NET", "Desktop Software" },
                    { 2, "ASP.NET Core / Blazor", "Web Application" },
                    { 3, "Android / Kotlin", "Mobile Application" },
                    { 4, "C++ / Embedded", "Hardware Firmware" },
                    { 5, "Python / Django", "Web Service" },
                    { 6, "Network Infrastructure", "Hardware" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CurrentVersion", "Description", "Name", "TechStackId" },
                values: new object[,]
                {
                    { 1, "3.2.1", "Програма для ведення бухгалтерського обліку підприємства", "Бухгалтерія Pro", 1 },
                    { 2, "2.5.0", "Система управління складом та інвентаризацією", "Warehouse Manager", 1 },
                    { 3, "1.8.3", "Корпоративний портал для управління персоналом", "HR Portal", 2 },
                    { 4, "4.1.0", "Платформа для онлайн-продажів з інтеграцією платіжних систем", "E-Commerce Platform", 2 },
                    { 5, "2.0.5", "Мобільний додаток для роботи з клієнтською базою", "Mobile CRM", 3 },
                    { 6, "1.4.2", "Прошивка для касових терміналів", "POS Terminal v2", 4 },
                    { 7, "2.1.0", "Контролер системи контролю доступу", "Smart Lock Controller", 4 },
                    { 8, "5.0.1", "Корпоративний маршрутизатор з підтримкою VPN", "Office Router Pro", 6 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Login", "PasswordHash", "PersonId", "Role" },
                values: new object[,]
                {
                    { 1, "admin", "admin123", 1, "Admin" },
                    { 2, "o.kovalenko", "dev123", 2, "Developer" },
                    { 3, "m.shevchenko", "client123", 3, "Client" },
                    { 4, "j.smith", "client123", 4, "Client" },
                    { 5, "a.bondarenko", "dev123", 5, "Developer" }
                });

            migrationBuilder.InsertData(
                table: "Tickets",
                columns: new[] { "Id", "AffectedVersion", "AuthorId", "CreatedAt", "Description", "DeveloperId", "Environment", "Priority", "ProductId", "Status", "StepsToReproduce", "Title" },
                values: new object[,]
                {
                    { 1, "3.2.1", 3, new DateTime(2024, 4, 15, 10, 30, 0, 0, DateTimeKind.Unspecified), "При спробі сформувати звіт з ПДВ за останній квартал програма видає помилку 'Invalid date range'.", 2, "Windows 11 / Chrome", "High", 1, "In Progress", "1. Відкрити розділ Звіти\n2. Вибрати 'Звіт ПДВ'\n3. Встановити період: 01.01.2024 - 31.03.2024\n4. Натиснути 'Сформувати'", "Помилка при формуванні звіту ПДВ" },
                    { 2, "3.2.1", 3, new DateTime(2024, 4, 18, 14, 0, 0, 0, DateTimeKind.Unspecified), "Функція експорту документів в Excel формат не працює - файл створюється порожнім.", null, "Windows 10 / Edge", "Medium", 1, "New", "1. Відкрити будь-який документ\n2. Натиснути Експорт -> Excel\n3. Зберегти файл", "Не працює експорт в Excel" },
                    { 3, "2.5.0", 4, new DateTime(2024, 4, 10, 9, 0, 0, 0, DateTimeKind.Unspecified), "After synchronizing with barcode scanners, the inventory count shows incorrect values for some items.", 5, "Windows Server 2022", "Critical", 2, "Code Review", "1. Perform physical inventory count\n2. Sync data from handheld scanners\n3. Compare totals in system", "Inventory count mismatch after sync" },
                    { 4, "2.5.0", 4, new DateTime(2024, 4, 12, 11, 30, 0, 0, DateTimeKind.Unspecified), "Product search takes more than 10 seconds when warehouse has more than 50,000 items.", 2, "Windows 11", "Medium", 2, "In Progress", "1. Open product search\n2. Enter partial product name\n3. Wait for results", "Slow search performance" },
                    { 5, "1.8.3", 3, new DateTime(2024, 4, 5, 16, 0, 0, 0, DateTimeKind.Unspecified), "В особистому кабінеті працівника не відображається залишок днів відпустки за поточний рік.", 5, "Windows 10 / Firefox", "High", 3, "Testing", "1. Увійти в особистий кабінет\n2. Перейти в розділ 'Відпустки'\n3. Перевірити баланс днів", "Не відображаються дні відпустки" },
                    { 6, "1.8.3", 4, new DateTime(2024, 4, 20, 8, 45, 0, 0, DateTimeKind.Unspecified), "When generating PDF reports for employee attendance, the system throws an error.", null, "macOS Ventura / Safari", "Medium", 3, "New", "1. Go to Reports section\n2. Select Attendance Report\n3. Click Generate PDF", "PDF generation fails for reports" },
                    { 7, "4.1.0", 3, new DateTime(2024, 4, 19, 12, 0, 0, 0, DateTimeKind.Unspecified), "Клієнти не можуть завершити оплату через LiqPay - з'являється помилка 'Payment gateway timeout'.", 2, "Android / Chrome Mobile", "Critical", 4, "In Progress", "1. Додати товар в кошик\n2. Перейти до оформлення\n3. Вибрати оплату LiqPay\n4. Спробувати оплатити", "Помилка оплати через LiqPay" },
                    { 8, "4.0.5", 4, new DateTime(2024, 3, 25, 14, 30, 0, 0, DateTimeKind.Unspecified), "Items added to cart as guest user disappear after logging in to account.", 5, "iOS / Safari", "High", 4, "Done", "1. Browse as guest\n2. Add items to cart\n3. Click Login\n4. Enter credentials\n5. Check cart", "Cart items disappear after login" },
                    { 9, "2.0.5", 4, new DateTime(2024, 4, 21, 9, 15, 0, 0, DateTimeKind.Unspecified), "Users are not receiving push notifications for new tasks assigned to them.", null, "Android 14", "High", 5, "New", "1. Assign task to user\n2. Wait for notification\n3. Check device - no notification received", "Push notifications not working" },
                    { 10, "2.0.5", 3, new DateTime(2024, 4, 17, 11, 0, 0, 0, DateTimeKind.Unspecified), "При синхронізації великої кількості контактів (>1000) додаток зависає і потребує перезапуску.", 2, "iOS 17", "Medium", 5, "In Progress", "1. Відкрити налаштування\n2. Натиснути 'Синхронізувати контакти'\n3. Дочекатися завершення", "Синхронізація контактів зависає" },
                    { 11, "1.4.2", 4, new DateTime(2024, 4, 8, 7, 30, 0, 0, DateTimeKind.Unspecified), "The receipt printer loses connection randomly during operation, requiring terminal restart.", 5, "POS Terminal Hardware v2", "Critical", 6, "Code Review", "1. Process several transactions\n2. After ~20-30 receipts, printer stops responding", "Printer connection lost randomly" },
                    { 12, "2.1.0", 3, new DateTime(2024, 4, 22, 8, 0, 0, 0, DateTimeKind.Unspecified), "Система контролю доступу не реагує на NFC картки після останнього оновлення прошивки.", 5, "Smart Lock Hardware v3", "Critical", 7, "In Progress", "1. Піднести NFC картку до зчитувача\n2. Очікувати відкриття замка\n3. Замок не реагує", "Не працює відкриття через NFC" },
                    { 13, "5.0.1", 4, new DateTime(2024, 4, 14, 15, 0, 0, 0, DateTimeKind.Unspecified), "VPN connections are dropping when more than 50 concurrent users are connected.", 2, "Office Router Pro Hardware", "High", 8, "Testing", "1. Establish 50+ VPN connections\n2. Generate network traffic\n3. Monitor connection stability", "VPN tunnel drops under heavy load" },
                    { 14, "5.0.0", 4, new DateTime(2024, 3, 20, 10, 0, 0, 0, DateTimeKind.Unspecified), "New firewall rules added through web interface are not being applied until device restart.", 5, "Office Router Pro Hardware", "Medium", 8, "Done", "1. Login to web interface\n2. Add new firewall rule\n3. Save configuration\n4. Test rule - not working", "Firewall rules not applying" },
                    { 15, "3.2.1", 3, new DateTime(2024, 4, 23, 12, 0, 0, 0, DateTimeKind.Unspecified), "Було б зручно мати можливість переключатися на темну тему в інтерфейсі програми.", null, "Windows 11", "Low", 1, "New", "Це запит на нову функцію, а не баг.", "Запит на додавання темної теми" }
                });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "AuthorId", "CreatedAt", "IsInternal", "Message", "TicketId" },
                values: new object[,]
                {
                    { 1, 2, new DateTime(2024, 4, 15, 14, 0, 0, 0, DateTimeKind.Unspecified), false, "Дякую за звернення. Відтворив проблему, почав аналіз.", 1 },
                    { 2, 2, new DateTime(2024, 4, 15, 15, 30, 0, 0, DateTimeKind.Unspecified), true, "Проблема в парсингу дат - треба виправити формат для українського регіону.", 1 },
                    { 3, 4, new DateTime(2024, 4, 10, 11, 0, 0, 0, DateTimeKind.Unspecified), false, "This is blocking our monthly inventory audit. Please prioritize!", 3 },
                    { 4, 5, new DateTime(2024, 4, 11, 16, 0, 0, 0, DateTimeKind.Unspecified), true, "Found the issue - race condition in sync handler. Fix ready for review.", 3 },
                    { 5, 5, new DateTime(2024, 4, 8, 10, 0, 0, 0, DateTimeKind.Unspecified), false, "Виправлення готове, передано на тестування.", 5 },
                    { 6, 2, new DateTime(2024, 4, 19, 14, 0, 0, 0, DateTimeKind.Unspecified), false, "Зв'язався з технічною підтримкою LiqPay. Очікую відповідь.", 7 },
                    { 7, 2, new DateTime(2024, 4, 19, 16, 30, 0, 0, DateTimeKind.Unspecified), true, "LiqPay змінили API endpoint. Потрібно оновити конфігурацію.", 7 },
                    { 8, 5, new DateTime(2024, 3, 28, 12, 0, 0, 0, DateTimeKind.Unspecified), false, "Fixed by merging guest cart with user cart on login. Deployed to production.", 8 },
                    { 9, 5, new DateTime(2024, 4, 9, 14, 0, 0, 0, DateTimeKind.Unspecified), true, "Issue is in the USB driver timeout handling. Preparing hotfix.", 11 },
                    { 10, 3, new DateTime(2024, 4, 22, 8, 30, 0, 0, DateTimeKind.Unspecified), false, "Проблема критична - офіс не може працювати. Потрібно відкотити прошивку!", 12 },
                    { 11, 5, new DateTime(2024, 4, 22, 9, 45, 0, 0, DateTimeKind.Unspecified), false, "Тимчасово відкотив до версії 2.0.8. Аналізую зміни в 2.1.0.", 12 },
                    { 12, 2, new DateTime(2024, 4, 16, 11, 0, 0, 0, DateTimeKind.Unspecified), false, "Increased buffer size and connection pool. Testing in progress.", 13 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_TicketId",
                table: "Attachments",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorId",
                table: "Comments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TicketId",
                table: "Comments",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInfos_ContactTypeId",
                table: "ContactInfos",
                column: "ContactTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInfos_PersonId",
                table: "ContactInfos",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TechStackId",
                table: "Products",
                column: "TechStackId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AuthorId",
                table: "Tickets",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_DeveloperId",
                table: "Tickets",
                column: "DeveloperId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ProductId",
                table: "Tickets",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Login",
                table: "Users",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PersonId",
                table: "Users",
                column: "PersonId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "ContactInfos");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "ContactTypes");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "TechStacks");

            migrationBuilder.DropTable(
                name: "People");
        }
    }
}
