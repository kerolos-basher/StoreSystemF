global using System.Reflection;
global using System.Collections.Generic;
global using System.Globalization;
global using System.Text;
global using System;
//dotnet ef migrations add commites --project DBMigration --startup-project DBMigration_Api
//dotnet ef database update --project DBMigration --startup-project DBMigration_Api
//--او
//dotnet ef migrations add commites --project ../DBMigration --startup-project ../DBMigration_Api
//dotnet ef database update --project ../DBMigration --startup-project ../DBMigration_Api
//cd Migrations
//cd DBMigration_Api
//docker run --name redis-local -p 6379:6379 -d redis