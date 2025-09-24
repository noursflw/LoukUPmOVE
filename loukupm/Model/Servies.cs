using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loukupm.Model
{ 
    public class Servies
    {
        public int Id { get; set; }
        public string NameServies { get; set; }
        public double PriceServies { get; set; }
        public string Description { get; set; }
        public DateTime TimeServies { get; set; }
        public string? Image { get; set; }
        public string Catgery { get; set; }
        public Servies() { }
        public Servies(int id, string name, double price, string description, DateTime timeServies, string? image, string catgery)
        {
            Id = id;
            NameServies = name;
            PriceServies = price;
            Description = description;
            TimeServies = timeServies;
            Image = image;
            Catgery = catgery;
        }


    }
}
