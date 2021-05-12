using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using static Windows.Storage.AccessCache.StorageApplicationPermissions;

namespace PSC.UWP.Common.Files
{
    /// <summary>
    /// To deal with the process of ask user permission to read and write in folders
    /// outside the App install, data locations and few others, and to remember it for the App lifecycle:
    /// https://docs.microsoft.com/en-us/windows/uwp/files/file-access-permissions
    /// </summary>
    public static class DealAccessFiles
    {
        /// <summary>
        /// As a suggestion because Guid.NewGuid() is unique ever; it can be another Token
        /// </summary>
        /// <param></param>
        /// <returns> string </returns>
        public static string TokenForFutureAccessList { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// As a suggestion; it can be another place, but this is owned by the App
        /// </summary>
        /// <param></param>
        /// <returns> StorageFolder </returns>
        public static StorageFolder LocalForStoreToken { get; } = ApplicationData.Current.LocalFolder;

        /// <summary>To remember a file or folder; it will use FutureAccessList.AddOrReplace(token, fileOrFolder):</summary>
        private static void RememberFileOrFolder(StorageFolder fileOrFolder, string token)
        {
            FutureAccessList.AddOrReplace(token, fileOrFolder);
        }

        /// <summary>
        /// To retrieve the file the next time
        /// </summary>
        /// <param name="token"></param>
        /// <returns>StorageFile</returns>
        public static async Task<StorageFile> GetTokenForFile(string token)
        {
            if (FutureAccessList != null && FutureAccessList.ContainsItem(token))
                return await FutureAccessList.GetFileAsync(token);
            return null;
        }

        /// <summary>
        /// To retrieve the folder the next time
        /// </summary>
        /// <param name="token"></param>
        /// <returns>StorageFolder</returns>
        public static async Task<StorageFolder> GetTokenForFolder(string token)
        {
            if (FutureAccessList != null && FutureAccessList.ContainsItem(token))
                return await FutureAccessList.GetFolderAsync(token);
            return null;
        }

        //<summary>To forget a token, you can use this:</summary>
        public static async void ForgetTokenForFile(string token)
        {
            if (FutureAccessList != null && FutureAccessList.ContainsItem(token)) await Task.Run(() => FutureAccessList.Remove(token));
        }

        public static async Task<string> ReadOrCreateTokenFile(string tokenFileName, StorageFolder tokenFolder, string token = "")
        {
            var storageFileNameToken = await tokenFolder.TryGetItemAsync(tokenFileName) as StorageFile;
            if (storageFileNameToken == null)
            {
                storageFileNameToken =
                    await tokenFolder.CreateFileAsync(tokenFileName, CreationCollisionOption.ReplaceExisting);
                var customersFolderNameToken = string.IsNullOrWhiteSpace(token) ? TokenForFutureAccessList : token;
                await FileIO.WriteTextAsync(storageFileNameToken, customersFolderNameToken);
                return customersFolderNameToken;
            }
            else
            {
                return await FileIO.ReadTextAsync(storageFileNameToken);
            }
        }

        public static async Task<StorageFolder> GetOrPickFolder(
                    string folderNameToken
                    , PickerLocationId suggestedStartLocation = PickerLocationId.DocumentsLibrary
                    , string fileTypeFilter = ".json")
        {
            var localFolderToReturn = await GetTokenForFolder(folderNameToken);
            if (localFolderToReturn != null) return localFolderToReturn;

            var picker = new FolderPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = suggestedStartLocation
            };
            picker.FileTypeFilter.Add(fileTypeFilter);
            localFolderToReturn = await picker.PickSingleFolderAsync();
            RememberFileOrFolder(localFolderToReturn, folderNameToken);
            return localFolderToReturn;
        }
    }
}