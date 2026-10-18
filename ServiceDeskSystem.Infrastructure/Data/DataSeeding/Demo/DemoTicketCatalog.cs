using static ServiceDeskSystem.Domain.Enums.TicketPriority;
using static ServiceDeskSystem.Domain.Enums.TicketStatus;
using static ServiceDeskSystem.Domain.Enums.TicketType;
using static ServiceDeskSystem.Infrastructure.Data.DataSeeding.Demo.DemoIds;

namespace ServiceDeskSystem.Infrastructure.Data.DataSeeding.Demo;

/// <summary>
/// The demo tickets: every product, type, priority and status, spread over the last six months and denser
/// towards today. Ages are in days before the seed; a fraction under 1 means earlier today.
/// </summary>
internal static class DemoTicketCatalog
{
    public static IReadOnlyList<DemoTicket> All { get; } =
    [
        // ── New: just reported, nobody has looked yet ──────────────────────────────────────────────────
        new (
            "Не приходить лист для скидання пароля",
            "Після натискання «Забули пароль?» лист не надходить ні у вхідні, ні в спам. Колеги з іншого відділу мають ту саму проблему.",
            "1. Відкрити сторінку входу\n2. Натиснути «Забули пароль?»\n3. Ввести робочу пошту\n4. Чекати лист 15+ хвилин",
            HrPortal, Bug, Medium, Shevchenko, null, New, 0.15, "Windows 11 / Chrome 129", "1.9.0", ["Bug"]),
        new (
            "Checkout button unresponsive on iPad",
            "Tapping \"Place order\" on an iPad does nothing. The same cart checks out fine on a laptop.",
            "1. Open the shop on an iPad in landscape\n2. Add any product to the cart\n3. Go to checkout and tap \"Place order\"",
            Shop, Bug, High, Wilson, null, New, 0.4, "iPadOS 18 / Safari", "4.3.0", ["Bug", "UI/UX"]),
        new (
            "Router drops the VPN tunnel every night at 02:00",
            "The site-to-site tunnel to the warehouse goes down at about 02:00 and only comes back after a manual reconnect.",
            "1. Keep the IPsec tunnel up overnight\n2. Check the router log in the morning",
            Router, Bug, High, Smith, null, New, 0.8, "Office Router Pro / firmware 5.2.3", "5.2.3", ["Bug", "Security"]),
        new (
            "Додати експорт залишків у CSV",
            "Для щотижневого звіту керівництву потрібно вивантажувати залишки по всіх складах у CSV, а не тільки в PDF.",
            "Це запит на нову функцію.",
            Warehouse, Project, Low, Hrytsenko, null, New, 1.2, "Windows 10", "2.7.1", ["Feature"]) { PriorityAssessed = false },
        new (
            "Як правильно округлювати ПДВ у видаткових накладних?",
            "При кількох позиціях сума ПДВ у накладній відрізняється на 1 копійку від суми в податковій накладній. Яке округлення правильне?",
            "1. Створити накладну на 7 позицій з дробовою ціною\n2. Порівняти ПДВ з податковою накладною",
            Accounting, Consultation, Low, Petrenko, null, New, 2.0, "Windows 11", "3.4.0", []) { PriorityAssessed = false },
        new (
            "Сканер не читає штрихкоди GS1-128",
            "Нова партія від постачальника має етикетки GS1-128. ТСД показує «Невідомий формат», хоча EAN-13 читає нормально.",
            "1. Відкрити «Приймання»\n2. Відсканувати етикетку GS1-128",
            Warehouse, Bug, Medium, Hrytsenko, null, New, 2.6, "Zebra TC52 / Android 11", "2.7.1", ["Bug", "Integration"]),
        new (
            "Темна тема в мобільному CRM",
            "Менеджери працюють увечері з телефонів — світлий інтерфейс сліпить. Просимо додати темну тему, як у веб-порталі.",
            "Це запит на нову функцію.",
            Crm, Project, Low, Shevchenko, null, New, 3.0, "Android 14", "2.2.0", ["Feature", "UI/UX"]) { PriorityAssessed = false },

        // ── Open: triaged and assigned, work not started ───────────────────────────────────────────────
        new (
            "POS prints receipts with the wrong date after the clock change",
            "Since the switch to winter time every receipt shows yesterday's date until the first Z-report. Tax inspectors flagged it in one store.",
            "1. Open a shift after 00:00\n2. Sell any item\n3. Check the date on the printed receipt",
            Pos, Bug, Critical, Wilson, Lysenko, Open, 0.3, "POS Terminal v2 / firmware 3.1.0", "3.1.0", ["Bug", "Urgent"])
        {
            DueInDays = 0,
            Conversation =
            [
                "c:Three stores are affected now. Can we get a workaround for today?",
                "d:Workaround: set the terminal time zone manually in Service → Clock and restart. A firmware fix is on the way.",
                "i:RTC keeps the old DST offset until the fiscal module syncs; we should take time from the fiscal module at boot.",
            ],
        },
        new (
            "Lock controller reports 0% battery",
            "Two controllers in the east wing show 0% battery in the app although the batteries were replaced last week.",
            "1. Replace the batteries\n2. Open the lock app\n3. Check the battery level",
            SmartLock, Bug, High, Smith, Lysenko, Open, 0.9, "Smart Lock Controller / firmware 1.4.2", "1.4.2", ["Bug"]) { DueInDays = 3 },
        new (
            "Payment confirmation emails arrive twice",
            "Customers receive two identical \"Payment received\" emails for one order. Support gets a few questions a day about double charges.",
            "1. Place an order paid by card\n2. Check the customer's inbox",
            Shop, Bug, Medium, Wilson, Kovalenko, Open, 1.5, "Production", "4.3.0", ["Bug", "Backend"]) { DueInDays = 5 },
        new (
            "Звіт про відпустки показує дані за минулий рік",
            "У звіті «Залишок відпусток» на початку місяця підтягуються дані за минулий рік, якщо не змінити фільтр вручну.",
            "1. Відкрити «Звіти» → «Відпустки»\n2. Не змінювати фільтр року\n3. Порівняти дані з карткою працівника",
            HrPortal, Bug, Medium, Shevchenko, Melnyk, Open, 2.3, "Windows 11 / Edge", "1.9.0", ["Bug"]) { DueInDays = 6 },
        new (
            "Консультація: автоматичний імпорт банківської виписки",
            "Хочемо, щоб виписка з банку підтягувалася щоранку автоматично. Які банки підтримуються і що потрібно налаштувати?",
            "—",
            Accounting, Consultation, Medium, Petrenko, Kovalenko, Open, 3.5, "Windows 11", "3.4.0", ["Integration"]) { DueInDays = 2 },
        new (
            "Мобільний CRM не синхронізує нотатки, створені офлайн",
            "Нотатки, які менеджер додає без інтернету, зникають після появи мережі. Онлайн усе працює.",
            "1. Увімкнути режим польоту\n2. Додати нотатку до контакту\n3. Вимкнути режим польоту і дочекатися синхронізації",
            Crm, Bug, Medium, Shevchenko, Tkachenko, Open, 4.2, "Android 13 / Samsung A54", "2.2.0", ["Bug"]),
        new (
            "Sign in to the HR portal with Microsoft 365",
            "Staff should sign in with their Microsoft 365 accounts instead of separate portal passwords.",
            "This is a feature request.",
            HrPortal, Project, Medium, Smith, Melnyk, Open, 5.0, "—", "2.0.0", ["Feature", "Security"]) { DueInDays = 18 },

        // ── In progress ───────────────────────────────────────────────────────────────────────────────
        new (
            "LiqPay: тайм-аут шлюзу під час оплати",
            "Частина клієнтів не може завершити оплату через LiqPay — після підтвердження в банку з'являється «Payment gateway timeout».",
            "1. Додати товар у кошик\n2. Обрати оплату LiqPay\n3. Підтвердити платіж у застосунку банку",
            Shop, Bug, Critical, Wilson, Kovalenko, InProgress, 0.6, "Production / Android Chrome", "4.3.0", ["Bug", "Urgent", "Integration"])
        {
            DueInDays = 0,
            Note = "Callback від LiqPay приходить раніше, ніж замовлення встигає зберегтися; потрібна ідемпотентна обробка callback.",
            Conversation =
            [
                "c:За останню годину 14 невдалих оплат. Це основний спосіб оплати для мобільних клієнтів.",
                "d:Бачу в логах тайм-аути на callback. Тимчасово збільшив ліміт очікування, дивлюся причину.",
                "i:Гонка між callback і збереженням замовлення. Роблю обробку через чергу з повтором.",
                "c:Після збільшення ліміту стало краще, але кілька помилок ще є.",
            ],
        },
        new (
            "Помилка при формуванні звіту ПДВ за квартал",
            "При формуванні звіту з ПДВ за квартал програма видає «Invalid date range». За окремі місяці звіт формується.",
            "1. Відкрити «Звіти»\n2. Обрати «Звіт ПДВ»\n3. Вказати період за повний квартал\n4. Натиснути «Сформувати»",
            Accounting, Bug, High, Petrenko, Kovalenko, InProgress, 3.0, "Windows 11", "3.4.0", ["Bug", "Backend"])
        {
            DueInDays = 2,
            Note = "Кінець періоду передається як 00:00 останнього дня в локальному часі, а сервер очікує UTC — останній день випадає з діапазону.",
            Conversation =
            [
                "c:Звіт потрібно подати до 20 числа, чи встигнемо?",
                "d:Відтворив помилку. Окремо за місяці звіт можна сформувати вже зараз, виправлення буде цього тижня.",
                "i:Перевірити також річний звіт — там та сама функція побудови періоду.",
            ],
        },
        new (
            "Promo codes are applied twice after a page refresh",
            "Refreshing the checkout page after applying a promo code applies the discount a second time.",
            "1. Add items for 1000 UAH\n2. Apply code AUTUMN10\n3. Refresh the page\n4. Check the total",
            Shop, Bug, High, Wilson, Melnyk, InProgress, 2.0, "Windows 11 / Chrome 129", "4.3.0", ["Bug", "Backend"]) { DueInDays = 1 },
        new (
            "Smart lock does not re-pair after a firmware update",
            "After updating to 1.5.0-rc the lock forgets the hub and has to be paired again by hand.",
            "1. Update a paired lock to 1.5.0-rc\n2. Wait for the restart\n3. Try to open it from the app",
            SmartLock, Bug, High, Smith, Lysenko, InProgress, 4.0, "Smart Lock Controller / firmware 1.5.0-rc", "1.5.0", ["Bug"]) { DueInDays = 2 },
        new (
            "Push notifications are not delivered on Android 14",
            "Users on Android 14 get no push notifications for new deals and tasks; Android 13 works.",
            "1. Assign a task to a user on Android 14\n2. Wait for the notification",
            Crm, Bug, High, Smith, Tkachenko, InProgress, 6.0, "Android 14 / Pixel 8", "2.2.0", ["Bug"]),
        new (
            "Slow product search with 50,000+ items",
            "Searching by part of a product name takes over 10 seconds in warehouses with more than 50,000 items.",
            "1. Open product search in a large warehouse\n2. Type part of a product name\n3. Wait for results",
            Warehouse, Bug, Medium, Smith, Kovalenko, InProgress, 8.0, "Windows Server 2022 / SQL Server 2019", "2.7.0", ["Performance", "Backend"])
        {
            Note = "LIKE '%term%' over the whole table. Plan: full-text index on Name and SKU, then paging.",
        },
        new (
            "Валідація дат у формі заявки на відпустку",
            "Форма дозволяє обрати дату завершення раніше за дату початку, після чого заявка зависає в статусі «Чернетка».",
            "1. Відкрити «Нова заявка на відпустку»\n2. Вказати кінець раніше за початок\n3. Надіслати",
            HrPortal, Bug, Low, Shevchenko, Melnyk, InProgress, 9.0, "Chrome 129", "1.9.0", ["Bug", "UI/UX"]) { DueInDays = 6 },
        new (
            "Router admin panel: export the audit log",
            "Security asks for a monthly export of administrator actions on the router in CSV.",
            "This is a feature request.",
            Router, Project, Low, Admin, Bondarenko, InProgress, 14.0, "—", "5.3.0", ["Feature", "Security"]) { DueInDays = 20 },
        new (
            "Accessibility review of the HR portal (WCAG 2.2 AA)",
            "Check keyboard navigation, contrast and screen reader labels across the portal and fix what fails AA.",
            "This is a project task.",
            HrPortal, Project, Medium, Admin, Melnyk, InProgress, 16.0, "—", "2.0.0", ["UI/UX", "Documentation"]) { DueInDays = 9 },
        new (
            "Модуль графіка змін для складу",
            "Начальники складів мають планувати зміни прямо в системі, а не в Excel: графік, заміни, облік понаднормових.",
            "Це проєктне завдання.",
            Warehouse, Project, Medium, Admin, Kovalenko, InProgress, 20.0, "—", "2.8.0", ["Feature"]) { DueInDays = 12 },

        // ── Code review ───────────────────────────────────────────────────────────────────────────────
        new (
            "Inventory count mismatch after scanner sync",
            "After syncing the handheld scanners the stock count is wrong for some items, usually those scanned on two devices at once.",
            "1. Count the same shelf on two scanners\n2. Sync both\n3. Compare totals in the system",
            Warehouse, Bug, Critical, Hrytsenko, Kovalenko, CodeReview, 5.0, "Windows Server 2022", "2.7.0", ["Bug", "Backend", "Urgent"])
        {
            DueInDays = 1,
            Note = "Race condition in the sync handler: two devices update the same stock row without a lock. Fixed with a row version check.",
            Conversation =
            [
                "c:Це блокує щомісячну інвентаризацію. Прошу пріоритезувати!",
                "d:Reproduced with two scanners. The fix is in code review now.",
                "i:Race in SyncHandler.ApplyAsync — two devices write the same StockItem. Added a rowversion check and a retry.",
            ],
        },
        new (
            "Mobile CRM crashes on a contact with an emoji in the name",
            "Opening any contact whose name contains an emoji closes the app immediately.",
            "1. Create a contact named \"Olena 🌻\"\n2. Open it from the list",
            Crm, Bug, High, Smith, Tkachenko, CodeReview, 3.0, "Android 14", "2.2.0", ["Bug"]) { DueInDays = 2 },
        new (
            "Аватар у профілі не оновлюється без перезавантаження",
            "Після завантаження нового фото в профілі старий аватар показується, доки не оновиш сторінку.",
            "1. Відкрити профіль\n2. Завантажити нове фото\n3. Подивитися на аватар у шапці",
            HrPortal, Bug, Low, Shevchenko, Melnyk, CodeReview, 6.0, "Firefox 131", "1.9.0", ["Bug", "UI/UX"]) { DueInDays = 4 },
        new (
            "Імпорт банківської виписки у форматі ISO 20022",
            "Банки переходять на camt.053. Потрібен імпорт виписок у цьому форматі з автоматичним рознесенням платежів.",
            "Це проєктне завдання.",
            Accounting, Project, High, Petrenko, Kovalenko, CodeReview, 22.0, "—", "3.5.0", ["Feature", "Integration"]) { DueInDays = 4 },
        new (
            "POS: оплата через Apple Pay",
            "Покупці дедалі частіше просять оплатити телефоном. Термінал має приймати Apple Pay через новий NFC-модуль.",
            "Це проєктне завдання.",
            Pos, Project, Medium, Wilson, Lysenko, CodeReview, 27.0, "POS Terminal v2", "3.2.0", ["Feature", "Integration"]) { DueInDays = 7 },

        // ── Testing ───────────────────────────────────────────────────────────────────────────────────
        new (
            "Не відображаються дні відпустки в особистому кабінеті",
            "В особистому кабінеті працівника не видно залишку днів відпустки за поточний рік.",
            "1. Увійти в особистий кабінет\n2. Перейти в розділ «Відпустки»\n3. Перевірити баланс днів",
            HrPortal, Bug, High, Shevchenko, Melnyk, Testing, 7.0, "Windows 10 / Firefox", "1.9.0", ["Bug"])
        {
            DueInDays = 1,
            Conversation =
            [
                "c:Працівники вже питають, скільки днів у них лишилося. Можна хоча б тимчасово показати в листі?",
                "d:Знайшла причину: баланс рахувався лише для тих, хто працює понад рік. Виправлення на тестуванні.",
                "i:Андрію, перевір, будь ласка, і для працівників на випробувальному терміні.",
            ],
        },
        new (
            "Експорт в Excel створює порожній файл",
            "Експорт документів у формат Excel створює файл без даних — лише заголовки колонок.",
            "1. Відкрити будь-який журнал документів\n2. Натиснути «Експорт» → «Excel»\n3. Відкрити файл",
            Accounting, Bug, Medium, Petrenko, Kovalenko, Testing, 10.0, "Windows 11 / Excel 365", "3.4.0", ["Bug"]) { DueInDays = 3 },
        new (
            "Order status webhook retries flood the ERP",
            "When the ERP answers slowly the shop retries the order webhook every second, creating hundreds of duplicates.",
            "1. Make the ERP endpoint respond in over 5 s\n2. Change an order status\n3. Count the webhook calls",
            Shop, Bug, High, Smith, Kovalenko, Testing, 4.0, "Production", "4.3.0", ["Bug", "Integration", "Backend"]) { DueInDays = 2 },
        new (
            "Auto-lock timer ignores the schedule",
            "Locks set to stay open during office hours still lock after 30 seconds.",
            "1. Set an office-hours schedule\n2. Open the lock at 10:00\n3. Wait a minute",
            SmartLock, Bug, Medium, Smith, Lysenko, Testing, 9.0, "Smart Lock Controller / firmware 1.5.0-rc", "1.5.0", ["Bug"]) { DueInDays = 5 },
        new (
            "Новий екран угод у мобільному CRM",
            "Оновлений екран угод: воронка, фільтри за етапами і швидкі дії свайпом.",
            "Це проєктне завдання.",
            Crm, Project, Medium, Admin, Tkachenko, Testing, 30.0, "Android 14", "2.3.0", ["Feature", "UI/UX"]) { DueInDays = 8 },

        // ── Done: finished by the developer ───────────────────────────────────────────────────────────
        new (
            "Cart items disappear after login",
            "Items added to the cart as a guest disappear after signing in.",
            "1. Browse as a guest\n2. Add items to the cart\n3. Sign in\n4. Open the cart",
            Shop, Bug, High, Wilson, Melnyk, Done, 12.0, "iOS 17 / Safari", "4.2.1", ["Bug"]),
        new (
            "Друк етикеток зі зсувом на 2 мм",
            "Після оновлення драйвера етикетки друкуються зі зсувом, штрихкод обрізається по краю.",
            "1. Надрукувати етикетку товару на Zebra ZD421",
            Warehouse, Bug, Low, Hrytsenko, Bondarenko, Done, 18.0, "Zebra ZD421", "2.7.0", ["Bug"]),
        new (
            "Contact sync hangs with more than 1,000 contacts",
            "Syncing a large address book freezes the app and it has to be restarted.",
            "1. Open Settings\n2. Tap \"Sync contacts\" with 1,000+ contacts on the phone",
            Crm, Bug, Medium, Shevchenko, Tkachenko, Done, 15.0, "iOS 17", "2.1.4", ["Bug", "Performance"]),
        new (
            "POS receipt logo is blurry",
            "The store logo at the top of the receipt prints blurry since the last firmware update.",
            "1. Print any receipt",
            Pos, Bug, Low, Wilson, Lysenko, Done, 9.0, "POS Terminal v2 / firmware 3.1.0", "3.1.0", ["UI/UX"]),
        new (
            "Two-factor authentication for router administrators",
            "Administrator logins to the router panel must require a second factor (TOTP).",
            "This is a project task.",
            Router, Project, High, Admin, Bondarenko, Done, 35.0, "Office Router Pro / firmware 5.2.x", "5.2.3", ["Feature", "Security"]),
        new (
            "Прискорити формування зарплатної відомості",
            "Відомість на 400 працівників формується понад 3 хвилини. Потрібно вкластися в 20 секунд.",
            "Це проєктне завдання.",
            Accounting, Project, Medium, Petrenko, Kovalenko, Done, 40.0, "—", "3.4.0", ["Performance"]),

        // ── Resolved ──────────────────────────────────────────────────────────────────────────────────
        new (
            "Не вдається увійти після зміни пароля",
            "Після зміни пароля портал не пускає з новим паролем, а старий теж не підходить.",
            "1. Змінити пароль у профілі\n2. Вийти\n3. Увійти з новим паролем",
            HrPortal, Support, High, Shevchenko, Melnyk, Resolved, 6.0, "Chrome 129", "1.9.0", ["Security"])
        {
            Conversation =
            [
                "c:Не можу зайти вже пів години, а в мене співбесіди в порталі.",
                "d:Ваш обліковий запис був тимчасово заблокований після кількох невдалих спроб. Розблокувала — спробуйте ще раз.",
                "c:Вийшло, дякую!",
            ],
        },
        new (
            "Як додати нового працівника до графіка?",
            "Новий комірник не з'являється у списку при плануванні змін.",
            "—",
            Warehouse, Consultation, Low, Hrytsenko, Bondarenko, Resolved, 11.0, "Windows 10", "2.7.0", []),
        new (
            "How to isolate guest Wi-Fi from the office network?",
            "Visitors should get internet access but must not see office printers and file shares.",
            "—",
            Router, Consultation, Low, Smith, Bondarenko, Resolved, 20.0, "Office Router Pro / firmware 5.2.3", "5.2.3", ["Documentation"]),
        new (
            "Shipping cost is wrong for EU addresses",
            "Orders to EU countries are charged the domestic shipping rate.",
            "1. Add items to the cart\n2. Enter a delivery address in Poland\n3. Check the shipping cost",
            Shop, Bug, Medium, Wilson, Kovalenko, Resolved, 25.0, "Production", "4.2.0", ["Bug", "Backend"]),
        new (
            "Контролер замка не реагує на NFC-картки",
            "Замок на вході до складу перестав відкриватися картками працівників, працює лише з телефону.",
            "1. Прикласти картку до зчитувача",
            SmartLock, Bug, Critical, Hrytsenko, Lysenko, Resolved, 33.0, "Smart Lock Controller / firmware 1.4.2", "1.4.2", ["Bug", "Urgent", "Security"]),
        new (
            "PDF attendance report fails to generate",
            "Generating the attendance report as PDF ends with an error for months with more than 30 employees.",
            "1. Open Reports\n2. Choose Attendance\n3. Click \"Generate PDF\"",
            HrPortal, Bug, Medium, Smith, Melnyk, Resolved, 45.0, "macOS Sonoma / Safari", "1.8.3", ["Bug"]),
        new (
            "Mobile CRM login loop on Android 13",
            "After entering valid credentials the app returns to the login screen.",
            "1. Install the app on Android 13\n2. Sign in",
            Crm, Bug, High, Smith, Tkachenko, Resolved, 52.0, "Android 13", "2.1.2", ["Bug"]),
        new (
            "Переміщення залишків між складами",
            "Як перемістити залишки зі складу в Києві до складу у Львові так, щоб не зламалася собівартість?",
            "—",
            Warehouse, Support, Medium, Hrytsenko, Kovalenko, Resolved, 70.0, "Windows 10", "2.6.0", ["Documentation"]),

        // ── Closed: confirmed by the client ───────────────────────────────────────────────────────────
        new (
            "Telegram notifications about new orders",
            "Store managers want a message in the team's Telegram chat for every new order over 5,000 UAH.",
            "This is a feature request.",
            Shop, Project, Medium, Wilson, Kovalenko, Closed, 55.0, "Production", "4.2.0", ["Feature", "Integration"]),
        new (
            "Increase session timeout on warehouse terminals",
            "Terminals log users out after 10 minutes, which is too short during stocktaking.",
            "—",
            Warehouse, Support, Low, Smith, Bondarenko, Closed, 45.0, "Windows 10 IoT", "2.6.1", []),
        new (
            "Розбіжність підсумків у звіті інвентаризації",
            "Підсумок у звіті інвентаризації на одну позицію більший, ніж сума рядків.",
            "1. Сформувати звіт інвентаризації по складу\n2. Порівняти підсумок із сумою рядків",
            Warehouse, Bug, Medium, Hrytsenko, Kovalenko, Closed, 65.0, "Windows Server 2022", "2.6.1", ["Bug"]),
        new (
            "Нагадування про зустрічі в CRM не спрацьовують",
            "Нагадування за 15 хвилин до зустрічі не з'являються, якщо додаток закритий.",
            "1. Створити зустріч на найближчу годину\n2. Закрити додаток\n3. Дочекатися нагадування",
            Crm, Bug, Medium, Shevchenko, Tkachenko, Closed, 80.0, "Android 12", "2.0.5", ["Bug"]),
        new (
            "Product images do not load on slow connections",
            "On 3G product pages stay without images; the loader spins forever.",
            "1. Throttle the connection to Slow 3G\n2. Open any product page",
            Shop, Bug, Medium, Wilson, Melnyk, Closed, 95.0, "Android / Chrome", "4.0.5", ["Performance", "UI/UX"]),
        new (
            "Router firmware 5.2 rollout",
            "Update all office routers to firmware 5.2 and document the rollback procedure.",
            "This is a project task.",
            Router, Project, Medium, Admin, Bondarenko, Closed, 110.0, "—", "5.2.0", ["Feature", "Documentation"]),
        new (
            "Не друкується видаткова накладна",
            "Під час друку видаткової накладної програма закривається без повідомлення.",
            "1. Відкрити видаткову накладну\n2. Натиснути «Друк»",
            Accounting, Bug, High, Petrenko, Kovalenko, Closed, 120.0, "Windows 10", "3.2.1", ["Bug"]),
        new (
            "Як підключити замок через Bluetooth?",
            "Потрібна інструкція, як підключити новий замок до телефона без хаба.",
            "—",
            SmartLock, Consultation, Low, Savchuk, Lysenko, Closed, 130.0, "iOS 16", "1.3.0", ["Documentation"]),
        new (
            "Зависання POS під час закриття зміни",
            "Термінал зависає на формуванні Z-звіту, якщо за зміну було понад 500 чеків.",
            "1. Провести 500+ продажів за зміну\n2. Закрити зміну",
            Pos, Bug, Critical, Wilson, Lysenko, Closed, 140.0, "POS Terminal v2 / firmware 3.0.2", "3.0.2", ["Bug", "Urgent"]),
        new (
            "Доступ до порталу для нових співробітників",
            "Новим працівникам відділу продажів потрібен доступ до HR-порталу з першого робочого дня.",
            "—",
            HrPortal, Support, Medium, Savchuk, Melnyk, Closed, 150.0, "Chrome", "1.8.0", []),
        new (
            "Migrate annual leave balances to the new portal",
            "Move leave balances for all employees from the old spreadsheet into the HR portal.",
            "This is a project task.",
            HrPortal, Project, Medium, Admin, Melnyk, Closed, 160.0, "—", "1.8.0", ["Feature", "Backend"]),
        new (
            "Warehouse barcode labels in Ukrainian",
            "Labels print product names in English even when the Ukrainian name is filled in.",
            "1. Fill in the Ukrainian name of a product\n2. Print its label",
            Warehouse, Support, Low, Smith, Bondarenko, Closed, 175.0, "Windows 10", "2.5.0", ["UI/UX"]),
    ];
}
