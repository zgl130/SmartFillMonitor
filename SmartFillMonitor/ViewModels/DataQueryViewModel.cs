using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFillMonitor.Models;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFillMonitor.ViewModels
{
    public partial class DataQueryViewModel : ObservableObject
    {
        public DataQueryViewModel()
        {

        }

        private ObservableCollection<ProductionRecord> records = new();

        [ObservableProperty]
        private ProductionRecord? selectedRecord;

        [ObservableProperty]
        private DateTime? startDate = DateTime.Today.AddDays(-7);

        [ObservableProperty]
        private DateTime? endDate = DateTime.Today;

        [RelayCommand]
        private async Task QueryAsync()
        {

        }

        [RelayCommand]
        private async Task ExportAsync()
        {

        }
    }
}
