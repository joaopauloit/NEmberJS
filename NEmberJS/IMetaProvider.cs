namespace NEmberJS
{
    public interface IMetaProvider
    {
        bool Wants(object content);

        object GetMeta(object instance);
    }
}
