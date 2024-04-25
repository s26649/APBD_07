using System.Data.SqlClient;
using APBD_07.Models;
using Microsoft.AspNetCore.Mvc;

namespace APBD_07.Controllers
{
    [ApiController]
    [Route("api/warehouses")]
    public class WarehouseController : ControllerBase
    {
        private readonly string _connectionString = "Data Source = db-mssql;Initial Catalog=2019SBD; Integrated Security=True";

        [HttpPost]
        public IActionResult AddProductToWarehouse([FromBody] Warehouse request)
        {
            if (request.Amount <= 0) return BadRequest("ilosc ma byc wiesza od 0 dumbahh.");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    if (!ProductExists(request.IdProduct, connection) || !WarehouseExists(request.IdWarehouse, connection)) return NotFound("nie ma produktu albo warehousu.");

                    var orderId = GetOrder(request, connection);
                    if (orderId == 0) return BadRequest("nie ma orderu dla tego produktu.");

                    if (IsOrderFulfilled(orderId, connection)) return BadRequest("order juz byl.");

                    UpdateOrderFulfilledAt(orderId, connection);

                    var productWarehouseId = InsertProductWarehouse(orderId, request, connection);
                    if (productWarehouseId == 0) return BadRequest("nie wyszlo dodac produkt do warehouse.");

                    return Ok(new { IdProductWarehouse = productWarehouseId });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "server error: " + ex.Message);
            }
        }

        private bool ProductExists(int productId, SqlConnection connection)
        {
            var query = "SELECT COUNT(1) FROM Product WHERE IdProduct = @IdProduct";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IdProduct", productId);
                var exists = (int)command.ExecuteScalar();
                return exists > 0;
            }
        }

        private bool WarehouseExists(int warehouseId, SqlConnection connection)
        {
            var query = "SELECT COUNT(1) FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IdWarehouse", warehouseId);
                var exists = (int)command.ExecuteScalar();
                return exists > 0;
            }
        }

        private int GetOrder(Warehouse request, SqlConnection connection)
        {
            var query = @"
                SELECT TOP 1 IdOrder FROM [Order] 
                WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt";
            
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                command.Parameters.AddWithValue("@Amount", request.Amount);
                command.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);
                var orderId = command.ExecuteScalar();
                return (orderId != null) ? (int)orderId : 0;
            }
        }


        private bool IsOrderFulfilled(int orderId, SqlConnection connection)
        {
            var query = "SELECT COUNT(1) FROM Product_Warehouse WHERE IdOrder = @IdOrder";
            
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IdOrder", orderId);
                var exists = (int)command.ExecuteScalar();
                return exists > 0;
            }
        }

        private void UpdateOrderFulfilledAt(int orderId, SqlConnection connection)
        {
            var query = "UPDATE [Order] SET FulfilledAt = GETDATE() WHERE IdOrder = @IdOrder";
            
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IdOrder", orderId);
                command.ExecuteNonQuery();
            }
        }

        private int InsertProductWarehouse(int orderId, Warehouse request, SqlConnection connection)
        {
            var query = @"
                INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) 
                OUTPUT INSERTED.IdProductWarehouse 
                VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, 
                        (SELECT Price FROM Product WHERE IdProduct = @IdProduct) * @Amount, 
                        @CreatedAt)";
            
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                command.Parameters.AddWithValue("@IdOrder", orderId);
                command.Parameters.AddWithValue("@Amount", request.Amount);
                command.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);
        
                var insertedId = (int)command.ExecuteScalar();
                return insertedId;
            }
        }

    }
}