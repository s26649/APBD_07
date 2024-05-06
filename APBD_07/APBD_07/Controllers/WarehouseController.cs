using APBD_07.Models;
using APBD_07.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace APBD_07.Controllers;

[Route("api/warehouse")]
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _service;
    private readonly IWarehouseServiceWithStoredProcedure _storedProcedureService;

    // Dependency Injection for both services
    public WarehouseController(IWarehouseService service, IWarehouseServiceWithStoredProcedure storedProcedureService)
    {
        _service = service;
        _storedProcedureService = storedProcedureService;
    }

    [HttpPost]
    public async Task<IActionResult> AddProductToWarehouse([FromBody] Warehouse warehouse)
    {
        try
        {
            int newId = await _service.AddProductToWarehouse(warehouse);
            return Ok(new { message = "Produkt został dodany do magazynu", id = newId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("AddWithProcedure")]
    public async Task<IActionResult> AddProductWithProcedure([FromBody] Warehouse warehouse)
    {
        try
        {
            int newId = await _storedProcedureService.AddProductUsingStoredProcedure(warehouse);
            return Ok(new { message = "Produkt został dodany do magazynu za pomocą procedury składowanej", id = newId });
        }
        catch (SqlException ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}