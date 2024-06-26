using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class OrderItemDTO
    {
        public int ProductId { get; set; }

        public string Name { get; set; }

        public string PictureURL { get; set; }

        public long Price { get; set; }

        public int Quantity { get; set; }
    }
}