namespace ServiceDeskSystem.Services;

internal sealed class LocalizationService : ILocalizationService
{
    private readonly Dictionary<string, Dictionary<string, string>> translations = new ()
    {
        ["en"] = new Dictionary<string, string>
        {
            ["nav.allTickets"] = "All Tickets",
            ["nav.newTicket"] = "New Ticket",
            ["nav.serviceDesk"] = "ServiceDesk",
            ["login.title"] = "Sign in to your account",
            ["login.username"] = "Username",
            ["login.password"] = "Password",
            ["login.signIn"] = "Sign In",
            ["login.signingIn"] = "Signing in...",
            ["login.enterUsername"] = "Enter your username",
            ["login.enterPassword"] = "Enter your password",
            ["dashboard.welcome"] = "Welcome back!",
            ["dashboard.totalTickets"] = "Total Tickets",
            ["dashboard.openTickets"] = "Open Tickets",
            ["dashboard.criticalPriority"] = "Critical Priority",
            ["dashboard.myTickets"] = "My Tickets",
            ["tickets.title"] = "All Tickets",
            ["tickets.subtitle"] = "Manage and track support requests",
            ["tickets.noTickets"] = "No tickets yet",
            ["tickets.createFirst"] = "Get started by creating your first ticket",
            ["tickets.createTicket"] = "Create Ticket",
            ["tickets.loading"] = "Loading tickets...",
            ["table.id"] = "ID",
            ["table.title"] = "Title",
            ["table.status"] = "Status",
            ["table.priority"] = "Priority",
            ["table.product"] = "Product",
            ["table.author"] = "Author",
            ["table.created"] = "Created",
            ["create.title"] = "Create New Ticket",
            ["create.subtitle"] = "Submit a new support request or bug report",
            ["create.ticketTitle"] = "Title",
            ["create.description"] = "Description",
            ["create.priority"] = "Priority",
            ["create.product"] = "Product",
            ["create.stepsToReproduce"] = "Steps to Reproduce",
            ["create.environment"] = "Environment",
            ["create.cancel"] = "Cancel",
            ["create.submit"] = "Create Ticket",
            ["create.creating"] = "Creating...",
            ["create.selectPriority"] = "Select priority...",
            ["create.selectProduct"] = "Select product...",
            ["details.description"] = "Description",
            ["details.comments"] = "Comments",
            ["details.addComment"] = "Add a Comment",
            ["details.sendComment"] = "Send Comment",
            ["details.sending"] = "Sending...",
            ["details.noComments"] = "No comments yet.",
            ["details.writeComment"] = "Write your comment...",
            ["details.details"] = "Details",
            ["details.ticketId"] = "Ticket ID",
            ["details.actions"] = "Actions",
            ["details.startProgress"] = "Start Progress",
            ["details.markResolved"] = "Mark Resolved",
            ["details.closeTicket"] = "Close Ticket",
            ["details.reopenTicket"] = "Reopen Ticket",
            ["details.backToTickets"] = "? Back to Tickets",
            ["details.loadingTicket"] = "Loading ticket...",
            ["priority.low"] = "Low",
            ["priority.medium"] = "Medium",
            ["priority.high"] = "High",
            ["priority.critical"] = "Critical",
            ["status.open"] = "Open",
            ["status.inProgress"] = "In Progress",
            ["status.resolved"] = "Resolved",
            ["status.closed"] = "Closed",
            ["common.logout"] = "Logout",
            ["common.required"] = "required",
            ["common.footer"] = "Service Desk System",
            ["theme.light"] = "Light",
            ["theme.dark"] = "Dark",
            ["theme.toggle"] = "Toggle theme",
            ["register.title"] = "Create your account",
            ["register.firstName"] = "First Name",
            ["register.lastName"] = "Last Name",
            ["register.email"] = "Email (optional)",
            ["register.enterFirstName"] = "Enter your first name",
            ["register.enterLastName"] = "Enter your last name",
            ["register.enterEmail"] = "Enter your email",
            ["register.confirmPassword"] = "Confirm Password",
            ["register.enterConfirmPassword"] = "Confirm your password",
            ["register.signUp"] = "Sign Up",
            ["register.signingUp"] = "Creating account...",
            ["register.haveAccount"] = "Already have an account?",
            ["register.signIn"] = "Sign In",
            ["login.noAccount"] = "Don't have an account?",
            ["login.signUp"] = "Sign Up",
        },
        ["uk"] = new Dictionary<string, string>
        {
            ["nav.allTickets"] = "Усі заявки",
            ["nav.newTicket"] = "Нова заявка",
            ["nav.serviceDesk"] = "ServiceDesk",
            ["login.title"] = "Увійдіть у свій акаунт",
            ["login.username"] = "Ім'я користувача",
            ["login.password"] = "Пароль",
            ["login.signIn"] = "Увійти",
            ["login.signingIn"] = "Вхід...",
            ["login.enterUsername"] = "Введіть ім'я користувача",
            ["login.enterPassword"] = "Введіть пароль",
            ["dashboard.welcome"] = "З поверненням!",
            ["dashboard.totalTickets"] = "Всього заявок",
            ["dashboard.openTickets"] = "Відкриті заявки",
            ["dashboard.criticalPriority"] = "Критичний пріоритет",
            ["dashboard.myTickets"] = "Мої заявки",
            ["tickets.title"] = "Усі заявки",
            ["tickets.subtitle"] = "Керуйте та відстежуйте запити на підтримку",
            ["tickets.noTickets"] = "Заявок ще немає",
            ["tickets.createFirst"] = "Почніть зі створення першої заявки",
            ["tickets.createTicket"] = "Створити заявку",
            ["tickets.loading"] = "Завантаження заявок...",
            ["table.id"] = "ID",
            ["table.title"] = "Назва",
            ["table.status"] = "Статус",
            ["table.priority"] = "Пріоритет",
            ["table.product"] = "Продукт",
            ["table.author"] = "Автор",
            ["table.created"] = "Створено",
            ["create.title"] = "Створити нову заявку",
            ["create.subtitle"] = "Подайте новий запит на підтримку або звіт про помилку",
            ["create.ticketTitle"] = "Назва",
            ["create.description"] = "Опис",
            ["create.priority"] = "Пріоритет",
            ["create.product"] = "Продукт",
            ["create.stepsToReproduce"] = "Кроки для відтворення",
            ["create.environment"] = "Середовище",
            ["create.cancel"] = "Скасувати",
            ["create.submit"] = "Створити заявку",
            ["create.creating"] = "Створення...",
            ["create.selectPriority"] = "Виберіть пріоритет...",
            ["create.selectProduct"] = "Виберіть продукт...",
            ["details.description"] = "Опис",
            ["details.comments"] = "Коментарі",
            ["details.addComment"] = "Додати коментар",
            ["details.sendComment"] = "Надіслати",
            ["details.sending"] = "Надсилання...",
            ["details.noComments"] = "Коментарів ще немає.",
            ["details.writeComment"] = "Напишіть коментар...",
            ["details.details"] = "Деталі",
            ["details.ticketId"] = "ID заявки",
            ["details.actions"] = "Дії",
            ["details.startProgress"] = "Розпочати роботу",
            ["details.markResolved"] = "Позначити вирішеною",
            ["details.closeTicket"] = "Закрити заявку",
            ["details.reopenTicket"] = "Відкрити знову",
            ["details.backToTickets"] = "? Назад до заявок",
            ["details.loadingTicket"] = "Завантаження заявки...",
            ["priority.low"] = "Низький",
            ["priority.medium"] = "Середній",
            ["priority.high"] = "Високий",
            ["priority.critical"] = "Критичний",
            ["status.open"] = "Відкрита",
            ["status.inProgress"] = "В роботі",
            ["status.resolved"] = "Вирішена",
            ["status.closed"] = "Закрита",
            ["common.logout"] = "Вийти",
            ["common.required"] = "обов'язково",
            ["common.footer"] = "Система сервісних запитів",
            ["theme.light"] = "Світла",
            ["theme.dark"] = "Темна",
            ["theme.toggle"] = "Змінити тему",
            ["register.title"] = "Створіть обліковий запис",
            ["register.firstName"] = "Ім'я",
            ["register.lastName"] = "Прізвище",
            ["register.email"] = "Email (необов'язково)",
            ["register.enterFirstName"] = "Введіть ваше ім'я",
            ["register.enterLastName"] = "Введіть ваше прізвище",
            ["register.enterEmail"] = "Введіть ваш email",
            ["register.confirmPassword"] = "Підтвердіть пароль",
            ["register.enterConfirmPassword"] = "Підтвердіть ваш пароль",
            ["register.signUp"] = "Зареєструватися",
            ["register.signingUp"] = "Створення акаунту...",
            ["register.haveAccount"] = "Вже є обліковий запис?",
            ["register.signIn"] = "Увійти",
            ["login.noAccount"] = "Немає облікового запису?",
            ["login.signUp"] = "Зареєструватися",
        },
    };

    private string currentLanguage = "en";

    public event EventHandler? LanguageChanged;

    public string CurrentLanguage => this.currentLanguage;

    public void SetLanguage(string language)
    {
        if (this.translations.ContainsKey(language) && this.currentLanguage != language)
        {
            this.currentLanguage = language;
            this.LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string Translate(string key)
    {
        if (this.translations.TryGetValue(this.currentLanguage, out var langDict) &&
            langDict.TryGetValue(key, out var value))
        {
            return value;
        }

        if (this.translations.TryGetValue("en", out var enDict) &&
            enDict.TryGetValue(key, out var enValue))
        {
            return enValue;
        }

        return key;
    }
}
