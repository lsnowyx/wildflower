using Android.Content;
using Android.Provider;
using wildflower.Services.Library;
using AndroidApplication = global::Android.App.Application;
using AndroidCursor = global::Android.Database.ICursor;
using AndroidUri = global::Android.Net.Uri;

namespace wildflowerFrontend.Platforms.Android;

public sealed class AndroidMusicLibraryScanner : IMusicLibraryScanner
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3",
        ".wav",
        ".flac",
        ".ogg"
    };

    private readonly ContentResolver resolver = AndroidApplication.Context.ContentResolver
        ?? throw new InvalidOperationException("The Android content resolver is not available.");

    public Task<IReadOnlyList<string>> ScanAsync(string folderPath)
    {
        return Task.Run<IReadOnlyList<string>>(() => Scan(folderPath));
    }

    public bool IsSupportedAudioFile(string filePath)
    {
        return SupportedExtensions.Contains(Path.GetExtension(filePath));
    }

    private IReadOnlyList<string> Scan(string treeUriText)
    {
        AndroidUri treeUri = AndroidUri.Parse(treeUriText)
            ?? throw new InvalidOperationException("The selected Android folder URI is invalid.");
        string treeDocumentId = DocumentsContract.GetTreeDocumentId(treeUri)
            ?? throw new InvalidOperationException("The Android tree document id is unavailable.");
        AndroidUri childrenUri = DocumentsContract.BuildChildDocumentsUriUsingTree(treeUri, treeDocumentId)
            ?? throw new InvalidOperationException("The Android child-document URI is unavailable.");

        string[] projection =
        {
            DocumentsContract.Document.ColumnDocumentId,
            DocumentsContract.Document.ColumnDisplayName,
            DocumentsContract.Document.ColumnMimeType
        };

        using AndroidCursor? cursor = resolver.Query(childrenUri, projection, null, null, null);
        if (cursor is null)
            throw new InvalidOperationException("Android did not return the selected folder contents. Access may have been revoked.");

        int idColumn = cursor.GetColumnIndexOrThrow(DocumentsContract.Document.ColumnDocumentId);
        int nameColumn = cursor.GetColumnIndexOrThrow(DocumentsContract.Document.ColumnDisplayName);
        int mimeColumn = cursor.GetColumnIndexOrThrow(DocumentsContract.Document.ColumnMimeType);
        var tracks = new List<(string Name, string Uri)>();

        while (cursor.MoveToNext())
        {
            string? documentId = cursor.GetString(idColumn);
            string? displayName = cursor.GetString(nameColumn);
            string? mimeType = cursor.GetString(mimeColumn);
            if (string.IsNullOrWhiteSpace(documentId) ||
                string.IsNullOrWhiteSpace(displayName) ||
                string.Equals(mimeType, DocumentsContract.Document.MimeTypeDir, StringComparison.Ordinal) ||
                !IsSupportedAudioFile(displayName))
            {
                continue;
            }

            AndroidUri documentUri = DocumentsContract.BuildDocumentUriUsingTree(treeUri, documentId)
                ?? throw new InvalidOperationException("Android could not build a document URI for a selected track.");
            string documentUriText = documentUri.ToString()
                ?? throw new InvalidOperationException("Android returned an empty document URI for a selected track.");
            tracks.Add((displayName, documentUriText));
        }

        return tracks
            .OrderBy(track => track.Name, StringComparer.OrdinalIgnoreCase)
            .Select(track => track.Uri)
            .ToArray();
    }
}
