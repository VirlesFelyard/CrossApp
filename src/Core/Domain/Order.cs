namespace Core.Domain;

public sealed class Order
{
    private readonly string _id;
    private OrderStatus _status;

    public string Id => _id;
    public OrderStatus Status => _status;

    private Order(string id)
    {
        _id = id;
        _status = OrderStatus.Draft;
    }

    public static Order Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(
                "Ідентифікатор замовлення не може бути порожнім",
                nameof(id));
        }

        return new Order(id.Trim());
    }

    public void ChangeStatus(OrderStatus newStatus)
    {
        bool allowed = (_status, newStatus) switch
        {
            (OrderStatus.Draft, OrderStatus.Confirmed) => true,
            (OrderStatus.Draft, OrderStatus.Cancelled) => true,
            _ => false
        };

        if (!allowed)
        {
            throw new InvalidOperationException(
                $"Перехід зі стану '{_status}' у стан '{newStatus}' заборонений");
        }

        _status = newStatus;
    }

    public override string ToString() =>
        $"{Id} | Статус: {Status}";
}