# Чёрный калькулятор

Настольный калькулятор на **C# / .NET 10 / Windows Forms**. Форма и фон дисплея — чёрные, кнопки — тёмно-серые, операции выделены оранжевым.

Возможности: сложение, вычитание, умножение, деление, дробные и отрицательные числа, повтор операции кнопкой `=`, очистка всего (`AC`) и текущего числа (`C`), удаление последней цифры, обработка деления на ноль и переполнения. Операции выполняются последовательно, слева направо: `2 + 3 × 4 = 20`.

## Запуск на Windows

Требуются Windows 10/11 и .NET 10 SDK (версия из `global.json`). Из корня репозитория:

```powershell
dotnet restore BlackCalculator.slnx --locked-mode
dotnet run --project src/BlackCalculator
```

В Visual Studio с поддержкой .NET 10 можно открыть `BlackCalculator.slnx` и выбрать `BlackCalculator` стартовым проектом.

## Клавиатура

| Клавиши | Действие |
| --- | --- |
| `0`–`9`, цифровой блок | Ввод числа |
| `+`, `-`, `*`, `/` | Арифметические операции |
| `.` или `,` | Десятичный разделитель текущей локали |
| `Enter` или `=` | Результат |
| `Esc` | Полный сброс |
| `Delete` | Очистка текущего числа |
| `Backspace` | Удаление последней цифры |
| `F9` | Изменение знака |
| `Ctrl+C` | Копирование результата |

## Сборка и тесты

```sh
dotnet restore BlackCalculator.slnx --locked-mode
dotnet build BlackCalculator.slnx --configuration Release --no-restore
dotnet test tests/BlackCalculator.Core.Tests --configuration Release --no-build
```

Вычислительная часть отделена от интерфейса и тестируется на Windows, Linux и macOS. `EnableWindowsTargeting` позволяет собрать приложение на Linux, но для запуска Windows Forms необходима Windows. Секреты, API-ключи и внешние сервисы не нужны.

## EXE без отдельной установки .NET

```powershell
dotnet publish src/BlackCalculator -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:NuGetLockFilePath=obj/publish.packages.lock.json -o artifacts/win-x64
```

Запустите `artifacts/win-x64/BlackCalculator.exe` на 64-битной Windows. Для сборки и публикации нужен доступ к `api.nuget.org` и серверам NuGet для загрузки пакетов.
