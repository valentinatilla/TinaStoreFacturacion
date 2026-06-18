using Microsoft.Data.Sqlite;
using System;
var conn = new SqliteConnection("Data Source=tinastore-dev.db");
conn.Open();
var cmd = conn.CreateCommand();
cmd.CommandText = "SELECT Id, COALESCE(InternalCode,'[VACIO]'), COALESCE(Sku,'[VACIO]'), Name FROM Products WHERE IsDeleted=0 LIMIT 20";
var r = cmd.ExecuteReader();
int total = 0;
while (r.Read()) { Console.WriteLine($"ID:{r[0]} | InternalCode:'{r[1]}' | SKU:'{r[2]}' | Nombre:'{r[3]}'"); total++; }
Console.WriteLine($"Total productos: {total}");
conn.Close();
