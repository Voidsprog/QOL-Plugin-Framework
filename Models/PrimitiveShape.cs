using UnityEngine;

namespace QOLFramework.Models
{
    /// <summary>
    /// Define uma primitiva individual (esfera, cubo, cilindro, etc.) com posição, escala, rotação e cor.
    /// Usado como bloco de construção para modelos compostos por PrimitiveObjectToy.
    /// </summary>
    public class PrimitiveShape
    {
        public PrimitiveType Type { get; set; } = PrimitiveType.Sphere;
        public Vector3 PositionOffset { get; set; } = Vector3.zero;
        public Vector3 RotationEuler { get; set; } = Vector3.zero;
        public Vector3 Scale { get; set; } = Vector3.one;
        public Color Color { get; set; } = Color.white;
        public bool HasCollider { get; set; } = false;

        public PrimitiveShape() { }

        public PrimitiveShape(PrimitiveType type, Vector3 offset, Vector3 scale, Color color)
        {
            Type = type;
            PositionOffset = offset;
            Scale = scale;
            Color = color;
        }

        public PrimitiveShape(PrimitiveType type, Vector3 offset, Vector3 rotation, Vector3 scale, Color color)
        {
            Type = type;
            PositionOffset = offset;
            RotationEuler = rotation;
            Scale = scale;
            Color = color;
        }
    }
}
