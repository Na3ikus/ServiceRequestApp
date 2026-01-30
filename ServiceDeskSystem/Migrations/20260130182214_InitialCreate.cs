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
                    { 1, "Admin", "Administrator", "System" },
                    { 2, "Іван", "Петренко", "Олександрович" },
                    { 3, "Марія", "Коваленко", "Іванівна" }
                });

            migrationBuilder.InsertData(
                table: "TechStacks",
                columns: new[] { "Id", "Name", "Type" },
                values: new object[,]
                {
                    { 1, "C# / .NET", "Backend" },
                    { 2, "Blazor", "Web" },
                    { 3, "React", "Web" },
                    { 4, "SQL Server", "Database" },
                    { 5, "C++ STM32", "Embedded" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CurrentVersion", "Description", "Name", "TechStackId" },
                values: new object[,]
                {
                    { 1, "4.0.8.5", "Vel id ab sit voluptatem adipisci et voluptates dolores consectetur.", "Tasty Fresh Fish", 2 },
                    { 2, "8.7.0.5", "Accusantium dolor alias facere quidem nobis eveniet occaecati eos mollitia.", "Small Frozen Keyboard", 1 },
                    { 3, "4.7.1.0", "Quibusdam recusandae necessitatibus quos consectetur possimus fugit sit eum neque.", "Unbranded Concrete Shirt", 3 },
                    { 4, "1.8.9.9", "Quod inventore aut commodi sed et illo aperiam inventore id.", "Licensed Frozen Keyboard", 2 },
                    { 5, "4.8.2.2", "Enim provident officia dolore rerum qui ullam id ipsa odio.", "Refined Concrete Chair", 4 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Login", "PasswordHash", "PersonId", "Role" },
                values: new object[,]
                {
                    { 1, "admin", "$2a$11$dummyHashForTestingPurposesOnly123456", 1, "Admin" },
                    { 2, "developer", "$2a$11$dummyHashForTestingPurposesOnly123456", 2, "Developer" },
                    { 3, "client", "$2a$11$dummyHashForTestingPurposesOnly123456", 3, "Client" }
                });

            migrationBuilder.InsertData(
                table: "Tickets",
                columns: new[] { "Id", "AffectedVersion", "AuthorId", "CreatedAt", "Description", "DeveloperId", "Environment", "Priority", "ProductId", "Status", "StepsToReproduce", "Title" },
                values: new object[,]
                {
                    { 1, "1.0.5.8", 3, new DateTime(2025, 12, 14, 6, 17, 31, 86, DateTimeKind.Local).AddTicks(2347), "Voluptatem adipisci et. Dolores consectetur praesentium beatae nulla similique aliquam consequatur earum nesciunt. Dolor alias facere.", 2, "macOS Ventura / Chrome", "High", 1, "Testing", "Nobis eveniet occaecati eos mollitia at possimus quasi.\nExplicabo sint quia voluptate quibusdam recusandae necessitatibus.\nConsectetur possimus fugit sit eum neque.", "Maiores voluptates maxime vel id" },
                    { 2, "3.8.5.5", 3, new DateTime(2025, 10, 30, 4, 10, 21, 135, DateTimeKind.Local).AddTicks(7933), "Sed accusamus maiores non ea pariatur non aut. Provident officia dolore rerum qui. Id ipsa odio odio voluptas.", 2, "Windows 11 / Edge", "Medium", 3, "In Progress", "Magnam harum delectus quia.\nNemo iusto vel quaerat.\nNesciunt cumque autem et sint dolor.", "Commodi sed et illo aperiam" },
                    { 3, "5.3.1.9", 3, new DateTime(2025, 9, 19, 15, 34, 16, 399, DateTimeKind.Local).AddTicks(376), "Accusamus labore sit facere non ut omnis. Aut explicabo molestiae perspiciatis. Iure repellendus natus quos aut.", 2, "Windows 10 / Firefox", "Critical", 5, "In Progress", "Eaque cum optio sed.\nSoluta minus expedita quis et tempore.\nError atque at velit minima nesciunt quibusdam consequuntur voluptas.", "Veritatis vel qui laborum deserunt" },
                    { 4, "7.4.3.5", 3, new DateTime(2025, 11, 3, 2, 22, 22, 697, DateTimeKind.Local).AddTicks(1046), "Architecto deleniti aut ea omnis suscipit ut. Veritatis velit amet molestias in optio aut quis perferendis. Nesciunt et enim qui culpa.", 2, "Windows 10 / Chrome", "Medium", 1, "New", "Explicabo est qui enim odit nemo.\nDoloremque et natus.\nFacilis aut dolorum culpa sed et magnam necessitatibus.", "Excepturi praesentium quia pariatur ipsum" },
                    { 5, "7.0.3.2", 3, new DateTime(2025, 8, 30, 4, 38, 29, 73, DateTimeKind.Local).AddTicks(3908), "A possimus facere velit nesciunt ut quo aut voluptas. Reprehenderit facere tenetur et veritatis quo. Quia voluptas amet consectetur est sit ratione atque sit.", null, "Ubuntu 22.04 / Chrome", "Medium", 5, "Testing", "Ad vel maxime.\nIpsam molestiae ducimus aut dolores dolores.\nSint et qui quam quia.", "Quis nisi eveniet corrupti nemo" },
                    { 6, "5.7.1.3", 3, new DateTime(2025, 9, 27, 21, 55, 15, 936, DateTimeKind.Local).AddTicks(234), "Natus mollitia qui explicabo. Quia molestias est. Nihil excepturi ea eum animi assumenda error.", 2, "macOS Ventura / Edge", "Medium", 2, "Done", "Autem porro a impedit ab officiis.\nIncidunt suscipit laudantium aut molestiae possimus non ea.\nEnim voluptatem qui fuga molestias eligendi odio unde soluta.", "Error sit id est molestiae" },
                    { 7, "4.7.3.5", 3, new DateTime(2025, 11, 23, 22, 52, 32, 506, DateTimeKind.Local).AddTicks(6427), "Eos fuga omnis consequatur dolor aut fuga occaecati. Hic itaque voluptates nobis minima velit doloremque. Quasi ratione totam sit aspernatur dicta. Iusto exercitationem adipisci mollitia dolorem. Explicabo minus facilis hic culpa molestiae sint. Fuga et eos harum repellendus nulla soluta velit unde.", null, "Windows 11 / Chrome", "Critical", 1, "Code Review", "Assumenda blanditiis fugit expedita maxime qui natus voluptas sed qui.\nAut sint accusantium eligendi veritatis at.\nUt id cumque atque.", "Dolor atque molestias est saepe" },
                    { 8, "6.0.5.2", 3, new DateTime(2025, 10, 2, 15, 21, 14, 652, DateTimeKind.Local).AddTicks(4777), "Iure vel iure laudantium. Quod sint aut itaque quisquam doloribus. Ratione vel incidunt quis ut similique. Doloremque et aut qui. Qui aliquid dolores et velit. Nam velit eius sit ut.", null, "Windows 10 / Firefox", "Low", 1, "Done", "Aspernatur facilis architecto nihil sed eius laborum deserunt magnam nesciunt.\nRecusandae aperiam perspiciatis voluptatem eum incidunt a sit autem commodi.\nFacilis quia id.", "Voluptatem eum consequatur in odio" },
                    { 9, "5.4.1.4", 3, new DateTime(2026, 1, 22, 21, 11, 39, 256, DateTimeKind.Local).AddTicks(8770), "Nam rem sed. Expedita corporis quo sit quaerat fugiat ut. Alias repellat tenetur aspernatur laboriosam nemo repellat. Tempore ut facere totam est ex quia. Est magni laboriosam reiciendis quas. Voluptatibus est ea.", 2, "macOS Ventura / Chrome", "High", 1, "Done", "Aliquam enim eius qui.\nExercitationem dolores dolorem amet cumque perspiciatis velit corrupti sed ea.\nAutem omnis quisquam id non.", "Dolores molestias eum nulla in" },
                    { 10, "3.0.6.4", 3, new DateTime(2026, 1, 19, 14, 15, 26, 748, DateTimeKind.Local).AddTicks(9650), "Praesentium aut eos dolorem dolorum temporibus ipsam rerum aut sunt. Tempore modi non libero. Tenetur sit quisquam itaque et. Et aut tempora nobis labore porro reprehenderit atque quisquam.", 2, "Windows 10 / Safari", "High", 3, "Code Review", "Aspernatur at voluptatem.\nQuasi dolores ex impedit qui corrupti est quam voluptas.\nQuibusdam autem accusantium repellendus aspernatur.", "Eligendi explicabo ab nobis ipsam" },
                    { 11, "6.9.4.7", 3, new DateTime(2025, 8, 11, 9, 23, 46, 856, DateTimeKind.Local).AddTicks(8909), "Et et qui aut aut autem autem aut. Delectus rerum necessitatibus. Est est iure consequatur esse omnis vel id nostrum. Adipisci quam nesciunt.", 2, "macOS Ventura / Edge", "Medium", 4, "Done", "Labore consequatur similique tempore impedit molestiae impedit.\nUt ad autem.\nUt nam unde reprehenderit sunt asperiores ab.", "Dolore voluptatem at dolorem in" },
                    { 12, "6.2.0.1", 3, new DateTime(2026, 1, 20, 11, 19, 26, 133, DateTimeKind.Local).AddTicks(2986), "Exercitationem ut praesentium sequi similique et aut. Eum eius voluptatem illo eveniet molestiae velit consequatur rem quidem. Iusto vitae sint eveniet accusamus facilis molestias. Est qui id nostrum cupiditate. Aut quibusdam omnis officia provident laboriosam sunt qui dolores.", 2, "Windows 11 / Safari", "Critical", 2, "Done", "Deserunt voluptate aut porro corporis molestiae consectetur autem laborum ut.\nEt autem laboriosam officia molestias.\nPossimus praesentium ipsum.", "Et iste pariatur magnam eos" },
                    { 13, "0.3.8.1", 3, new DateTime(2025, 8, 22, 20, 58, 22, 475, DateTimeKind.Local).AddTicks(789), "Consequatur modi harum ea cum maiores quas ducimus nihil. Alias voluptatem minima illum commodi quia. Nam dolor omnis optio dolor doloribus velit et ipsam. Eligendi nulla vel eaque quia accusamus enim. Labore ut qui magnam quidem quod similique nihil.", 2, "Windows 10 / Chrome", "Low", 4, "Testing", "Omnis et sit.\nMagnam voluptatem nesciunt quidem.\nOfficiis sunt eum ut eius non ea voluptatum.", "Laborum officia asperiores corporis laudantium" },
                    { 14, "4.6.1.9", 3, new DateTime(2025, 12, 17, 1, 39, 17, 833, DateTimeKind.Local).AddTicks(7787), "Molestias ullam corporis odio culpa modi in dignissimos. Dolor et consequatur quia et necessitatibus neque non delectus nam. Ut est aut qui consequuntur atque inventore modi. At sapiente ex voluptatem qui quam veritatis. Quis accusantium qui maiores. Sequi eum ratione quia praesentium ullam.", 2, "Windows 10 / Chrome", "Low", 4, "Testing", "Voluptatibus explicabo neque consequatur sit id id.\nNesciunt ipsam aspernatur neque quia.\nQui numquam error quod quod eum dolorem doloribus iste earum.", "Esse ratione voluptatem magni rem" },
                    { 15, "6.7.5.4", 3, new DateTime(2026, 1, 1, 20, 18, 12, 131, DateTimeKind.Local).AddTicks(9763), "Sit inventore non expedita consequatur non perferendis laborum culpa. Quisquam sint cumque asperiores sed modi. Nobis enim sed fugit nesciunt enim architecto voluptate atque aliquid. Esse perferendis consequatur saepe cumque inventore. Dolores aut consectetur voluptas itaque est error esse fuga corporis.", 2, "macOS Ventura / Chrome", "Medium", 3, "Testing", "Asperiores iste modi in officia dolore in error expedita.\nPlaceat vero dolore voluptas dolor harum adipisci similique sunt.\nNumquam facilis voluptatem eaque non est voluptas suscipit excepturi.", "Omnis quibusdam unde non nulla" },
                    { 16, "2.9.4.6", 3, new DateTime(2025, 8, 14, 15, 28, 41, 440, DateTimeKind.Local).AddTicks(2853), "Excepturi omnis aut quis pariatur ab voluptatum fuga aliquid sed. Commodi vel voluptate et. Odit sed inventore.", null, "Windows 10 / Chrome", "Low", 2, "New", "Soluta quo eum.\nAliquid eos aut excepturi vitae labore.\nQuis illo fugiat voluptas in sed dolor necessitatibus quidem nostrum.", "Ipsam dolores ea fugit placeat" },
                    { 17, "2.5.9.2", 3, new DateTime(2025, 9, 2, 8, 47, 49, 881, DateTimeKind.Local).AddTicks(6319), "Impedit ipsa delectus. Aut fuga eveniet nemo voluptatibus beatae libero. Non tempora nihil autem minus optio quia. Officiis non vero odio alias earum sint.", 2, "macOS Ventura / Chrome", "Low", 1, "In Progress", "Maiores sunt et velit eaque neque sit.\nVoluptas ea doloremque quidem architecto ipsum harum.\nEa natus consequatur.", "Est quasi voluptatem cum autem" },
                    { 18, "3.7.5.4", 3, new DateTime(2025, 10, 16, 8, 33, 4, 915, DateTimeKind.Local).AddTicks(6999), "Totam atque assumenda quae eum totam. Libero corrupti et cumque saepe. Aperiam odio in voluptatem sit impedit ipsa cum rerum. Sit repellat voluptate.", 2, "Ubuntu 22.04 / Chrome", "Low", 4, "Testing", "Et et molestiae est at eius et itaque beatae.\nQuisquam iusto consequatur velit enim voluptas natus.\nQuis ea quos aspernatur deserunt natus.", "Ut dolorem impedit ullam voluptatem" },
                    { 19, "4.3.6.5", 3, new DateTime(2025, 11, 19, 17, 59, 54, 938, DateTimeKind.Local).AddTicks(7038), "Commodi aut molestiae. Ut nulla commodi sit atque in quo consequatur nemo voluptas. Consequatur consequatur et at ut.", 2, "Windows 10 / Safari", "Critical", 5, "In Progress", "Quidem at tempore perferendis qui minus.\nSed ut deserunt laborum ad qui id.\nMagni quasi impedit occaecati iure laboriosam tenetur.", "Aut natus expedita pariatur cum" },
                    { 20, "6.1.0.1", 3, new DateTime(2025, 11, 14, 22, 59, 36, 803, DateTimeKind.Local).AddTicks(9026), "Vero voluptatibus culpa. Aut qui repudiandae doloremque expedita labore quod provident sit excepturi. Soluta soluta laboriosam. Temporibus aperiam reiciendis aut illo voluptatibus temporibus. Quo aut quos. Amet hic voluptate non ex quis.", null, "macOS Ventura / Edge", "Critical", 5, "In Progress", "Ut nobis dignissimos tempore.\nTenetur aut architecto quos porro id ipsa dignissimos esse.\nUt beatae aut aut non.", "Soluta est quia facere rerum" }
                });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "AuthorId", "CreatedAt", "IsInternal", "Message", "TicketId" },
                values: new object[,]
                {
                    { 1, 3, new DateTime(2025, 12, 24, 12, 57, 18, 73, DateTimeKind.Local).AddTicks(5417), false, "Maxime vel id ab sit voluptatem adipisci et voluptates dolores. Praesentium beatae nulla similique. Consequatur earum nesciunt accusantium dolor. Facere quidem nobis. Occaecati eos mollitia at possimus quasi deserunt explicabo sint quia.", 19 },
                    { 2, 2, new DateTime(2025, 11, 23, 11, 8, 38, 551, DateTimeKind.Local).AddTicks(3223), true, "Possimus fugit sit eum. Deleniti cumque sequi aut. Qui sapiente consequuntur quod inventore aut commodi.", 2 },
                    { 3, 2, new DateTime(2025, 12, 13, 7, 38, 30, 473, DateTimeKind.Local).AddTicks(7498), false, "Sed accusamus maiores non ea pariatur non aut. Provident officia dolore rerum qui.", 1 },
                    { 4, 2, new DateTime(2025, 11, 7, 11, 12, 5, 902, DateTimeKind.Local).AddTicks(3040), false, "Non magnam harum delectus quia ratione nemo iusto vel. Odio nesciunt cumque autem et. Dolor unde laboriosam nemo et sed omnis.", 10 },
                    { 5, 2, new DateTime(2025, 12, 6, 4, 54, 37, 705, DateTimeKind.Local).AddTicks(928), false, "Vel qui laborum. Numquam cupiditate accusamus labore sit facere non. Omnis adipisci aut explicabo molestiae perspiciatis voluptatem. Repellendus natus quos aut dolores eaque.", 5 },
                    { 6, 3, new DateTime(2026, 1, 9, 9, 35, 27, 339, DateTimeKind.Local).AddTicks(3277), false, "Expedita quis et tempore dolorem error atque at. Minima nesciunt quibusdam consequuntur. Accusantium nisi ut suscipit fugit hic voluptatem voluptatem ad. Sunt excepturi praesentium quia pariatur ipsum aperiam mollitia architecto.", 9 },
                    { 7, 2, new DateTime(2025, 7, 31, 5, 4, 22, 870, DateTimeKind.Local).AddTicks(6375), false, "Eos veritatis velit amet molestias. Optio aut quis perferendis sit nesciunt. Enim qui culpa qui explicabo est qui enim odit.", 1 },
                    { 8, 2, new DateTime(2026, 1, 26, 7, 17, 26, 309, DateTimeKind.Local).AddTicks(2650), false, "Facilis aut dolorum culpa sed et magnam necessitatibus. Architecto assumenda nihil qui sed sunt neque sint. Dolorem quis nisi. Corrupti nemo voluptatem facere a possimus facere velit nesciunt ut.", 7 },
                    { 9, 3, new DateTime(2026, 1, 15, 20, 7, 39, 504, DateTimeKind.Local).AddTicks(1763), false, "Tenetur et veritatis quo voluptas quia voluptas amet consectetur. Sit ratione atque sit architecto ad vel maxime qui. Molestiae ducimus aut dolores dolores.", 11 },
                    { 10, 3, new DateTime(2025, 8, 14, 4, 57, 34, 39, DateTimeKind.Local).AddTicks(1672), true, "Animi assumenda voluptas inventore. Modi quas inventore dolorem reiciendis. Error sit id est molestiae eos consectetur natus mollitia qui.", 7 },
                    { 11, 3, new DateTime(2025, 12, 25, 8, 43, 34, 70, DateTimeKind.Local).AddTicks(7636), false, "Nihil excepturi ea eum animi assumenda error. Autem porro a impedit ab officiis. Incidunt suscipit laudantium aut molestiae possimus non ea. Enim voluptatem qui fuga molestias eligendi odio unde soluta.", 11 },
                    { 12, 3, new DateTime(2025, 12, 14, 15, 30, 37, 82, DateTimeKind.Local).AddTicks(9869), false, "Autem et voluptatem incidunt quia. Atque molestias est saepe.", 18 },
                    { 13, 3, new DateTime(2025, 11, 24, 8, 5, 13, 562, DateTimeKind.Local).AddTicks(338), true, "Dolor aut fuga occaecati culpa hic itaque voluptates nobis. Velit doloremque qui quasi ratione. Sit aspernatur dicta enim iusto exercitationem. Mollitia dolorem sint explicabo. Facilis hic culpa molestiae sint pariatur fuga et.", 16 },
                    { 14, 3, new DateTime(2025, 11, 8, 7, 59, 7, 432, DateTimeKind.Local).AddTicks(7873), true, "Unde non assumenda blanditiis fugit expedita. Qui natus voluptas sed qui autem aut sint. Eligendi veritatis at. Ut id cumque atque.", 9 },
                    { 15, 2, new DateTime(2025, 8, 26, 12, 47, 26, 844, DateTimeKind.Local).AddTicks(3413), false, "Enim et harum voluptatem maxime voluptatem eum. In odio maiores dolore iure. Iure laudantium rem quod sint aut itaque quisquam doloribus.", 17 },
                    { 16, 2, new DateTime(2025, 8, 17, 14, 49, 44, 684, DateTimeKind.Local).AddTicks(5347), false, "Similique non doloremque et aut. Exercitationem qui aliquid dolores et velit. Nam velit eius sit ut.", 13 },
                    { 17, 3, new DateTime(2025, 11, 19, 9, 44, 18, 5, DateTimeKind.Local).AddTicks(9864), true, "Eius laborum deserunt magnam. Debitis recusandae aperiam perspiciatis. Eum incidunt a. Autem commodi explicabo facilis.", 18 },
                    { 18, 3, new DateTime(2025, 9, 25, 12, 55, 57, 923, DateTimeKind.Local).AddTicks(2270), false, "Culpa aliquam consequuntur. Ut consectetur cumque dolores molestias eum. In eos quae nam rem sed et expedita corporis. Sit quaerat fugiat ut occaecati alias repellat tenetur aspernatur.", 20 },
                    { 19, 3, new DateTime(2025, 12, 10, 10, 53, 41, 592, DateTimeKind.Local).AddTicks(7197), false, "Facere totam est ex quia suscipit est magni laboriosam reiciendis. Quae voluptatibus est ea et aliquam enim. Qui asperiores exercitationem dolores. Amet cumque perspiciatis velit corrupti sed ea quia autem.", 15 },
                    { 20, 3, new DateTime(2025, 12, 23, 8, 31, 58, 550, DateTimeKind.Local).AddTicks(5717), false, "Et praesentium magni quos asperiores sit a sit corporis eligendi. Ab nobis ipsam. Et praesentium aut eos dolorem.", 7 },
                    { 21, 2, new DateTime(2025, 10, 3, 13, 27, 45, 337, DateTimeKind.Local).AddTicks(2552), true, "Dolor tempore modi non libero enim tenetur. Quisquam itaque et et et. Tempora nobis labore porro reprehenderit atque quisquam aut aspernatur. Voluptatem omnis quasi dolores ex impedit qui corrupti est. Voluptas veniam quibusdam autem accusantium repellendus.", 12 },
                    { 22, 2, new DateTime(2025, 10, 8, 7, 0, 14, 785, DateTimeKind.Local).AddTicks(9541), false, "Rem illo earum itaque omnis mollitia dolore voluptatem. Dolorem in consequatur nam et et qui aut aut.", 3 },
                    { 23, 2, new DateTime(2025, 11, 25, 15, 59, 59, 111, DateTimeKind.Local).AddTicks(4735), false, "Necessitatibus et est est iure consequatur esse omnis vel id. Perferendis adipisci quam nesciunt perspiciatis. Consequatur similique tempore impedit. Impedit quia ut ad autem qui. Nam unde reprehenderit sunt asperiores ab nobis.", 19 },
                    { 24, 3, new DateTime(2026, 1, 8, 16, 45, 28, 728, DateTimeKind.Local).AddTicks(3865), false, "Iste veritatis et incidunt et iste pariatur magnam eos. Excepturi exercitationem ut praesentium sequi similique et aut. Eum eius voluptatem illo eveniet molestiae velit consequatur rem quidem. Iusto vitae sint eveniet accusamus facilis molestias. Est qui id nostrum cupiditate.", 16 },
                    { 25, 3, new DateTime(2025, 8, 31, 10, 9, 46, 335, DateTimeKind.Local).AddTicks(4177), false, "Laboriosam sunt qui dolores molestiae deserunt voluptate. Porro corporis molestiae. Autem laborum ut corporis. Autem laboriosam officia molestias veritatis possimus.", 2 },
                    { 26, 3, new DateTime(2025, 9, 28, 5, 31, 12, 84, DateTimeKind.Local).AddTicks(143), false, "Sit consequuntur odio omnis. Exercitationem accusantium laborum officia asperiores corporis laudantium quidem temporibus consequatur. Harum ea cum maiores. Ducimus nihil quam alias voluptatem minima illum.", 17 },
                    { 27, 3, new DateTime(2025, 10, 5, 8, 13, 26, 201, DateTimeKind.Local).AddTicks(7038), false, "Optio dolor doloribus velit et ipsam excepturi eligendi nulla. Eaque quia accusamus enim placeat labore. Qui magnam quidem quod similique nihil sit. Et sit non magnam voluptatem nesciunt quidem soluta officiis. Eum ut eius.", 10 },
                    { 28, 3, new DateTime(2025, 9, 24, 6, 44, 0, 963, DateTimeKind.Local).AddTicks(6595), false, "Eum debitis voluptatem. Et consequuntur dolor. Esse ratione voluptatem. Rem ut quidem molestias.", 9 },
                    { 29, 2, new DateTime(2025, 8, 22, 21, 45, 12, 238, DateTimeKind.Local).AddTicks(986), true, "Dignissimos asperiores dolor et consequatur quia. Necessitatibus neque non delectus nam quidem ut est aut.", 10 },
                    { 30, 2, new DateTime(2025, 8, 28, 3, 20, 53, 256, DateTimeKind.Local).AddTicks(849), false, "At sapiente ex voluptatem qui quam veritatis. Quis accusantium qui maiores.", 17 },
                    { 31, 3, new DateTime(2025, 8, 30, 1, 16, 5, 858, DateTimeKind.Local).AddTicks(4364), true, "Ullam non voluptatibus explicabo neque consequatur. Id id nostrum nesciunt ipsam.", 4 },
                    { 32, 3, new DateTime(2025, 8, 4, 4, 31, 12, 830, DateTimeKind.Local).AddTicks(7154), true, "Error quod quod eum. Doloribus iste earum nulla est laudantium dolorum dolor repellat.", 16 },
                    { 33, 2, new DateTime(2025, 10, 8, 18, 44, 5, 618, DateTimeKind.Local).AddTicks(1232), false, "Quibusdam unde non nulla soluta assumenda sit inventore non. Consequatur non perferendis laborum culpa vel quisquam sint. Asperiores sed modi sapiente nobis enim sed fugit. Enim architecto voluptate atque.", 9 },
                    { 34, 3, new DateTime(2025, 12, 4, 6, 3, 4, 125, DateTimeKind.Local).AddTicks(9754), false, "Cumque inventore ut dolores aut consectetur voluptas itaque est error. Fuga corporis quo asperiores iste modi. Officia dolore in error expedita vero placeat. Dolore voluptas dolor harum adipisci similique sunt dolorem numquam. Voluptatem eaque non est voluptas suscipit excepturi unde.", 16 },
                    { 35, 3, new DateTime(2025, 9, 2, 22, 28, 1, 572, DateTimeKind.Local).AddTicks(3098), false, "Sequi voluptas molestiae natus ipsam dolores ea fugit placeat. Earum excepturi omnis aut. Pariatur ab voluptatum fuga aliquid.", 8 },
                    { 36, 3, new DateTime(2025, 9, 21, 22, 32, 16, 70, DateTimeKind.Local).AddTicks(8349), false, "Illo odit sed inventore. Soluta quo eum. Aliquid eos aut excepturi vitae labore.", 2 },
                    { 37, 2, new DateTime(2025, 9, 16, 22, 36, 31, 869, DateTimeKind.Local).AddTicks(6960), true, "Sed dolor necessitatibus quidem nostrum rerum eos. Ut deleniti laborum sequi architecto. Exercitationem asperiores est.", 14 },
                    { 38, 3, new DateTime(2026, 1, 7, 15, 13, 50, 491, DateTimeKind.Local).AddTicks(6929), false, "Impedit ipsa delectus. Aut fuga eveniet nemo voluptatibus beatae libero. Non tempora nihil autem minus optio quia.", 11 },
                    { 39, 3, new DateTime(2025, 9, 19, 13, 51, 17, 2, DateTimeKind.Local).AddTicks(3324), true, "Earum sint mollitia. Sunt et velit eaque neque sit deserunt voluptas ea doloremque. Architecto ipsum harum beatae ea natus consequatur voluptas.", 11 },
                    { 40, 3, new DateTime(2025, 8, 5, 23, 51, 53, 630, DateTimeKind.Local).AddTicks(5094), false, "Qui sit sit non ut dolorem impedit ullam voluptatem velit. Totam atque assumenda quae eum totam. Libero corrupti et cumque saepe.", 9 },
                    { 41, 3, new DateTime(2026, 1, 5, 20, 46, 54, 667, DateTimeKind.Local).AddTicks(7769), false, "Impedit ipsa cum rerum ipsa. Repellat voluptate et.", 19 },
                    { 42, 2, new DateTime(2025, 8, 23, 14, 50, 52, 801, DateTimeKind.Local).AddTicks(3078), false, "Et itaque beatae animi. Iusto consequatur velit enim voluptas natus odio quis. Quos aspernatur deserunt natus delectus est. Placeat occaecati odio placeat quae. Nam laudantium aut natus expedita pariatur.", 2 },
                    { 43, 2, new DateTime(2025, 11, 3, 21, 9, 55, 700, DateTimeKind.Local).AddTicks(1394), false, "Eveniet ut nulla commodi sit atque. Quo consequatur nemo voluptas ad consequatur consequatur. At ut esse quidem. Tempore perferendis qui minus molestias sed ut deserunt laborum. Qui id et magni quasi.", 8 },
                    { 44, 2, new DateTime(2025, 12, 3, 9, 53, 14, 961, DateTimeKind.Local).AddTicks(4297), true, "Suscipit esse ex cumque. Consectetur pariatur est qui incidunt soluta est. Facere rerum possimus. Vero voluptatibus culpa. Aut qui repudiandae doloremque expedita labore quod provident sit excepturi.", 14 },
                    { 45, 3, new DateTime(2026, 1, 25, 4, 44, 13, 531, DateTimeKind.Local).AddTicks(2425), true, "Aperiam reiciendis aut illo voluptatibus temporibus quae quo aut. Autem amet hic voluptate non ex. Amet ut nobis dignissimos tempore. Tenetur aut architecto quos porro id ipsa dignissimos esse.", 2 },
                    { 46, 3, new DateTime(2025, 9, 14, 6, 34, 38, 238, DateTimeKind.Local).AddTicks(3137), true, "Alias enim nobis velit sit neque eos. Culpa et maiores quisquam enim quos odit. Nisi commodi odit illo earum voluptates ullam sit aut delectus. Sapiente eveniet voluptatibus. Est eum voluptas amet et.", 14 },
                    { 47, 2, new DateTime(2025, 10, 3, 19, 54, 10, 40, DateTimeKind.Local).AddTicks(8639), true, "Quos similique minus aut minus tenetur nemo. Fuga porro omnis sint officia explicabo.", 2 },
                    { 48, 2, new DateTime(2025, 11, 27, 21, 36, 12, 423, DateTimeKind.Local).AddTicks(5581), true, "Expedita animi deserunt suscipit laboriosam totam nemo sit et aspernatur. Qui delectus eum esse qui animi incidunt ut.", 11 },
                    { 49, 2, new DateTime(2025, 9, 16, 6, 1, 30, 785, DateTimeKind.Local).AddTicks(9105), true, "Totam aliquam sunt voluptates est neque id deserunt amet. Saepe reiciendis qui perspiciatis ut est quia totam non aut. Hic sed cum minima facere vero. Voluptatem magnam dolores et voluptatibus laudantium officiis non numquam.", 19 },
                    { 50, 2, new DateTime(2025, 10, 8, 7, 4, 22, 827, DateTimeKind.Local).AddTicks(6668), false, "Atque cupiditate facere quia est delectus ducimus non illo. Id quos sint ab esse et eveniet corporis repellendus. Itaque assumenda est repellat non libero inventore. Pariatur non deserunt et in ipsum. Sequi non unde dolorem pariatur sunt quisquam beatae.", 20 }
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
