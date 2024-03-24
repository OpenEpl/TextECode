using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static OpenEpl.TextECode.Model.DependencyModel;

namespace OpenEpl.TextECode.Model
{
    public class OrderModel
    {
        public List<OrderItem> Items { get; init; } = new List<OrderItem>();

        public void Clear()
        {
            Items.Clear();
        }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "Kind")]
    [JsonDerivedType(typeof(Folder), "Folder")]
    [JsonDerivedType(typeof(Elem), "Elem")]
    public class OrderItem
    {
        public class Folder : OrderItem
        {
            public string Name { get; init; }
            public List<OrderItem> Items { get; init; } = new List<OrderItem>();
        }
        public class Elem : OrderItem
        {
            public string Name { get; init; }
        }
    }
}
