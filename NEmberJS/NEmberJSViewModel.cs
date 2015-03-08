using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NEmberJS
{
    public interface INEmberJSViewModel
    {
        object Id { get; set; }
        List<T> GetSideLoadCollection<T>(Type type);
    }
}