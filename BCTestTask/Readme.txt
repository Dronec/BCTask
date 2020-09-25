The change tracking mechanism is employing the SQL Server CDC.
For the sake of simplicity the following assumptions have been made:
1. MS SQL Server is installed on Localhost;
2. The user running the application has sufficient rights.

Setup:
1. Connect to SQL Server
2. Run sql_Setup.sql

Unit tests for C# part are in BCTestTaskTests project

Run the compiled code with the following parameters:
<servername> <databasename> <tablename> <FilePathToTheJSONLog>
Example: BCTestTask.exe localhost BCTestTask dbo.TestTable c:\dev\