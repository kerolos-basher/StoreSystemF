namespace Domain.DeferredPaymentAggregate;

public sealed class DeferredPayment : ParentEntity
{
    public long SalesInvoiceId { get; private set; }
    public long CustomerId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public decimal RemainingAmount { get; private set; }
    public bool IsFullyPaid { get; private set; }
    public string Notes { get; private set; } = string.Empty;

    private readonly List<DeferredPaymentTransaction> _transactions = new();
    public IReadOnlyCollection<DeferredPaymentTransaction> Transactions => _transactions.AsReadOnly();

    private DeferredPayment()
    {
    }

    private DeferredPayment(
        long id,
        long salesInvoiceId,
        long customerId,
        decimal totalAmount,
        string? notes)
    {
        EnsureValidId(id);
        Id = id;
        SalesInvoiceId = salesInvoiceId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        PaidAmount = 0;
        RemainingAmount = totalAmount;
        IsFullyPaid = false;
        Notes = notes?.Trim() ?? string.Empty;
    }

    public static DeferredPayment Create(
        long id,
        long salesInvoiceId,
        long customerId,
        decimal totalAmount,
        string? notes)
    {
        if (salesInvoiceId <= 0)
            throw new Exception("معرف الفاتورة غير صالح.");

        if (customerId <= 0)
            throw new Exception("معرف العميل غير صالح.");

        if (totalAmount <= 0)
            throw new Exception("المبلغ الإجمالي يجب أن يكون أكبر من صفر.");

        return new DeferredPayment(id, salesInvoiceId, customerId, totalAmount, notes);
    }

    public void RegisterPayment(long transactionId, decimal amountPaid, string? notes, long createdBy)
    {
        if (amountPaid <= 0)
            throw new Exception("المبلغ المدفوع يجب أن يكون أكبر من صفر.");

        if (IsFullyPaid)
            throw new Exception("تم سداد هذه الفاتورة بالكامل.");

        if (amountPaid > RemainingAmount)
            throw new Exception($"المبلغ المدفوع ({amountPaid}) أكبر من المتبقي ({RemainingAmount}).");

        PaidAmount += amountPaid;
        RemainingAmount = TotalAmount - PaidAmount;
        IsFullyPaid = RemainingAmount <= 0;

        _transactions.Add(DeferredPaymentTransaction.Create(
            transactionId,
            Id,
            amountPaid,
            notes,
            createdBy));

        MarkUpdated();
    }

    public void UpdateTransaction(long transactionId, decimal amountPaid, string? notes)
    {
        var transaction = _transactions.FirstOrDefault(x => x.Id == transactionId)
            ?? throw new StoreException("الدفعة غير موجودة.");

        if (amountPaid <= 0)
            throw new StoreException("المبلغ المدفوع يجب أن يكون أكبر من صفر.");

        var otherPaid = PaidAmount - transaction.AmountPaid;
        var newTotalPaid = otherPaid + amountPaid;

        if (newTotalPaid > TotalAmount)
            throw new StoreException($"المبلغ المدفوع ({newTotalPaid}) أكبر من إجمالي الفاتورة ({TotalAmount}).");

        transaction.Update(amountPaid, notes);
        PaidAmount = newTotalPaid;
        RemainingAmount = TotalAmount - PaidAmount;
        IsFullyPaid = RemainingAmount <= 0;
        MarkUpdated();
    }

    public void DeleteTransaction(long transactionId)
    {
        var transaction = _transactions.FirstOrDefault(x => x.Id == transactionId)
            ?? throw new StoreException("الدفعة غير موجودة.");

        PaidAmount -= transaction.AmountPaid;
        RemainingAmount = TotalAmount - PaidAmount;
        IsFullyPaid = RemainingAmount <= 0;
        _transactions.Remove(transaction);
        MarkUpdated();
    }

    public void AdjustTotalForReturn(decimal returnAmount)
    {
        if (returnAmount <= 0)
            return;

        TotalAmount = Math.Max(0, TotalAmount - returnAmount);

        if (PaidAmount > TotalAmount)
            PaidAmount = TotalAmount;

        RemainingAmount = TotalAmount - PaidAmount;
        IsFullyPaid = RemainingAmount <= 0;
        MarkUpdated();
    }

    public void SyncInvoiceTotal(decimal newTotal)
    {
        if (newTotal <= 0)
            throw new Exception("إجمالي الفاتورة يجب أن يكون أكبر من صفر.");

        TotalAmount = newTotal;

        if (PaidAmount > TotalAmount)
            PaidAmount = TotalAmount;

        RemainingAmount = TotalAmount - PaidAmount;
        IsFullyPaid = RemainingAmount <= 0;
        MarkUpdated();
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        MarkUpdated();
    }
}
