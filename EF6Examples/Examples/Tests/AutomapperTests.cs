using AutoMapper;
using Examples.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;

namespace Examples.Tests
{
    [TestFixture]
    class AutomapperTests
    {
        public const string PERSON_JANE = "Jane";
        public const string PERSON_BOB = "Bob";
        public const string PERSON_ANNA = "Anna";
        public const string PET_FLUFFY = "Fluffy";
        public const string PET_PUFFY = "Puffy";
        public const string PET_FIDO = "Fido";
        public const string FOOD_ALPHA = "Alpha Chow";
        public const string FOOD_BRAVO = "Bravo Food";

        [TestFixtureSetUp]
        public void Setup()
        {
            HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();

            InitializeAutomapper();

        }

        private static void InitializeAutomapper()
        {
            Mapper.CreateMap<PetFoodBrand, PetFoodBrandDto>();

            Mapper.CreateMap<Pet, PetDto>();

            Mapper.CreateMap<Person, PersonDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(x => x.Name))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.Id));

            Mapper.CreateMap<Person, PersonWithPetsDto>();

            Mapper.CreateMap<Pet, PetWithPersonAndFoodDto>();
        }

        [Test]
        public void MapFromBasicEntityToDtoOnlyLoadsRequestedFields()
        {
            using (ExampleDbContext dbContext = new ExampleDbContext())
            {
                dbContext.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);

                Person person = dbContext.Persons.Where(p => p.Name == PERSON_JANE).First();

                /*
                SELECT TOP (1) [Extent1].[Id]   AS [Id],
                [Extent1].[Name] AS [Name]
                FROM   [dbo].[Person] AS [Extent1]
                WHERE  N'Jane' = [Extent1].[Name]
                */
                var dto = Mapper.Map<PersonDto>(person);
            }
        }

        [Test]
        public void ProjectFromBasicEntityToDtoOnlyLoadsRequestedFields()
        {
            using (ExampleDbContext dbContext = new ExampleDbContext())
            {
                dbContext.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);

                var result = dbContext.Persons.Where(p => p.Name == PERSON_JANE);

                /*
                SELECT TOP (1) [Extent1].[Id]   AS [Id],
                [Extent1].[Name] AS [Name]
                FROM   [dbo].[Person] AS [Extent1]
                WHERE  N'Jane' = [Extent1].[Name]
                */
                var dto = result.Project().To<PersonDto>().First();
            }
        }

        [Test]
        public void MapFromEntityToDtoWithChildrenDoesNotEagerLoad()
        {
            using (ExampleDbContext dbContext = new ExampleDbContext())
            {
                dbContext.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);

                Person person = dbContext.Persons.Where(p => p.Name == PERSON_JANE).First();

                /*
                SELECT TOP (1) [Extent1].[Id]   AS [Id],
                [Extent1].[Name] AS [Name]
                FROM   [dbo].[Person] AS [Extent1]
                WHERE  N'Jane' = [Extent1].[Name]
                */
                var dto = Mapper.Map<PersonWithPetsDto>(person);

                /*
                SELECT [Extent1].[Id]                     AS [Id],
                        [Extent1].[Name]                   AS [Name],
                        [Extent1].[OwningPersonId]         AS [OwningPersonId],
                        [Extent1].[FavoritePetFoodBrandId] AS [FavoritePetFoodBrandId]
                FROM   [dbo].[Pet] AS [Extent1]
                WHERE  [Extent1].[OwningPersonId] = 1 
                */
                
                Assert.That(dto.Name, Is.Not.Null);
                Assert.That(dto.Pets.Count(), Is.GreaterThan(0));
            }
        }

        [Test]
        public void MapFromEntityWithIncludeToDtoHasSingleCall()
        {
            using (ExampleDbContext dbContext = new ExampleDbContext())
            {
                dbContext.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);

                Person person = dbContext.Persons.Include("Pets")
                    .Where(p => p.Name == PERSON_JANE)
                    .First();

                var dto = Mapper.Map<PersonWithPetsDto>(person);

                /*
                SELECT [Project1].[Id]                     AS [Id],
                       [Project1].[Name]                   AS [Name],
                       [Project1].[C1]                     AS [C1],
                       [Project1].[Id1]                    AS [Id1],
                       [Project1].[Name1]                  AS [Name1],
                       [Project1].[OwningPersonId]         AS [OwningPersonId],
                       [Project1].[FavoritePetFoodBrandId] AS [FavoritePetFoodBrandId]
                FROM   (SELECT [Limit1].[Id]                      AS [Id],
                               [Limit1].[Name]                    AS [Name],
                               [Extent2].[Id]                     AS [Id1],
                               [Extent2].[Name]                   AS [Name1],
                               [Extent2].[OwningPersonId]         AS [OwningPersonId],
                               [Extent2].[FavoritePetFoodBrandId] AS [FavoritePetFoodBrandId],
                               CASE
                                 WHEN ([Extent2].[Id] IS NULL) THEN CAST(NULL AS int)
                                 ELSE 1
                               END                                AS [C1]
                        FROM   (SELECT TOP (1) [Extent1].[Id]   AS [Id],
                                               [Extent1].[Name] AS [Name]
                                FROM   [dbo].[Person] AS [Extent1]
                                WHERE  N'Jane' = [Extent1].[Name]) AS [Limit1]
                               LEFT OUTER JOIN [dbo].[Pet] AS [Extent2]
                                 ON [Limit1].[Id] = [Extent2].[OwningPersonId]) AS [Project1]
                ORDER  BY [Project1].[Id] ASC,
                          [Project1].[C1] ASC
                */

                Assert.That(dto.Name, Is.Not.Null);
                Assert.That(dto.Pets.Count(), Is.GreaterThan(0));
            }
        }

        // using Project() is exactly like adding the Includes()
        [Test]
        public void ProjectFromEntityToDtoWithChildrenHasSingleCallForAllValues()
        {
            using (ExampleDbContext dbContext = new ExampleDbContext())
            {
                dbContext.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);

                var result = dbContext.Persons.Where(p => p.Name == PERSON_JANE);

                var dto = result.Project().To<PersonWithPetsDto>(result).First();

                /*
                SELECT [Project1].[Id]                     AS [Id],
                       [Project1].[Name]                   AS [Name],
                       [Project1].[C1]                     AS [C1],
                       [Project1].[Id1]                    AS [Id1],
                       [Project1].[Name1]                  AS [Name1],
                       [Project1].[OwningPersonId]         AS [OwningPersonId],
                       [Project1].[FavoritePetFoodBrandId] AS [FavoritePetFoodBrandId]
                FROM   (SELECT [Limit1].[Id]                      AS [Id],
                               [Limit1].[Name]                    AS [Name],
                               [Extent2].[Id]                     AS [Id1],
                               [Extent2].[Name]                   AS [Name1],
                               [Extent2].[OwningPersonId]         AS [OwningPersonId],
                               [Extent2].[FavoritePetFoodBrandId] AS [FavoritePetFoodBrandId],
                               CASE
                                 WHEN ([Extent2].[Id] IS NULL) THEN CAST(NULL AS int)
                                 ELSE 1
                               END                                AS [C1]
                        FROM   (SELECT TOP (1) [Extent1].[Id]   AS [Id],
                                               [Extent1].[Name] AS [Name]
                                FROM   [dbo].[Person] AS [Extent1]
                                WHERE  N'Jane' = [Extent1].[Name]) AS [Limit1]
                               LEFT OUTER JOIN [dbo].[Pet] AS [Extent2]
                                 ON [Limit1].[Id] = [Extent2].[OwningPersonId]) AS [Project1]
                ORDER  BY [Project1].[Id] ASC,
                          [Project1].[C1] ASC
                */

                Assert.That(dto.Name, Is.Not.Null);
                Assert.That(dto.Pets.Count(), Is.GreaterThan(0));
            }
        }

        [Test]
        public void MapEntityWithCollectionsMakesSeveralQueries()
        {
            using (ExampleDbContext dbContext = new ExampleDbContext())
            {
                var result = dbContext.Pets.Where(x => x.Name == PET_PUFFY).First();
                /*
                SELECT TOP (1) [Extent1].[Id]                     AS [Id],
                               [Extent1].[Name]                   AS [Name],
                               [Extent1].[OwningPersonId]         AS [OwningPersonId],
                               [Extent1].[FavoritePetFoodBrandId] AS [FavoritePetFoodBrandId]
                FROM   [dbo].[Pet] AS [Extent1]
                WHERE  N'Puffy' = [Extent1].[Name]
                 * * */

                var dto = Mapper.Map<PetWithPersonAndFoodDto>(result);

                /*
                SELECT [Extent1].[Id]   AS [Id],
                       [Extent1].[Name] AS [Name]
                FROM   [dbo].[Person] AS [Extent1]
                WHERE  [Extent1].[Id] = 1
                */


                /* 
                SELECT [Extent1].[Id]   AS [Id],
                       [Extent1].[Name] AS [Name]
                FROM   [dbo].[PetFoodBrand] AS [Extent1]
                WHERE  [Extent1].[Id] = 1                  
                */
            }
        }

        [Test]
        public void MapEntityWithCollectionsToDtoWithoutCollectionOnlyMapsEntity()
        {
            using (ExampleDbContext dbContext = new ExampleDbContext())
            {
                var result = dbContext.Pets.Where(x => x.Name == PET_PUFFY).First();
                /*
                SELECT TOP (1) [Extent1].[Id]                     AS [Id],
                               [Extent1].[Name]                   AS [Name],
                               [Extent1].[OwningPersonId]         AS [OwningPersonId],
                               [Extent1].[FavoritePetFoodBrandId] AS [FavoritePetFoodBrandId]
                FROM   [dbo].[Pet] AS [Extent1]
                WHERE  N'Puffy' = [Extent1].[Name]
                 * * */

                var dto = Mapper.Map<PetDto>(result);
            }
        }

        [Test]
        public void ProjectEntityWithCollectionsMakesSingleQuery()
        {
            using (ExampleDbContext dbContext = new ExampleDbContext())
            {
                var result = dbContext.Pets.Where(x => x.Name == PET_PUFFY);

                var dto = result.Project().To<PetWithPersonAndFoodDto>().First();

                /*
                SELECT TOP (1) [Extent1].[Id]                     AS [Id],
                                [Extent1].[Name]                   AS [Name],
                                [Extent1].[OwningPersonId]         AS [OwningPersonId],
                                [Extent1].[FavoritePetFoodBrandId] AS [FavoritePetFoodBrandId],
                                [Extent2].[Name]                   AS [Name1],
                                [Extent3].[Name]                   AS [Name2]
                FROM   [dbo].[Pet] AS [Extent1]
                        INNER JOIN [dbo].[Person] AS [Extent2]
                            ON [Extent1].[OwningPersonId] = [Extent2].[Id]
                        INNER JOIN [dbo].[PetFoodBrand] AS [Extent3]
                            ON [Extent1].[FavoritePetFoodBrandId] = [Extent3].[Id]
                WHERE  N'Puffy' = [Extent1].[Name]
                */
            }
        }

    }
}
