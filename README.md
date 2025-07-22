## 📦 Database Setup

1. Open SQL Server Management Studio (SSMS).
2. Run the `Database/PIA_SRM_Database.sql` script to generate the database with tables.
3. Update the connection string in:
   `Web.config`

Example connection string:
```
<connectionStrings>
  <add name="DefaultConnection" 
       connectionString="Data Source=YOUR_SERVER_NAME;Initial Catalog=PIA_SRM;Integrated Security=True;" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```
