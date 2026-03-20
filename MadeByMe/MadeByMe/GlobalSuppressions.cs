using System.Diagnostics.CodeAnalysis;

// === Правила документації ===
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:File should have header", Justification = "Заголовки файлів не використовуються.")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Не вимагаємо XML-документацію для всіх елементів.")]

// === Правила сортування та імен ===
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1200:Using directives should be placed correctly", Justification = "using поза namespace.")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1210:Using directives should be ordered alphabetically by namespace", Justification = "Сортування за алфавітом не критичне.")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1208:System using directives should be placed before other using directives", Justification = "Сортування не критичне.")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1309:Field names should not begin with underscore", Justification = "Використовуємо _ для приватних полів.")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "Дозволяємо малу літеру для папки src.")]

// === Правила читабельності ===
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "this не є обов'язковим.")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1111:Closing parenthesis should be on line of last parameter", Justification = "Вільне форматування дужок.")]

// === Правила відступів та пробілів (Layout & Spacing) ===
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1413:Use trailing comma in multi-line initializers", Justification = "Висяча кома не обов'язкова.")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1515:Single-line comment should be preceded by blank line", Justification = "Вільне форматування коментарів.")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1516:Elements should be separated by blank line", Justification = "Вільне форматування порожніх рядків.")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1502:Element should not be on a single line", Justification = "Дозволяємо конструктори в один рядок.")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1512:Single-line comments should not be followed by blank line", Justification = "Вільне форматування коментарів.")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1005:Single line comment should begin with a space", Justification = "Дозволяємо коментарі без пробілу.")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should not be preceded by a space", Justification = "Вільне форматування дужок.")]