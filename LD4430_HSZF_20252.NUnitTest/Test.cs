using LD4430_HSZF_2024252.Application;
using LD4430_HSZF_2024252.Model;
using LD4430_HSZF_2024252.Persistence.MsSql;
using Moq;
using NUnit;

namespace LD4430_HSZF_20252.Test
{
    namespace LD4430_HSZF_2024252.Test
    {
        [TestFixture]
        public class ProductServiceTest
        {
            private Mock<IProductDataProvider> _productDataProviderMock;
            private Mock<IFavoriteProductDataProvider> _favoriteProductDataProviderMock;
            private Mock<ICapacityDataProvider> _capacityDataProviderMock;
            private Mock<ICapacityService> _capacityServiceProviderMock;
            private ProductService _productService;

            [SetUp]
            public void Setup()
            {
                _productDataProviderMock = new Mock<IProductDataProvider>();
                _favoriteProductDataProviderMock = new Mock<IFavoriteProductDataProvider>();
                _capacityDataProviderMock = new Mock<ICapacityDataProvider>();
                _capacityServiceProviderMock = new Mock<ICapacityService>();
                _productService = new ProductService(
                    _productDataProviderMock.Object,
                    _favoriteProductDataProviderMock.Object,
                    _capacityServiceProviderMock.Object
                );
            }

            [Test]
            public void GetAllProducts_ShouldReturnCorrectCount()
            {
                var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product1", Quantity = 10, CriticalLevel = 5 },
                new Product { Id = 2, Name = "Product2", Quantity = 3, CriticalLevel = 5 }
            };
                _productDataProviderMock.Setup(x => x.GetAllProducts()).Returns(products);

                var result = _productService.GetAllProducts();

                Assert.That(result.Count(), Is.EqualTo(2));
            }

            [Test]
            public void DeleteProduct_ShouldThrow_WhenProductDoesNotExist()
            {
                int productId = 1;
                _productDataProviderMock.Setup(p => p.Exists(productId)).Returns(false);

                var exception = Assert.Throws<InvalidOperationException>(() => _productService.DeleteProduct(productId));

                Assert.That(exception.Message, Is.EqualTo("Product with ID 1 not found."));
            }

            [Test]
            public void UpdateProduct_ShouldRaiseCriticalEvent_WhenBelowThreshold()
            {
                var product = new Product { Id = 1, Name = "Tej", Quantity = 2, CriticalLevel = 3 };
                _productDataProviderMock.Setup(x => x.Exists(product.Id)).Returns(true);
                _favoriteProductDataProviderMock.Setup(x => x.GetPersonsByFavoriteProductId(product.Id))
                    .Returns(new List<Person> { new Person { Id = 10, Name = "Józsi" } });

                ProductEventArgs? eventArgs = null;
                _productService.OnProductCritical += (s, e) => eventArgs = e;

                _productService.UpdateProduct(product);

                Assert.IsNotNull(eventArgs);
                Assert.That(eventArgs.ProductName, Is.EqualTo("Tej"));
                Assert.That(eventArgs.PersonName, Is.EqualTo("Józsi"));
                Assert.That(eventArgs.CriticalLevel, Is.EqualTo(3));
            }
        }

        [TestFixture]
        public class PersonServiceTest
        {
            private Mock<IPersonDataProvider> _personDataProviderMock;
            private PersonService _personService;

            [SetUp]
            public void Setup()
            {
                _personDataProviderMock = new Mock<IPersonDataProvider>();
                _personService = new PersonService(_personDataProviderMock.Object);
            }

            [Test]
            public void AddPerson_ShouldSucceed_WhenPersonDoesNotExist()
            {
                var person = new Person { Id = 1, Name = "Anna" };
                _personDataProviderMock.Setup(p => p.Exists(person.Id)).Returns(false);

                _personService.AddPerson(person);

                _personDataProviderMock.Verify(p => p.AddPerson(person), Times.Once);
            }

            [Test]
            public void DeletePerson_ShouldSucceed_WhenPersonExists()
            {
                int personId = 1;
                _personDataProviderMock.Setup(p => p.Exists(personId)).Returns(true);

                _personService.DeletePerson(personId);

                _personDataProviderMock.Verify(p => p.DeletePerson(personId), Times.Once);
            }
        }

