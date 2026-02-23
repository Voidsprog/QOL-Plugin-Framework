using System.Collections.Generic;

namespace QOLFramework.Models
{
    /// <summary>
    /// Modelo 3D composto por múltiplas primitivas do Unity (PrimitiveObjectToy).
    /// Cada forma é um PrimitiveShape com offset relativo ao centro do modelo.
    /// Visível a TODOS os clientes sem mods — usa o sistema AdminToys nativo do SCP:SL.
    /// </summary>
    public class PrimitiveModel
    {
        public string Name { get; set; }
        public List<PrimitiveShape> Shapes { get; set; } = new List<PrimitiveShape>();

        public PrimitiveModel(string name)
        {
            Name = name;
        }

        public PrimitiveModel(string name, List<PrimitiveShape> shapes)
        {
            Name = name;
            Shapes = shapes;
        }

        public void AddShape(PrimitiveShape shape)
        {
            Shapes.Add(shape);
        }
    }
}
