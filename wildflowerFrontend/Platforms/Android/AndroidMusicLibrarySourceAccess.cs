using Android.Content;
using Android.Provider;
using wildflower.Services.Library;
using AndroidApplication = global::Android.App.Application;
using AndroidCursor = global::Android.Database.ICursor;
using AndroidUri = global::Android.Net.Uri;

namespace wildflowerFrontend.Platforms.Android;

public sealed class AndroidMusicLibrarySourceAccess : IMusicLibrarySourceAccess
{
    private readonly ContentResolver resolver = AndroidApplication.Context.ContentResolver
        ?? throw new InvalidOperationException("The Android content resolver is not available.");

    public bool IsLibraryAvailable(string sourceIdentifier)
    {
        if (!TryParseContentUri(sourceIdentifier, out AndroidUri? treeUri) || treeUri is null ||
            !HasPersistedReadPermission(treeUri))
        {
            return false;
        }

        try
        {
            string documentId = DocumentsContract.GetTreeDocumentId(treeUri)
                ?? throw new InvalidOperationException("The Android tree document id is unavailable.");
            AndroidUri documentUri = DocumentsContract.BuildDocumentUriUsingTree(treeUri, documentId)
                ?? throw new InvalidOperationException("The Android tree document URI is unavailable.");
            return CanQuery(documentUri);
        }
        catch
        {
            return false;
        }
    }

    public bool IsTrackAvailable(string trackIdentifier)
    {
        return TryParseContentUri(trackIdentifier, out AndroidUri? documentUri) &&
            documentUri is not null &&
            CanQuery(documentUri);
    }

    private bool HasPersistedReadPermission(AndroidUri treeUri)
    {
        string expected = treeUri.ToString() ?? string.Empty;
        return resolver.PersistedUriPermissions.Any(permission =>
            permission.IsReadPermission &&
            string.Equals(permission.Uri?.ToString(), expected, StringComparison.Ordinal));
    }

    private bool CanQuery(AndroidUri documentUri)
    {
        try
        {
            using AndroidCursor? cursor = resolver.Query(
                documentUri,
                new[] { DocumentsContract.Document.ColumnDocumentId },
                null,
                null,
                null);
            return cursor is not null && cursor.MoveToFirst();
        }
        catch
        {
            return false;
        }
    }

    private static bool TryParseContentUri(string identifier, out AndroidUri? uri)
    {
        uri = null;
        if (string.IsNullOrWhiteSpace(identifier))
            return false;

        uri = AndroidUri.Parse(identifier);
        return string.Equals(uri?.Scheme, ContentResolver.SchemeContent, StringComparison.OrdinalIgnoreCase);
    }
}
