using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicShop.Common.Models
{
    public enum Role : byte { Admin = 1, Staff = 2, Customer = 3 }

    public enum OrderStatus : byte { Pending = 1, Paid = 2, Cancelled = 3, Shipped = 4 }
    public enum PaymentMethod : byte { Cod = 1, Bank = 2, Momo = 3, VnPay = 4 }
    public enum PaymentStatus : byte { Init = 1, Success = 2, Failed = 3, Refunded = 4 }

    public enum ItemStatus : byte { Draft = 0, Active = 1, Inactive = 2 }
    public enum ItemType : byte { Instrument = 1, Accessory = 2, Service = 3 }

    public enum EmailStatus : byte { Queued = 1, Sending = 2, Sent = 3, Failed = 4 }
    public enum TokenType : byte { VerifyEmail = 1, ResetPassword = 2 }

    public enum InventoryReason : byte { Import = 1, Sale = 2, CancelOrder = 3, ManualAdjust = 4 }

}
