using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Tests
{
    class PersonWithPetsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<PetDto> Pets { get; set; }

        public PersonWithPetsDto()
        {
            Pets = new List<PetDto>();
        }
    }
}
