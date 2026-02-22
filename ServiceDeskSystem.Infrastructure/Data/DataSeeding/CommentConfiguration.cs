using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Infrastructure.Data.DataSeeding;

internal static class CommentConfiguration
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>().HasData(
            new Comment
            {
                Id = 3,
                Message = "This is blocking our monthly inventory audit. Please prioritize!",
                IsInternal = false,
                CreatedAt = new DateTime(2024, 4, 10, 11, 0, 0, DateTimeKind.Utc),
                TicketId = 3,
                AuthorId = 4,
            },
            new Comment
            {
                Id = 4,
                Message = "Found the issue - race condition in sync handler. Fix ready for review.",
                IsInternal = true,
                CreatedAt = new DateTime(2024, 4, 11, 16, 0, 0, DateTimeKind.Utc),
                TicketId = 3,
                AuthorId = 5,
            },
            new Comment
            {
                Id = 5,
                Message = "Виправлення готове, передано на тестування.",
                IsInternal = false,
                CreatedAt = new DateTime(2024, 4, 8, 10, 0, 0, DateTimeKind.Utc),
                TicketId = 5,
                AuthorId = 5,
            },
            new Comment
            {
                Id = 6,
                Message = "Зв'язався з технічною підтримкою LiqPay. Очікую відповідь.",
                IsInternal = false,
                CreatedAt = new DateTime(2024, 4, 19, 14, 0, 0, DateTimeKind.Utc),
                TicketId = 7,
                AuthorId = 2,
            },
            new Comment
            {
                Id = 7,
                Message = "LiqPay змінили API endpoint. Потрібно оновити конфігурацію.",
                IsInternal = true,
                CreatedAt = new DateTime(2024, 4, 19, 16, 30, 0, DateTimeKind.Utc),
                TicketId = 7,
                AuthorId = 2,
            },
            new Comment
            {
                Id = 1,
                Message = "Дякую за звернення. Відтворив проблему, почав аналіз.",
                IsInternal = false,
                CreatedAt = new DateTime(2024, 4, 15, 14, 0, 0, DateTimeKind.Utc),
                TicketId = 1,
                AuthorId = 2,
            },
            new Comment
            {
                Id = 2,
                Message = "Проблема в парсингу дат - треба виправити формат для українського регіону.",
                IsInternal = true,
                CreatedAt = new DateTime(2024, 4, 15, 15, 30, 0, DateTimeKind.Utc),
                TicketId = 1,
                AuthorId = 2,
            },
            new Comment
            {
                Id = 8,
                Message = "Fixed by merging guest cart with user cart on login. Deployed to production.",
                IsInternal = false,
                CreatedAt = new DateTime(2024, 3, 28, 12, 0, 0, DateTimeKind.Utc),
                TicketId = 8,
                AuthorId = 5,
            },
            new Comment
            {
                Id = 9,
                Message = "Issue is in the USB driver timeout handling. Preparing hotfix.",
                IsInternal = true,
                CreatedAt = new DateTime(2024, 4, 9, 14, 0, 0, DateTimeKind.Utc),
                TicketId = 11,
                AuthorId = 5,
            },
            new Comment
            {
                Id = 10,
                Message = "Проблема критична - офіс не може працювати. Потрібно відкотити прошивку!",
                IsInternal = false,
                CreatedAt = new DateTime(2024, 4, 22, 8, 30, 0, DateTimeKind.Utc),
                TicketId = 12,
                AuthorId = 3,
            },
            new Comment
            {
                Id = 11,
                Message = "Тимчасово відкотив до версії 2.0.8. Аналізую зміни в 2.1.0.",
                IsInternal = false,
                CreatedAt = new DateTime(2024, 4, 22, 9, 45, 0, DateTimeKind.Utc),
                TicketId = 12,
                AuthorId = 5,
            },
            new Comment
            {
                Id = 12,
                Message = "Increased buffer size and connection pool. Testing in progress.",
                IsInternal = false,
                CreatedAt = new DateTime(2024, 4, 16, 11, 0, 0, DateTimeKind.Utc),
                TicketId = 13,
                AuthorId = 2,
            });
    }
}
