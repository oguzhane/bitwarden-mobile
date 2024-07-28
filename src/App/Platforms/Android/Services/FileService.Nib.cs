using Android;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Bit.Core.Abstractions;
using Bit.Core.Resources.Localization;

namespace Bit.Droid.Services
{
    // Tag:Nibblewarden
    public partial class FileService: IFileService
    {
        public Task<T> SelectFileAsync<T>() where T : class
        {
            var result = new TaskCompletionSource<T>();

            try
            {
                var activity = (MainActivity)Platform.CurrentActivity;

                var chooserIntent = CreateFileChooserIntent();
                if (chooserIntent == null)
                {
                    result.SetResult(null);
                    return result.Task;
                }

                activity.StartActivityForResult(chooserIntent, result);
            }
            catch (Exception ex)
            {
                result.SetException(ex);
            }


            return result.Task;
        }

        private Intent CreateFileChooserIntent()
        {
            var activity = (MainActivity)Platform.CurrentActivity;
            var hasStorageWritePermission = !_cameraPermissionsDenied &&
                HasPermission(Manifest.Permission.WriteExternalStorage);
            var additionalIntents = new List<IParcelable>();
            if (activity.PackageManager.HasSystemFeature(PackageManager.FeatureCamera))
            {
                var hasCameraPermission = !_cameraPermissionsDenied && HasPermission(Manifest.Permission.Camera);
                if (!_cameraPermissionsDenied && !hasStorageWritePermission)
                {
                    AskPermission(Manifest.Permission.WriteExternalStorage);
                    return null;
                }
                if (!_cameraPermissionsDenied && !hasCameraPermission)
                {
                    AskPermission(Manifest.Permission.Camera);
                    return null;
                }
                if (!_cameraPermissionsDenied && hasCameraPermission && hasStorageWritePermission)
                {
                    try
                    {
                        var tmpDir = new Java.IO.File(activity.FilesDir, Constants.TEMP_CAMERA_IMAGE_DIR);
                        var file = new Java.IO.File(tmpDir, Constants.TEMP_CAMERA_IMAGE_NAME);
                        if (!file.Exists())
                        {
                            file.ParentFile.Mkdirs();
                            file.CreateNewFile();
                        }
                        var outputFileUri = FileProvider.GetUriForFile(activity,
                            "com.x8bit.bitwarden.fileprovider", file);
                        additionalIntents.AddRange(GetCameraIntents(outputFileUri));
                    }
                    catch (Java.IO.IOException) { }
                }
            }

            var docIntent = new Intent(Intent.ActionOpenDocument);
            docIntent.AddCategory(Intent.CategoryOpenable);
            docIntent.SetType("*/*");
            var chooserIntent = Intent.CreateChooser(docIntent, AppResources.FileSource);
            if (additionalIntents.Count > 0)
            {
                chooserIntent.PutExtra(Intent.ExtraInitialIntents, additionalIntents.ToArray());
            }

            return chooserIntent;
        }
    }
}
