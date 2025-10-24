// Grocery.App/ViewModels/NewProductViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel; // [MVVM]
using CommunityToolkit.Mvvm.Input;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.App.ViewModels
{
    public partial class NewProductViewModel : ObservableObject
    {
        private readonly IProductService _products;

        [ObservableProperty] private string name = "";
        [ObservableProperty] private int stock;
        [ObservableProperty] private DateTime? shelfLifeDate;
        [ObservableProperty] private decimal price;

        public NewProductViewModel(IProductService products) => _products = products;

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                var life = ShelfLifeDate.HasValue ? DateOnly.FromDateTime(ShelfLifeDate.Value) : default;
                _ = _products.Add(new Product(0, Name?.Trim() ?? "", Stock, life, Price));
                await Shell.Current.GoToAsync(".."); // terug naar lijst
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Fout", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task CancelAsync() => await Shell.Current.GoToAsync("..");
    }
}
