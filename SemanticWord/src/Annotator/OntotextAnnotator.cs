using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System;

namespace SemanticWord
{
    namespace Annotation
    {
        /** <summary>
                An example annotator that uses Ontotext Tag
                service to annotate text.
            </summary>
        */
        public class OntotextAnnotator : IAnnotator
        {
            private readonly HttpClient _client = new HttpClient();
            private const string _uri = "http://tag.ontotext.com/ces-en/extract";
            private const string _token =
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJhdXRob3JpdGllcyI6WyJS"
                + "T0xFX1VTRVIiXSwidXNlcm5hbWUiOiJ1c2VyIn0.JMV-kAdLd9RhGcxeCgq"
                + "Cc0O5xG9-oQUUwI4vC83BpwU";

            public OntotextAnnotator()
            {
                var hs = _client.DefaultRequestHeaders;
                hs.Add("X-JwtToken", _token);
                hs.Add("Accept", "application/vnd.ontotext.ces+json");
            }

            public IList<Metatag> Annotations { get; private set; } =
                new List<Metatag>();

            public async Task AnnotateAsync(string text)
            {
                var mentions = await PostTextAsync(text);

                Annotations = mentions.Mentions
                    .Select(
                        m =>
                        {
                            int contextStart =
                                Math.Clamp(
                                    m.StartOffset - 10,
                                    0, text.Length
                                );
                            int contextEnd = m.EndOffset + 10 - contextStart; 
                            return new Metatag(
                                m.Name,
                                text.Substring(contextStart, contextEnd),
                                m.Type,
                                m.Features.Class,
                                m.Features.Inst
                            );
                        }
                    ).ToList();
            }

            private async Task<OntotextTagResponse> PostTextAsync(string text)
            {
                var body = Encoding.UTF8.GetBytes(text);
                var content = new ByteArrayContent(body);
                content.Headers.ContentType =
                    new MediaTypeHeaderValue("text/plain");

                var response = await _client.PostAsync(_uri, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var mentions = JsonSerializer
                    .Deserialize<OntotextTagResponse>(
                        responseBody,
                        options
                    );

                return mentions;
            }

        }

        class OntotextTagResponse
        {
            public IList<Mention> Mentions { get; set; }
        }

        class Mention
        {
            public string Name { get; set; }
            public int StartOffset { get; set; }
            public int EndOffset { get; set; }
            public string Type { get; set; }

            public Features Features { get; set; }
        }

        class Features
        {
            public string Class { get; set; }
            public string Inst { get; set; }
        }
    }
}