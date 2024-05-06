using APBD_07.Models;

namespace APBD_07.Services;

public interface IWarehouseService
{
    Task<int> AddProductToWarehouse(Warehouse warehouseData);
}