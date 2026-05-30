

namespace Infrastructure.Database.Models;

public class RequestCustomerModel
{
    public long Id { get; set; }
    public string CustomerName { get; set; }
    public string CustomerNumber { get; set; }
    public string PlateNo { get; set; }
    public bool IsPaymentDone { get; set; }
    public decimal? ServiceAmount { get; set; }

}
