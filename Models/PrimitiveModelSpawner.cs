using System;
using System.Collections.Generic;
using System.Reflection;
using AdminToys;
using LabExtended.API.Toys;
using UnityEngine;

namespace QOLFramework.Models
{
    /// <summary>
    /// Spawna modelos compostos por PrimitiveToy (LabExtended wrapper).
    /// Visíveis a TODOS os clientes nativamente via AdminToys.
    /// </summary>
    public static class PrimitiveModelSpawner
    {
        private static ConstructorInfo _toyConstructor;
        private static Type _primitiveTypeParam;
        private static bool _reflectionInit;

        private static void InitReflection()
        {
            if (_reflectionInit) return;
            _reflectionInit = true;

            try
            {
                var toyType = typeof(PrimitiveToy);
                foreach (var ctor in toyType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
                {
                    var p = ctor.GetParameters();
                    if (p.Length >= 3 && p.Length <= 4)
                    {
                        _toyConstructor = ctor;
                        _primitiveTypeParam = p[2].ParameterType;
                        LabApi.Features.Console.Logger.Info(
                            $"[QOL:Models] PrimitiveToy ctor encontrado: {p.Length} params, tipo primitiva = {_primitiveTypeParam.FullName}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Warn($"[QOL:Models] Reflexão falhou: {ex.Message}");
            }
        }

        private static object ConvertPrimitiveType(PrimitiveType unityType)
        {
            if (_primitiveTypeParam == null || _primitiveTypeParam == typeof(PrimitiveType))
                return unityType;

            // Converter por nome do enum (Sphere -> Sphere, Cube -> Cube, etc.)
            var name = unityType.ToString();
            try
            {
                return Enum.Parse(_primitiveTypeParam, name);
            }
            catch
            {
                // Converter por valor inteiro
                return Enum.ToObject(_primitiveTypeParam, (int)unityType);
            }
        }

        private static PrimitiveToy CreateToy(Vector3 pos, Quaternion rot, PrimitiveType type, PrimitiveFlags flags)
        {
            InitReflection();

            if (_toyConstructor != null && _primitiveTypeParam != typeof(PrimitiveType))
            {
                var convertedType = ConvertPrimitiveType(type);
                var p = _toyConstructor.GetParameters();
                object[] args;
                if (p.Length == 4)
                    args = new object[] { (Vector3?)pos, (Quaternion?)rot, convertedType, flags };
                else if (p.Length == 3)
                    args = new object[] { (Vector3?)pos, (Quaternion?)rot, convertedType };
                else
                    args = new object[] { (Vector3?)pos, (Quaternion?)rot, convertedType, flags };

                return (PrimitiveToy)_toyConstructor.Invoke(args);
            }

            return new PrimitiveToy(pos, rot, type, flags);
        }

        public static List<PrimitiveToy> SpawnModel(PrimitiveModel model, Vector3 worldPosition, Quaternion worldRotation)
        {
            if (model == null || model.Shapes.Count == 0) return null;

            var spawned = new List<PrimitiveToy>();

            foreach (var shape in model.Shapes)
            {
                try
                {
                    var pos = worldPosition + worldRotation * shape.PositionOffset;
                    var rot = worldRotation * Quaternion.Euler(shape.RotationEuler);

                    var toy = CreateToy(pos, rot, shape.Type,
                        shape.HasCollider
                            ? PrimitiveFlags.Visible | PrimitiveFlags.Collidable
                            : PrimitiveFlags.Visible);

                    toy.Color = shape.Color;
                    toy.Scale = shape.Scale;

                    spawned.Add(toy);
                }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Warn($"[QOL:Models] Erro ao spawnar shape '{shape.Type}': {ex.Message}");
                }
            }

            if (spawned.Count > 0)
                LabApi.Features.Console.Logger.Info($"[QOL:Models] Modelo '{model.Name}' spawnado ({spawned.Count}/{model.Shapes.Count} shapes).");

            return spawned;
        }

        public static List<PrimitiveToy> SpawnModelOnTarget(PrimitiveModel model, Mirror.NetworkBehaviour target, Vector3 localOffset)
        {
            if (model == null || model.Shapes.Count == 0 || target == null) return null;

            var spawned = new List<PrimitiveToy>();

            foreach (var shape in model.Shapes)
            {
                try
                {
                    var worldPos = target.transform.position + localOffset + shape.PositionOffset;

                    var toy = CreateToy(worldPos, Quaternion.Euler(shape.RotationEuler),
                        shape.Type, PrimitiveFlags.Visible);

                    toy.Color = shape.Color;
                    toy.Scale = shape.Scale;
                    toy.IsStatic = false;

                    try { toy.SetParent(target); }
                    catch { }

                    spawned.Add(toy);
                }
                catch (Exception ex)
                {
                    LabApi.Features.Console.Logger.Warn($"[QOL:Models] Erro ao spawnar shape on target: {ex.Message}");
                }
            }

            return spawned;
        }

        public static void DestroyModel(List<PrimitiveToy> toys)
        {
            if (toys == null) return;
            foreach (var toy in toys)
            {
                try
                {
                    if (toy?.Base != null && toy.Base.gameObject != null)
                        Mirror.NetworkServer.Destroy(toy.Base.gameObject);
                }
                catch { }
            }
            toys.Clear();
        }
    }
}
