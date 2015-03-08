using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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


            var sideLoadsProperties = GetSideloadProperties(content);

            var hasSideLoad = sideLoadsProperties.Any();
            if (hasSideLoad)
            {
                var withoutSideloadDictionary = new Dictionary<object, object>();
                var propertiesWithoutSideLoad =
                    content.GetType()
                        .GetProperties()
                        .Where(x => x.GetCustomAttributes().All(z => z.GetType() != typeof(SideloadAttribute))).ToList();
                propertiesWithoutSideLoad.ForEach(prop => withoutSideloadDictionary.Add(prop.Name, prop.GetValue(content)));
                dict.Add(envelopePropertyName, withoutSideloadDictionary);
                foreach (var property in sideLoadsProperties)
                {
                    
                    var sideLoadProperty = property.GetMethod.Invoke(content, null) ;

                    var enumerable = sideLoadProperty as IEnumerable;
                    if (enumerable != null)
                    {
                        List<object> idItems =
                            (from object item in enumerable
                                select item.GetType().GetProperty("Id").GetValue(item)).ToList();
                        withoutSideloadDictionary.Add(property.Name, idItems);
                    }
                    else
                    {
                       object idItem =
                            sideLoadProperty.GetType().GetProperty("Id").GetValue(sideLoadProperty);
                        withoutSideloadDictionary.Add(property.Name, idItem);
                    }

                    dict.Add(property.Name, sideLoadProperty);
                }
                
            }
            else
            {
                dict.Add(envelopePropertyName, content);
            }




            var metaProvider = _metaProviders.FirstOrDefault(m => m.Wants(content));

            if (metaProvider != null)
            {
                dict.Add("Meta", metaProvider.GetMeta(content));
            }

            return dict;
        }

        private static List<PropertyInfo> GetSideloadProperties(object content)
        {
            return content.GetType()
                .GetProperties()
                .Where(x => x.GetCustomAttributes().Any(z => z.GetType() == typeof(SideloadAttribute))).ToList();
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