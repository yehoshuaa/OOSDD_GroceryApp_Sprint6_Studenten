using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using Grocery.Core.Services;

namespace Tests.UC19
{
    // Eenvoudige fake-repository [nep-opslaglaag] voor unit tests
    class FakeProductRepository : IProductRepository
    {
        private readonly List<Product> _items = new();
        private int _nextId = 1;

        public List<Product> GetAll() => _items.ToList();
        public Product? Get(int id) => _items.FirstOrDefault(p => p.Id == id);

        public Product Add(Product item)
        {
            item.Id = _nextId++;
            _items.Add(item);
            return item;
        }

        public Product? Update(Product item)
        {
            var i = _items.FindIndex(p => p.Id == item.Id);
            if (i < 0) return null;
            _items[i] = item;
            return item;
        }

        public Product? Delete(Product item)
        {
            return _items.RemoveAll(p => p.Id == item.Id) > 0 ? item : null;
        }
    }

    // (optioneel) Fake AuthService als je ProductService autorisatie zou gebruiken
    class DummyAuthService : IAuthService
    {
        public Client? CurrentUser { get; } = new(1, "Test", "t@t.nl", "x");
        public Client? Login(string email, string password) => CurrentUser;
        // Als je IAuthService geen CurrentUser heeft in jouw project: laat deze class weg.
    }

    [TestFixture]
    public class ProductServiceTests
    {
        private IProductRepository _repo = null!;
        private IProductService _svc = null!;

        [SetUp]
        public void Setup()
        {
            _repo = new FakeProductRepository();
            // Gebruik de ProductService-versie die JIJ nu hebt (zonder role-check):
            _svc = new ProductService(_repo);
        }

        [Test]
        public void TC19_01_Add_HappyPath_ShouldAssignId_AndBeRetrievable()
        {
            // Arrange
            var input = new Product(0, "Cola 33cl", stock: 10, shelfLife: default, price: 0.95m);

            // Act
            var created = _svc.Add(input);

            // Assert
            Assert.That(created.Id, Is.GreaterThan(0), "Id moet gezet worden door de repository.");
            var all = _svc.GetAll();
            Assert.That(all.Any(p => p.Name == "Cola 33cl" && p.Price == 0.95m && p.Stock == 10), Is.True);
        }

        [Test]
        public void TC19_02_Add_EmptyName_ShouldThrow()
        {
            var input = new Product(0, "", stock: 1, shelfLife: default, price: 1.00m);
            var ex = Assert.Throws<ArgumentException>(() => _svc.Add(input));
            StringAssert.Contains("Naam is verplicht", ex!.Message);
        }

        [Test]
        public void TC19_03_Add_PriceNotPositive_ShouldThrow()
        {
            var zeroPrice = new Product(0, "Fanta", stock: 1, shelfLife: default, price: 0m);
            var negPrice = new Product(0, "Sprite", stock: 1, shelfLife: default, price: -1m);

            var ex1 = Assert.Throws<ArgumentException>(() => _svc.Add(zeroPrice));
            var ex2 = Assert.Throws<ArgumentException>(() => _svc.Add(negPrice));

            StringAssert.Contains("Prijs moet groter zijn dan 0", ex1!.Message);
            StringAssert.Contains("Prijs moet groter zijn dan 0", ex2!.Message);
        }

        [Test]
        public void TC19_04_Add_NegativeStock_ShouldThrow()
        {
            var input = new Product(0, "AA Drink", stock: -5, shelfLife: default, price: 1.20m);
            var ex = Assert.Throws<ArgumentException>(() => _svc.Add(input));
            StringAssert.Contains("Voorraad mag niet negatief zijn", ex!.Message);
        }

        [Test]
        public void TC19_05_Add_DuplicateName_ShouldThrow()
        {
            // Arrange
            _ = _svc.Add(new Product(0, "Cola 33cl", stock: 5, shelfLife: default, price: 0.99m));
            // Act + Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                _svc.Add(new Product(0, "Cola 33cl", stock: 2, shelfLife: default, price: 1.05m)));
            StringAssert.Contains("Er bestaat al een product met deze naam", ex!.Message);
        }
    }
}
