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
            return true;
        }

        private bool WarehouseExists(int warehouseId, SqlConnection connection)
        {
            return true;
        }

        private int GetOrder(Warehouse request, SqlConnection connection)
        {
            return 1;
        }

        private bool IsOrderFulfilled(int orderId, SqlConnection connection)
        {
            return false;
        }

        private void UpdateOrderFulfilledAt(int orderId, SqlConnection connection)
        {
        }

        private int InsertProductWarehouse(int orderId, Warehouse request, SqlConnection connection)
        {
            return 1;
        }
    }
}