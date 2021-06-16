#nullable enable

using System;
using System.IO;
using DocumentFormat.OpenXml.Packaging;

namespace Kumo
{

    class UriStore
    {
        private const string ID_PART = "kumo-document-id";
        private const string ROOT = "document";

        private MainDocumentPart _mainPart;

        private CustomXmlPart? _container;

        private Uri? _uri;

        public Uri Value
        {
            get
            {
                if (_container is null)
                {
                    Load();
                    if (_container is null)
                    {
                        Initialize();

                        var guid = Guid.NewGuid().ToString();
                        var time = DateTime.Now.ToString("yyyyMMddTHHMMss");

                        var b = new UriBuilder(Schema.Namespace);
                        b.Path = $"{ROOT}/{guid}-{time}";
                        Update(b.Uri);
                    }
                }
                
                return _uri!;
            }
        }

        public UriStore(MainDocumentPart mainPart)
        {
            _mainPart = mainPart;
        }

        private void Load()
        {
            _mainPart.TryGetPartById(ID_PART, out var part);

            _container = part switch
            {
                CustomXmlPart p => p,
                null => null,
                _ => throw new ArgumentException(
                    $"a part with ID \"{ID_PART}\" is not a custom XML part"
                )
            };

            if (_container is not null)
            {
                using (var sr = new StreamReader(_container.GetStream(
                    FileMode.Open, FileAccess.Read)))
                {
                    var rawUri = sr.ReadToEnd().Trim();
                    _uri = new Uri(rawUri);
                }
            }
        }

        private void Update(Uri value)
        {
            using (var sw = new StreamWriter(_container!.GetStream(
                FileMode.Create, FileAccess.Write)))
            {
                _uri = value;
                sw.WriteLine(value);
            }
        }

        private void Initialize()
        {
            _container = _mainPart.AddCustomXmlPart(
                "text/plain",
                ID_PART
            );
        }
    }
}
