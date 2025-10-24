using System.Data;
using Microsoft.Data.Sqlite;
using Grocery.Core.Data;
using Grocery.Core.Data.Helpers;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;

namespace Grocery.Core.Data.Repositories
{
    /// UC18: SQLite implementatie voor GroceryListItem.
    public class GroceryListItemsRepository : DatabaseConnection, IGroceryListItemsRepository
    {
        public GroceryListItemsRepository()
        {
            CreateTable(@"
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS GroceryListItem (
    Id            INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    GroceryListId INTEGER NOT NULL,
    ProductId     INTEGER NOT NULL,
    Amount        INTEGER NOT NULL,
    UNIQUE (GroceryListId, ProductId)
);
CREATE INDEX IF NOT EXISTS IX_GroceryListItem_ListId ON GroceryListItem(GroceryListId);
");
            SeedIfEmpty();
        }

        private void SeedIfEmpty()
        {
            OpenConnection();
            try
            {
                using var count = Connection.CreateCommand();
                count.CommandText = "SELECT COUNT(*) FROM GroceryListItem;";
                if (Convert.ToInt32(count.ExecuteScalar()) > 0) return;

                using var tx = Connection.BeginTransaction();
                using var cmd = Connection.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES
(1,1,3),(1,2,1),(1,3,4),(2,1,2),(2,2,5);";
                cmd.ExecuteNonQuery();
                tx.Commit();
            }
            finally { CloseConnection(); }
        }

        public List<GroceryListItem> GetAll()
        {
            var list = new List<GroceryListItem>();
            OpenConnection();
            try
            {
                using var cmd = Connection.CreateCommand();
                cmd.CommandText = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem;";
                using var r = cmd.ExecuteReader();
                while (r.Read())
                    list.Add(new GroceryListItem(r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetInt32(3)));
            }
            finally { CloseConnection(); }
            return list;
        }

        public List<GroceryListItem> GetAllOnGroceryListId(int listId)
        {
            var list = new List<GroceryListItem>();
            OpenConnection();
            try
            {
                using var cmd = Connection.CreateCommand();
                cmd.CommandText = @"SELECT Id, GroceryListId, ProductId, Amount
                                    FROM GroceryListItem WHERE GroceryListId=@lid;";
                cmd.Parameters.AddWithValue("@lid", listId);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                    list.Add(new GroceryListItem(r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetInt32(3)));
            }
            finally { CloseConnection(); }
            return list;
        }

        public GroceryListItem Add(GroceryListItem item)
        {
            OpenConnection();
            try
            {
                // extra defensieve check naast UNIQUE-constraint
                using (var check = Connection.CreateCommand())
                {
                    check.CommandText = "SELECT COUNT(*) FROM GroceryListItem WHERE GroceryListId=@l AND ProductId=@p;";
                    check.Parameters.AddWithValue("@l", item.GroceryListId);
                    check.Parameters.AddWithValue("@p", item.ProductId);
                    if (Convert.ToInt32(check.ExecuteScalar()) > 0)
                        throw new InvalidOperationException("Product bestaat al in deze boodschappenlijst.");
                }

                using var cmd = Connection.CreateCommand();
                cmd.CommandText = @"
INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount)
VALUES (@l, @p, @a);
SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@l", item.GroceryListId);
                cmd.Parameters.AddWithValue("@p", item.ProductId);
                cmd.Parameters.AddWithValue("@a", item.Amount);
                item.Id = Convert.ToInt32(cmd.ExecuteScalar());
                return item;
            }
            finally { CloseConnection(); }
        }

        public GroceryListItem? Update(GroceryListItem item)
        {
            OpenConnection();
            try
            {
                using var cmd = Connection.CreateCommand();
                cmd.CommandText = @"UPDATE GroceryListItem
                                    SET GroceryListId=@l, ProductId=@p, Amount=@a
                                    WHERE Id=@id;";
                cmd.Parameters.AddWithValue("@id", item.Id);
                cmd.Parameters.AddWithValue("@l", item.GroceryListId);
                cmd.Parameters.AddWithValue("@p", item.ProductId);
                cmd.Parameters.AddWithValue("@a", item.Amount);
                return cmd.ExecuteNonQuery() > 0 ? item : null;
            }
            finally { CloseConnection(); }
        }

        public GroceryListItem? Delete(GroceryListItem item)
        {
            OpenConnection();
            try
            {
                using var cmd = Connection.CreateCommand();
                cmd.CommandText = "DELETE FROM GroceryListItem WHERE Id=@id;";
                cmd.Parameters.AddWithValue("@id", item.Id);
                return cmd.ExecuteNonQuery() > 0 ? item : null;
            }
            finally { CloseConnection(); }
        }

        public GroceryListItem? Get(int id)
        {
            OpenConnection();
            try
            {
                using var cmd = Connection.CreateCommand();
                cmd.CommandText = @"SELECT Id, GroceryListId, ProductId, Amount
                                    FROM GroceryListItem WHERE Id=@id;";
                cmd.Parameters.AddWithValue("@id", id);
                using var r = cmd.ExecuteReader();
                return r.Read() ? new GroceryListItem(r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetInt32(3)) : null;
            }
            finally { CloseConnection(); }
        }
    }
}