        [TestFixture]
        public class FavoriteProductServiceTest
        {
            private Mock<IFavoriteProductDataProvider> _favoriteProductDataProviderMock;
            private FavoriteProductService _favoriteProductService;
            private Mock<IPersonDataProvider> _personDataProviderMock;
            private Mock<IProductDataProvider> _productDataProviderMock;

            [SetUp]
            public void Setup()
            {
                _favoriteProductDataProviderMock = new Mock<IFavoriteProductDataProvider>();
                _personDataProviderMock = new Mock<IPersonDataProvider>();
                _productDataProviderMock = new Mock<IProductDataProvider>();
                _favoriteProductService = new FavoriteProductService(_favoriteProductDataProviderMock.Object, _productDataProviderMock.Object, _personDataProviderMock.Object);
            }

            [Test]
            public void AddFavorite_ShouldThrow_WhenAlreadyExists()
            {
                int personId = 1, productId = 2;
                _favoriteProductDataProviderMock.Setup(x => x.Exists(personId, productId)).Returns(true);

                var ex = Assert.Throws<InvalidOperationException>(() => _favoriteProductService.AddFavorite(personId, productId));

                Assert.That(ex.Message, Is.EqualTo("Ez a kedvenc már létezik."));
            }

            [Test]
            public void AddFavorite_ShouldSucceed_WhenNotExists()
            {
                int personId = 1, productId = 2;
                _favoriteProductDataProviderMock.Setup(x => x.Exists(personId, productId)).Returns(false);

                _favoriteProductService.AddFavorite(personId, productId);

                _favoriteProductDataProviderMock.Verify(x => x.AddFavorite(personId, productId), Times.Once);
            }
        }

        [TestFixture]
        public class CapacityServiceTest
        {
            private Mock<ICapacityDataProvider> _capacityDataProviderMock;
            private CapacityService _capacityService;
            private Mock<IProductDataProvider> _productDataProviderMock;
            private Mock<IFavoriteProductDataProvider> _favoriteProductDataProviderMock;
            private ProductService _productService;
            private Mock<ICapacityService> _capacityServiceProviderMock;

            [SetUp]
            public void Setup()
            {
                _productDataProviderMock = new Mock<IProductDataProvider>();
                _favoriteProductDataProviderMock = new Mock<IFavoriteProductDataProvider>();
                _capacityDataProviderMock = new Mock<ICapacityDataProvider>();
                _capacityServiceProviderMock = new Mock<ICapacityService>();

                _capacityService = new CapacityService(_capacityDataProviderMock.Object, _productDataProviderMock.Object);
                _productService = new ProductService(_productDataProviderMock.Object, _favoriteProductDataProviderMock.Object, _capacityServiceProviderMock.Object);
            }

            [Test]
            public void FridgeCapacity_ShouldBeFull_WhenProductQuantitiesMatchCapacity()
            {
                _capacityServiceProviderMock.Setup(c => c.GetFridgeCapacity()).Returns(10);
                var products = new List<Product>
            {
                new Product { Name = "Product1", Id = 1, Quantity = 10, StoreInFridge = true },
                new Product { Name = "Product2", Id = 2, Quantity = 0, StoreInFridge = true }
            };
                _productDataProviderMock.Setup(p => p.GetAllProducts()).Returns(products);

                bool full = _capacityService.IsFridgeFull();

                Assert.IsTrue(full);
            }

            [Test]
            public void PantryCapacity_ShouldBeFull_WhenProductQuantitiesMatchCapacity()
            {
                _capacityServiceProviderMock.Setup(c => c.GetPantryCapacity()).Returns(10);
                var products = new List<Product>
            {
                new Product { Name = "Product1", Id = 1, Quantity = 10, StoreInFridge = false },
                new Product { Name = "Product2", Id = 2, Quantity = 0, StoreInFridge = false }
            };
                _productDataProviderMock.Setup(p => p.GetAllProducts()).Returns(products);

                bool full = _capacityService.IsPantryFull();

                Assert.IsTrue(full);
            }

            [Test]
            public void AddProduct_ShouldThrow_WhenExceedsFridgeCapacity()
            {
                _capacityServiceProviderMock.Setup(c => c.GetFridgeCapacity()).Returns(10);
                var product = new Product { Name = "Product1", Quantity = 100, StoreInFridge = true };

                var ex = Assert.Throws<StorageUnitFullException>(() => _productService.AddProduct(product));

                Assert.That(ex.Message, Is.EqualTo("Nem lehet hozzáadni a terméket: nincs elég hely a hűtőben."));
            }
        }
    }
}