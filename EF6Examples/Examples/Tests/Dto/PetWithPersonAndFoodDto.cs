using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Tests
{
    class PetWithPersonAndFoodDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OwningPersonId { get; set; }
        public int FavoritePetFoodBrandId { get; set; }

        public PersonDto Owner { get; set; }
        public PetFoodBrandDto FavoritePetFoodBrand { get; set; }
    }
}
