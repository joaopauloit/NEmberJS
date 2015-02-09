using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using NEmberJS.MediaTypeFormatters;

namespace NEmberJS
{
    public class NEmberJSMediaTypeFormatter : MediaTypeFormatter
    {
        private readonly JsonMediaTypeFormatter _jsonMediaTypeFormatter;

        private readonly NEmberJSJsonMediaTypeFormatter _nEmberJsJsonMediaTypeFormatter;

        public NEmberJSMediaTypeFormatter()
            : this(new DefaultPluralizer())
        {
        }

        public NEmberJSMediaTypeFormatter(IPluralizer pluralizer)
        {
            _jsonMediaTypeFormatter = new JsonMediaTypeFormatter();
            _nEmberJsJsonMediaTypeFormatter = new NEmberJSJsonMediaTypeFormatter(pluralizer);

            // we should probably take these from the registered inner media type formatters
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
        }

        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override Task<object> ReadFromStreamAsync(Type type,
            Stream readStream,
            HttpContent content,
            IFormatterLogger formatterLogger,
            CancellationToken cancellationToken)
        {
            return _nEmberJsJsonMediaTypeFormatter.ReadFromStreamAsync(type, readStream, content, formatterLogger, cancellationToken);
        }

        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            var pairs = request.GetQueryNameValuePairs();

            if (pairs.Any(p => p.Key == "envelope" && p.Value == "false"))
            {
                return _jsonMediaTypeFormatter;
            }

            return _nEmberJsJsonMediaTypeFormatter;
        }

        public void AddMetaProvider(IMetaProvider provider)
        {
            _nEmberJsJsonMediaTypeFormatter.AddMetaProvider(provider);
        }
    }
}
