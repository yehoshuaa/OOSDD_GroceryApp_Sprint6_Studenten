using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public List<Product> GetAll()
        {
            return _productRepository.GetAll();
        }

        public Product Add(Product item)
        {
            // ===== UC19: nieuwe producten aanmaken =====

            // 1) Controleer of naam ingevuld is
            if (string.IsNullOrWhiteSpace(item.Name))
                throw new ArgumentException("Naam is verplicht.");

            // 2) Controleer op geldige prijs (> 0)
            if (item.Price <= 0)
                throw new ArgumentException("Prijs moet groter zijn dan 0.");

            // 3) Controleer of voorraad niet negatief is
            if (item.Stock < 0)
                throw new ArgumentException("Voorraad mag niet negatief zijn.");

            // 4) Controleer of productnaam al bestaat (optioneel)
            var allProducts = _productRepository.GetAll();
            if (allProducts.Any(p => p.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Er bestaat al een product met deze naam.");

            // 5) Voeg toe aan database via repository
            return _productRepository.Add(item);
        }

        public Product? Delete(Product item)
        {
            throw new NotImplementedException();
        }

        public Product? Get(int id)
        {
            throw new NotImplementedException();
        }

        public Product? Update(Product item)
        {
            return _productRepository.Update(item);
        }
    }
}
