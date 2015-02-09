namespace NEmberJS.MediaTypeFormatters
{
    internal class EnvelopeWrite : IEnvelope
    {
        public EnvelopeWrite(object value)
        {
            Value = value;
        }

        public object Value { get; set; }
    }
}