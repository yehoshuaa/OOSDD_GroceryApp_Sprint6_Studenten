// Grocery.Core.Data/Repositories/ProductRepository.cs
using System.Globalization;
using Microsoft.Data.Sqlite;
using Grocery.Core.Data;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;

namespace Grocery.Core.Data.Repositories
{
    public class ProductRepository : DatabaseConnection, IProductRepository
    {
        public ProductRepository()
        {
            CreateTable(@"
CREATE TABLE IF NOT EXISTS Product (
  Id        INTEGER PRIMARY KEY AUTOINCREMENT,
  Name      TEXT    NOT NULL,
  Stock     INTEGER NOT NULL,
  ShelfLife TEXT    NULL,
  Price     REAL    NOT NULL
);
");
        }

        private static string? ToIso(DateOnly d) =>
            d == default ? null : d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        private static DateOnly FromIsoOrDefault(string? s) =>
            DateOnly.TryParseExact(s ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : default;

        public List<Product> GetAll()
        {
            var list = new List<Product>();
            OpenConnection();
            try
            {
                using var cmd = Connection.CreateCommand();
                cmd.CommandText = "SELECT Id, Name, Stock, ShelfLife, Price FROM Product ORDER BY Name;";
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    list.Add(new Product(
                        r.GetInt32(0),
                        r.GetString(1),
                        r.GetInt32(2),
                        FromIsoOrDefault(r.IsDBNull(3) ? null : r.GetString(3)),
                        Convert.ToDecimal(r.GetDouble(4))));
                }
            }
            finally { CloseConnection(); }
            return list;
        }

        public Product? Get(int id)
        {
            Product? p = null;
            OpenConnection();
            try
            {
                using var cmd = Connection.CreateCommand();
                cmd.CommandText = "SELECT Id, Name, Stock, ShelfLife, Price FROM Product WHERE Id=@id;";
                cmd.Parameters.AddWithValue("@id", id);
                using var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    p = new Product(
                        r.GetInt32(0),
                        r.GetString(1),
                        r.GetInt32(2),
                        FromIsoOrDefault(r.IsDBNull(3) ? null : r.GetString(3)),
                        Convert.ToDecimal(r.GetDouble(4)));
                }
            }
            finally { CloseConnection(); }
            return p;
        }

        public Product Add(Product item)
        {
            OpenConnection();
            try
            {
                using var cmd = Connection.CreateCommand();
                cmd.CommandText = @"
INSERT INTO Product (Name, Stock, ShelfLife, Price)
VALUES (@n, @s, @life, @p);
SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@n", item.Name);
                cmd.Parameters.AddWithValue("@s", item.Stock);
                cmd.Parameters.AddWithValue("@life", (object?)ToIso(item.ShelfLife) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p", item.Price);
                item.Id = Convert.ToInt32(cmd.ExecuteScalar());
                return item;
            }
            finally { CloseConnection(); }
        }

        public Product? Update(Product item)
        {
            OpenConnection();
            try
            {
                using var cmd = Connection.CreateCommand();
                cmd.CommandText = @"
UPDATE Product SET Name=@n, Stock=@s, ShelfLife=@life, Price=@p
WHERE Id=@id;";
                cmd.Parameters.AddWithValue("@n", item.Name);
                cmd.Parameters.AddWithValue("@s", item.Stock);
                cmd.Parameters.AddWithValue("@life", (object?)ToIso(item.ShelfLife) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p", item.Price);
                cmd.Parameters.AddWithValue("@id", item.Id);
                return cmd.ExecuteNonQuery() > 0 ? item : null;
            }
            finally { CloseConnection(); }
        }

        public Product? Delete(Product item)
        {
            OpenConnection();
            try
            {
                using var cmd = Connection.CreateCommand();
                cmd.CommandText = "DELETE FROM Product WHERE Id=@id;";
                cmd.Parameters.AddWithValue("@id", item.Id);
                return cmd.ExecuteNonQuery() > 0 ? item : null;
            }
            finally { CloseConnection(); }
        }
    }
}
