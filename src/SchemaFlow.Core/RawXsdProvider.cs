using System.Text;
using System.Xml;
using Microsoft.Extensions.Caching.Memory;
using SchemaFlow.Model;
using SchemaFlow.Model.GlobalDefinitions;
using SchemaFlow.Model.Types;

namespace SchemaFlow.Core;

/// <summary>
/// Provides utilities to retrieve raw XSD text (full document or element/type fragment)
/// using stored SourceLocation (DocumentUri, Line, Column).
/// </summary>
public static class RawXsdProvider
{
    // Bounded memory cache with size limit and expiration
    private static readonly MemoryCache Cache = new(new MemoryCacheOptions
    {
        // Size is in arbitrary units; we use UTF-16 chars count (~2 bytes/char) as size
        SizeLimit = 128 * 1024 * 1024 // 128 MB budget
    });

    private const int MaxDocumentBytesToCache = 4 * 1024 * 1024; // 4 MB upper limit per entry

    /// <summary>
    /// Attempts to load the full raw text of the XSD document given by a DocumentUri.
    /// Supports file URIs/paths and falls back to XmlUrlResolver for other URIs.
    /// Uses bounded MemoryCache with eviction.
    /// </summary>
    public static bool TryGetDocumentText(string? documentUri, out string text)
    {
        text = string.Empty;
        if (string.IsNullOrWhiteSpace(documentUri))
        {
            return false;
        }

        if (Cache.TryGetValue(documentUri!, out string? cached) && cached is not null)
        {
            text = cached;
            return true;
        }

        try
        {
            string loaded;
            if (Uri.TryCreate(documentUri, UriKind.Absolute, out var uri))
            {
                if (uri.IsFile)
                {
                    loaded = File.ReadAllText(uri.LocalPath);
                }
                else
                {
                    var resolver = new XmlUrlResolver();
                    using var stream = resolver.GetEntity(uri, null, typeof(Stream)) as Stream;
                    if (stream is null)
                    {
                        return false;
                    }
                    using var sr = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
                    loaded = sr.ReadToEnd();
                }
            }
            else
            {
                // Treat as local file path
                var path = Path.GetFullPath(documentUri);
                loaded = File.ReadAllText(path);
            }

            // Optionally cache only if not too large
            var approxBytes = checked(loaded.Length * sizeof(char));
            text = loaded;

            if (approxBytes <= MaxDocumentBytesToCache)
            {
                var entryOptions = new MemoryCacheEntryOptions
                {
                    // Sliding expiration keeps hot documents around
                    SlidingExpiration = TimeSpan.FromMinutes(10),
                    // Absolute cap to avoid stale entries
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                    // Size in bytes
                    Size = approxBytes
                };
                Cache.Set(documentUri!, loaded, entryOptions);
            }

            return true;
        }
        catch
        {
            text = string.Empty;
            return false;
        }
    }

    /// <summary>
    /// Attempts to retrieve the raw XML fragment (outer XML) at the given source location
    /// by re-reading the source document and extracting the subtree starting at Line/Column.
    /// This streams the document and does not require full-document caching.
    /// </summary>
    public static bool TryGetRawFragment(SourceLocation? source, out string xml)
    {
        xml = string.Empty;
        if (source is null || string.IsNullOrWhiteSpace(source.DocumentUri))
        {
            return false;
        }

        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreComments = false,
                IgnoreProcessingInstructions = false,
                IgnoreWhitespace = false
            };

            using var reader = CreateReader(source.DocumentUri!, settings);
            if (reader is not IXmlLineInfo li || !li.HasLineInfo())
            {
                return false; // no line info available
            }

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && li.LineNumber == source.Line && li.LinePosition == source.Column)
                {
                    using var subtree = reader.ReadSubtree();
                    var sb = new StringBuilder();
                    var ws = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true, NewLineHandling = NewLineHandling.Replace };
                    using var xw = XmlWriter.Create(sb, ws);
                    subtree.MoveToContent();
                    xw.WriteNode(subtree, true);
                    xw.Flush();
                    xml = sb.ToString();
                    return true;
                }
            }
        }
        catch
        {
            // ignore
        }

        return false;
    }

    /// <summary>
    /// Convenience: Gets raw XML for a global element declaration.
    /// </summary>
    public static bool TryGetRawFragment(ElementDecl decl, out string xml) => TryGetRawFragment(decl?.Source, out xml);

    /// <summary>
    /// Convenience: Gets raw XML for a type definition (simple/complex).
    /// </summary>
    public static bool TryGetRawFragment(TypeDefinition type, out string xml) => TryGetRawFragment(type?.Source, out xml);

    /// <summary>
    /// Clears all cached documents (bounded memory cache).
    /// </summary>
    public static void ClearCache() => Cache.Compact(1.0);

    /// <summary>
    /// Removes a single cached document by its key (DocumentUri or path).
    /// </summary>
    public static void RemoveFromCache(string documentUri) => Cache.Remove(documentUri);

    private static XmlReader? CreateReader(string documentUri, XmlReaderSettings settings)
    {
        if (Uri.TryCreate(documentUri, UriKind.Absolute, out var uri))
        {
            if (uri.IsFile)
            {
                return XmlReader.Create(uri.LocalPath, settings);
            }
            var resolver = new XmlUrlResolver();
            var stream = resolver.GetEntity(uri, null, typeof(Stream)) as Stream;
            return stream is null ? null : XmlReader.Create(stream, settings, documentUri);
        }
        else
        {
            var path = Path.GetFullPath(documentUri);
            return XmlReader.Create(path, settings);
        }
    }
}
