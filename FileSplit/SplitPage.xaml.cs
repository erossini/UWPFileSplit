using FileSplit.ViewModels;
using PSC.UWP.Common.CustomEventArgs;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FileSplit
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SplitPage : Page
    {
        public SplitPageViewModel vm;

        public SplitPage()
        {
            this.InitializeComponent();

            vm = new SplitPageViewModel(this.Dispatcher);
            DataContext = vm;
        }

        public async void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            openPicker.FileTypeFilter.Add(".xls");
            openPicker.FileTypeFilter.Add(".xlsx");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFileToken", file);

                vm.FileName = file.Path;
            }
        }

        private async void btnBrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                    FutureAccessList.AddOrReplace("PickedFolderToken", folder);

                vm.Folder = folder.Path;
            }
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            await vm.SaveExport();
        }

        private void lvHeaders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.SelectedItemLeft = GetSelection(sender as ListView);
        }

        private void lvSelected_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.SelectedItemRight = GetSelection(sender as ListView);
        }

        private List<ListItemData> GetSelection(ListView listView)
        {
            List<ListItemData> rtn = new List<ListItemData>();

            foreach (ListItemData item in listView.SelectedItems)
                rtn.Add(item);

            return rtn;
        }
    }
}