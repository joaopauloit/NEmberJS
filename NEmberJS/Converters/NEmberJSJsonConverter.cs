using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NEmberJS.MediaTypeFormatters;

namespace NEmberJS.Converters
{
    public class NEmberJSJsonConverter : JsonConverter
    {
        private readonly IPluralizer _pluralizer;

        private readonly ConcurrentDictionary<Type, string> _envelopePropertyNameCache =
            new ConcurrentDictionary<Type, string>();

        private readonly List<IMetaProvider> _metaProviders = new List<IMetaProvider>();

        public NEmberJSJsonConverter(IPluralizer pluralizer)
        {
            _pluralizer = pluralizer;
        }

        public void AddMetaProvider(
            IMetaProvider provider)
        {
            _metaProviders.Add(provider);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IEnvelope).IsAssignableFrom(objectType);
        }

        public override object ReadJson(
            JsonReader reader,
            Type envelope,
            object existingValue,
            JsonSerializer serializer)
        {
            var innerEnvelopeType = envelope.GetGenericArguments()[0];

            var envelopePropertyName = _envelopePropertyNameCache.GetOrAdd(
                innerEnvelopeType,
                GetEnvelopePropertyName);

           
            var camelCasePropertyName = CamelCase(envelopePropertyName);

            var json = JObject.Load(reader);
            var inner = json[camelCasePropertyName];

            if (inner == null)
            {
                return null;
            }

            using (var innerReader = inner.CreateReader())
            {
                return serializer.Deserialize(innerReader, innerEnvelopeType);
            }
        }

        private string CamelCase(string name)
        {
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var envelope = (EnvelopeWrite)value;

            var inner = FormatEnvelope(envelope.Value);
            serializer.Serialize(writer, inner);
        }

        public object FormatEnvelope(object content)
        {
            var envelopePropertyName = _envelopePropertyNameCache.GetOrAdd(
                content.GetType(),
                GetEnvelopePropertyName);
            var dict = new Dictionary<object, object>();
            

            var sideLoadsProperties = content.GetType()
                .GetProperties()
                .Where(x => x.GetCustomAttributes().Any(z => z.GetType() == typeof(SideloadAttribute))).ToList();

            var hasSideLoad = sideLoadsProperties.Any();
            if (hasSideLoad)
            {
                var returnDict = new Dictionary<object, object>();
                var withoutSideloadDictionary = new Dictionary<object, object>();
                var propertiesWithoutSideLoad =
                    content.GetType()
                        .GetProperties()
                        .Where(x => x.GetCustomAttributes().All(z => z.GetType() != typeof (SideloadAttribute))).ToList();
                propertiesWithoutSideLoad.ForEach(prop => withoutSideloadDictionary.Add(prop.Name, prop.GetValue(content)));
                dict.Add(envelopePropertyName, withoutSideloadDictionary);
                foreach (var property in sideLoadsProperties)
                {

                   var items  = (IEnumerable)property.GetMethod.Invoke(content, null);
                    List<object> idItems = new List<object>();
                    foreach (var item in items)
                    {
                        var idValueItem = item.GetType().GetProperty("Id").GetValue(item);
                        idItems.Add(idValueItem);
                    }
                    withoutSideloadDictionary.Add(property.Name, idItems);
                    dict.Add(property.Name, items);
                 }
                
                
                
              
            }

           
            

            var metaProvider = _metaProviders.FirstOrDefault(m => m.Wants(content));

            if (metaProvider != null)
            {
                dict.Add("Meta", metaProvider.GetMeta(content));
            }

            return dict;
        }

        public string GetEnvelopePropertyName(Type type)
        {
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                return _pluralizer.Pluralize(GetEnvelopeTypeName(elementType));
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (!type.GetGenericArguments().Any())
                {
                    return _pluralizer.Pluralize(type.Name);
                }

                var arg = type.GetGenericArguments()[0];
                return _pluralizer.Pluralize(GetEnvelopeTypeName(arg));
            }

            return GetEnvelopeTypeName(type);
        }

        private string GetEnvelopeTypeName(Type type)
        {
            var attr = type.GetCustomAttributes(typeof(EnvelopeAttribute), true)
                .Cast<EnvelopeAttribute>()
                .FirstOrDefault();

            return attr != null
                ? attr.Name
                : type.Name;
        }


    }
}