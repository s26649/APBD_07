using System.Data.SqlClient;
using APBD_07.Models;

namespace APBD_07.Services
{
    public class WarehouseService : IWarehouseService
    {
        private const string ConnectionString = "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True";

        public async Task<int> AddProductToWarehouse(Warehouse warehouse)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();

            if (!await IfProductExists(connection, warehouse.IdProduct))
                throw new ArgumentException("Produkt o podanym identyfikatorze nie istnieje w bazie danych");
            if (!await IfWarehouseExists(connection, warehouse.IdWarehouse))
                throw new ArgumentException("Magazyn o podanym identyfikatorze nie istnieje w bazie danych");
            if (warehouse.Amount <= 0)
                throw new ArgumentException("Ilość powinna być większa od 0");

            var idOrder = await GetValidOrderId(connection, warehouse);
            if (idOrder == 0)
                throw new ArgumentException("Nie ma w bazie danych zamówienia spełniającego wymagania");

            decimal price = await GetProductPrice(connection, warehouse.IdProduct);
            await UpdateFulfilledAt(connection, idOrder, warehouse.CreatedAt);
            return await InsertToProductWarehouse(connection, warehouse, idOrder, price);
        }

        private async Task<bool> IfProductExists(SqlConnection connection, int id)
        {
            using var command = new SqlCommand("SELECT COUNT(1) FROM Product WHERE IdProduct = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            return (int)await command.ExecuteScalarAsync() > 0;
        }

        private async Task<bool> IfWarehouseExists(SqlConnection connection, int id)
        {
            using var command = new SqlCommand("SELECT COUNT(1) FROM Warehouse WHERE IdWarehouse = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            return (int)await command.ExecuteScalarAsync() > 0;
        }

        private async Task<int> GetValidOrderId(SqlConnection connection, Warehouse warehouse)
        {
            using var command = new SqlCommand(
                "SELECT IdOrder FROM [Order] WHERE IdProduct = @IdProduct AND Amount >= @Amount AND CreatedAt < @CreatedAt AND IdOrder NOT IN (SELECT IdOrder FROM Product_Warehouse)", connection);
            command.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
            command.Parameters.AddWithValue("@Amount", warehouse.Amount);
            command.Parameters.AddWithValue("@CreatedAt", warehouse.CreatedAt);
            object result = await command.ExecuteScalarAsync();
            return result != null ? (int)result : 0;
        }

        private async Task<decimal> GetProductPrice(SqlConnection connection, int idProduct)
        {
            using var command = new SqlCommand("SELECT Price FROM Product WHERE IdProduct = @Id", connection);
            command.Parameters.AddWithValue("@Id", idProduct);
            return (decimal)await command.ExecuteScalarAsync();
        }

        private async Task UpdateFulfilledAt(SqlConnection connection, int idOrder, DateTime createdAt)
        {
            using var command = new SqlCommand("UPDATE [Order] SET FulfilledAt = @CreatedAt WHERE IdOrder = @Id", connection);
            command.Parameters.AddWithValue("@Id", idOrder);
            command.Parameters.AddWithValue("@CreatedAt", createdAt);
            await command.ExecuteNonQueryAsync();
        }

        private async Task<int> InsertToProductWarehouse(SqlConnection connection, Warehouse warehouse, int idOrder, decimal price)
        {
            using var command = new SqlCommand(
                "INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) VALUES (@IdWare, @IdProd, @IdOrde, @Amount, @Price, @CreatedAt); SELECT SCOPE_IDENTITY();", connection);
            command.Parameters.AddWithValue("@IdWare", warehouse.IdWarehouse);
            command.Parameters.AddWithValue("@IdProd", warehouse.IdProduct);
            command.Parameters.AddWithValue("@IdOrde", idOrder);
            command.Parameters.AddWithValue("@Amount", warehouse.Amount);
            command.Parameters.AddWithValue("@Price", warehouse.Amount * price);
            command.Parameters.AddWithValue("@CreatedAt", warehouse.CreatedAt);
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }
    }
}
