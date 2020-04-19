using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YounesCo_Backend.Data;
using YounesCo_Backend.Models;

namespace YounesCo_Backend.Services
{
    public class InvoiceSender
    {
        private readonly ApplicationDbContext _db;

        public InvoiceSender(ApplicationDbContext db)
        {
            _db = db;
        }

        public void saveInvoice(string UserId, string Source, string Message)
        {
            //Invoice invoice = new Invoice
            //{
            //    CustomerId = UserId,
            //    Source = Source,
            //    Message = Message
            //};

            //_db.Invoices.Add(invoice);
            //_db.SaveChanges();
        }
    }
}
