using APBD_07.Models;
using System.Threading.Tasks;

namespace APBD_07.Services
{
    public interface IWarehouseServiceWithStoredProcedure
    {
        Task<int> AddProductUsingStoredProcedure(Warehouse warehouse);
    }
}