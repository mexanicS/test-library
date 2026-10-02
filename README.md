# Домашняя библиотека

Небольшое приложение для учёта книг на ASP.NET Core MVC (.NET 10) и SQL Server. Данные обрабатываются хранимыми процедурами, оглавление хранится в XML.

## Запуск

Нужны .NET 10 SDK, SQL Server Express LocalDB и `sqlcmd`. Из корня проекта:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -I -f 65001 -i database\setup.sql
if ($LASTEXITCODE -ne 0) { throw "Не удалось подготовить БД" }

dotnet run --project src\LibraryCatalog.Web --launch-profile http
```
