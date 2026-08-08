using Android.App;
using Android.Content;
using Microsoft.Maui.ApplicationModel;
using wildflowerFrontend.Services;
using AndroidApplication = global::Android.App.Application;
using AndroidUri = global::Android.Net.Uri;

namespace wildflowerFrontend.Platforms.Android;

public sealed class AndroidFolderPickerService : IFolderPickerService
{
    private const int OpenDocumentTreeRequestCode = 64217;
    private readonly object syncRoot = new();
    private TaskCompletionSource<string?>? pendingRequest;
    private CancellationTokenRegistration cancellationRegistration;

    public AndroidFolderPickerService()
    {
        MainActivity.ActivityResultReceived += MainActivity_ActivityResultReceived;
    }

    public Task<string?> PickFolderAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Activity activity = Platform.CurrentActivity
            ?? throw new InvalidOperationException("The Android activity is not available.");

        TaskCompletionSource<string?> request;
        lock (syncRoot)
        {
            if (pendingRequest is not null)
                throw new InvalidOperationException("A folder selection is already in progress.");

            request = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
            pendingRequest = request;
        }

        CancellationTokenRegistration registration = cancellationToken.Register(
            () => CancelPendingRequest(cancellationToken));
        lock (syncRoot)
        {
            if (ReferenceEquals(pendingRequest, request))
                cancellationRegistration = registration;
            else
                registration.Dispose();
        }

        var intent = new Intent(Intent.ActionOpenDocumentTree);
        intent.AddFlags(
            ActivityFlags.GrantReadUriPermission |
            ActivityFlags.GrantPersistableUriPermission |
            ActivityFlags.GrantPrefixUriPermission);

        try
        {
            activity.StartActivityForResult(intent, OpenDocumentTreeRequestCode);
        }
        catch
        {
            ClearPendingRequest();
            throw;
        }

        return request.Task;
    }

    private void MainActivity_ActivityResultReceived(object? sender, AndroidActivityResultEventArgs e)
    {
        if (e.RequestCode != OpenDocumentTreeRequestCode)
            return;

        if (e.ResultCode != Result.Ok || e.Data?.Data is null)
        {
            CompletePendingRequest(null);
            return;
        }

        try
        {
            AndroidUri treeUri = e.Data.Data;
            ActivityFlags grantedFlags = e.Data.Flags & ActivityFlags.GrantReadUriPermission;
            if (grantedFlags == 0)
                grantedFlags = ActivityFlags.GrantReadUriPermission;

            ContentResolver resolver = Platform.CurrentActivity?.ContentResolver
                ?? AndroidApplication.Context.ContentResolver
                ?? throw new InvalidOperationException("The Android content resolver is not available.");

            resolver.TakePersistableUriPermission(treeUri, grantedFlags);
            CompletePendingRequest(treeUri.ToString());
        }
        catch (Exception ex)
        {
            FailPendingRequest(new InvalidOperationException(
                "Wildflower could not retain access to the selected Android folder. Select it again and allow read access.",
                ex));
        }
    }

    private void CompletePendingRequest(string? result)
    {
        TaskCompletionSource<string?>? request = ClearPendingRequest();
        request?.TrySetResult(result);
    }

    private void FailPendingRequest(Exception exception)
    {
        TaskCompletionSource<string?>? request = ClearPendingRequest();
        request?.TrySetException(exception);
    }

    private void CancelPendingRequest(CancellationToken cancellationToken)
    {
        TaskCompletionSource<string?>? request = ClearPendingRequest();
        request?.TrySetCanceled(cancellationToken);
    }

    private TaskCompletionSource<string?>? ClearPendingRequest()
    {
        lock (syncRoot)
        {
            TaskCompletionSource<string?>? request = pendingRequest;
            pendingRequest = null;
            cancellationRegistration.Dispose();
            cancellationRegistration = default;
            return request;
        }
    }
}
