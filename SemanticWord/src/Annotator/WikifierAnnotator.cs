// lyjisejmjpgiaexgjzjwgoqggxjajh
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Net;

namespace SemanticWord
{
    namespace Annotation
    {
        /// <summary>
        ///     An annotator that uses the Wikifier service
        ///     to annotate text.
        /// </summary>
        public class WikifierAnnotator : IAnnotator
        {
            private readonly HttpClient _client = new HttpClient();
            private const string Token = "lyjisejmjpgiaexgjzjwgoqggxjajh";
            private const string URI =
                "http://www.wikifier.org"
                + "/annotate-article";

            private readonly WikifierParameters _parameters;

            public WikifierAnnotator(WikifierParameters parameters = null)
            {
                if (parameters == null)
                {
                    _parameters = new WikifierParameters();
                }
            }

            public async Task<IList<Metatag>> AnnotateAsync(string text)
            {
                var response = await PostTextAsync(text);
                return response.Annotations
                    .SelectMany(a => Convert(a, text))
                    .ToList();
            }

            private IEnumerable<Metatag> Convert(
                WikifierAnnotation a,
                string text)
            {
                return a.Support.Select(
                    s =>
                    {
                        int contextStart =
                            Math.Clamp(
                                s.ChFrom - 10,
                                0, text.Length
                            );
                        int contextLength =
                            Math.Clamp(
                                s.ChTo - contextStart + 10,
                                0, text.Length - contextStart
                            );
                        return new Metatag(
                            text.Substring(s.ChFrom, s.ChTo - s.ChFrom),
                            text.Substring(contextStart, contextLength),
                            a.Title,
                            a.DBPediaTypes.Count > 0 ? a.DBPediaTypes[0] : "",
                            a.DBPediaIRI
                        );
                    }
                );
            }

            private async Task<WikifierResponse> PostTextAsync(string text)
            {
                var textEncoded = WebUtility.UrlEncode(text);
                var parametersEncoded = EncodeParameters();
                var body = Encoding.UTF8.GetBytes(
                    $"userKey={Token}&text={textEncoded}&{parametersEncoded}"
                );
                var content = new ByteArrayContent(body);
                content.Headers.ContentType =
                    new MediaTypeHeaderValue(
                        "application/x-www-form-urlencoded"
                    );

                var response = await _client.PostAsync(URI, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                Console.WriteLine(responseBody);

                var annotations = JsonSerializer
                    .Deserialize<WikifierResponse>(
                        responseBody,
                        options
                    );

                return annotations;
            }

            private string EncodeParameters()
            {
                var parameters = new (string, string)[] {
                    ("lang", _parameters.Language.ToString()),
                    (
                        "pageRankSqThreshold",
                        _parameters.PageRankSQThreshold.ToString()
                    ),
                    (
                        "applyPageRankSqThreshold",
                        _parameters.ApplyPageRankSqThreshold.ToString()
                    ),
                    ("includeCosines", _parameters.IncludeCosines.ToString()),
                    ("ranges", _parameters.Ranges.ToString()),
                    ("support", _parameters.Support.ToString()),
                    (
                        "nTopDfValuesToIgnore",
                        _parameters.NTopDfValuesToIgnore.ToString()
                    ),
                    (
                        "minLinkFrequency",
                        _parameters.MinLinkFrequency.ToString()
                    )
                };

                var parameterPairs = parameters.Select(
                    p =>
                        $"{p.Item1}="
                        + $"{WebUtility.UrlEncode(p.Item2.ToLowerInvariant())}"
                );

                return String.Join('&', parameterPairs);
            }
        }

        public class WikifierParameters
        {
            public string Language { get; set; } = "auto";
            public double PageRankSQThreshold { get; set; } = -1.0;
            public bool ApplyPageRankSqThreshold { get; set; } = false;
            public bool IncludeCosines { get; set; } = false;
            public bool Ranges { get; set; } = false;
            public bool Support { get; set; } = true;
            public int NTopDfValuesToIgnore { get; set; } = 200;
            public int MinLinkFrequency { get; set; } = 5;
        }

        class WikifierResponse
        {
            public IList<WikifierAnnotation> Annotations { get; set; }
        }

        class WikifierAnnotation
        {
            public string Title { get; set; }
            public string URL { get; set; }
            public IList<string> DBPediaTypes { get; set; }
            public string DBPediaIRI { get; set; }
            public IList<WikifierSupport> Support { get; set; }
        }

        class WikifierSupport
        {
            public int ChFrom { get; set; }
            public int ChTo { get; set; }
        }
    }
}
