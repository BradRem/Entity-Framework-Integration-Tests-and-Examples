using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Tests
{
    class PetDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OwningPersonId { get; set; }
        public int FavoritePetFoodBrandId { get; set; }
    }
}
