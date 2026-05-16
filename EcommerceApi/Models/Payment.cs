namespace EcommerceApi.Models
{
    /// <summary>
    /// Represents a payment made by a customer for an order. It includes details such as the payment method, amount, and status.
    /// </summary>
    public class Payment
    {
        public int Id { get; set; }

        /// <summary>
        /// Foreign key referencing the order associated with this payment. This establishes a relationship between the Payment and Order entities, allowing us to track which payment corresponds to which order.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Payment amount for the order. This property holds the total amount paid by the customer for the order. It is crucial for processing payments and ensuring that the correct amount is charged to the customer.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Payment method used by the customer to complete the transaction. This could include options such as credit card, debit card,PayPal, bank transfer, etc. The payment method is important for processing the payment correctly and may also be used for reporting and analytics purposes.
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Payment status indicates the current state of the payment. It can have values such as "Pending", "Completed", "Failed", "Refunded". This property is essential for tracking the progress of the payment and determining whether the order can be fulfilled based on the payment status.
        /// </summary>
        public string PaymentStatus { get; set; } = PaymentStatus.Pending;

        /// <summary>
        /// Transaction ID is a unique identifier provided by the payment gateway or processor (Stripe, Paypal, etc) for the transaction. This ID can be used for tracking and referencing the payment in case of disputes, refunds, or for auditing purposes. It is important to store this information to maintain a record of the transaction and facilitate any necessary follow-up actions related to the payment.
        /// </summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// Error message if payment fails. This property is used to store any error messages returned by the payment gateway or processor in case the payment fails. It can provide valuable information for troubleshooting and resolving payment issues, as well as for communicating with the customer about the reason for the failure.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// When payment was initiated. This timestamp indicates when the payment process was started. It is important for tracking the timeline of the payment and can be used for reporting, analytics, and resolving any issues related to payment processing.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When payment was processed. This timestamp indicates when the payment was completed or failed. It is crucial for tracking the status of the payment and can be used for reporting, analytics, and resolving any issues related to payment processing. If the payment is still pending, this property may be null until the payment is processed.
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        public Order Order { get; set; } = null!;
    }

    /// <summary>
    /// PaymentStatus is an enumeration that defines the possible states of a payment. It includes values such as "Pending", "Completed", "Failed", and "Refunded". This enum is used to standardize the representation of payment statuses throughout the application, making it easier to manage and interpret the status of payments in a consistent manner.
    /// </summary>
    public enum PaymentStatus
    {
        Pending = 0,
        Completed =1,
        Failed=2,
        Refunded=3
    }
}
