using APBD_07.Models;
using System.Data.SqlClient;

namespace APBD_07.Services
{
    public class WarehouseServiceWithStoredProcedure : IWarehouseServiceWithStoredProcedure
    {
        private const string ConnectionString = "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True";

        public async Task<int> AddProductUsingStoredProcedure(Warehouse warehouse)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();
            using var command = new SqlCommand("AddProductToWarehouse", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
            command.Parameters.AddWithValue("@IdWarehouse", warehouse.IdWarehouse);
            command.Parameters.AddWithValue("@Amount", warehouse.Amount);
            command.Parameters.AddWithValue("@CreatedAt", warehouse.CreatedAt);

            object result = await command.ExecuteScalarAsync();
            if (result != null)
            {
                return Convert.ToInt32(result);
            }
            throw new InvalidOperationException("Nie udało się dodac produktu za pomocą procedry składowanej.");
        }
    }
}