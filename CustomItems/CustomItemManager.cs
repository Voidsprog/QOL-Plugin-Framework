using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using UnityEngine;
using Mirror;
using PlayerRoles;

namespace QOLFramework.CustomItems
{
    /// <summary>
    /// Spawna triggers invisíveis no mundo. Quando um jogador entra no raio, dispara o callback.
    /// Para modelos visuais, usar QOLFramework.Models.PrimitiveModelSpawner.
    /// </summary>
    public class CustomItemManager
    {
        private readonly Dictionary<int, Action<Player>> _pickupCallbacks = new Dictionary<int, Action<Player>>();
        private readonly HashSet<GameObject> _spawnedObjects = new HashSet<GameObject>();
        private int _nextId = 1;

        /// <summary>
        /// Spawna um trigger invisível no mundo.
        /// Quando um jogador entrar no raio, onPickedUp é chamado e o trigger é destruído.
        /// </summary>
        public int SpawnTrigger(Vector3 position, float triggerRadius, Action<Player> onPickedUp)
        {
            var go = new GameObject("QOL_Trigger_" + _nextId);
            var sphere = go.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = triggerRadius;
            go.transform.position = position;

            var id = _nextId++;
            _pickupCallbacks[id] = onPickedUp;
            _spawnedObjects.Add(go);

            var behaviour = go.AddComponent<CustomItemTriggerBehaviour>();
            behaviour.Init(this, id, triggerRadius);

            if (NetworkServer.active)
                NetworkServer.Spawn(go);

            return id;
        }

        /// <summary>Mantido para compatibilidade — redireciona para SpawnTrigger.</summary>
        public int SpawnCustomItem(Vector3 position, string modelPath, float triggerRadius, Action<Player> onPickedUp)
        {
            return SpawnTrigger(position, triggerRadius, onPickedUp);
        }

        public void NotifyPlayerEntered(int itemId, Player player)
        {
            if (!_pickupCallbacks.TryGetValue(itemId, out var cb)) return;
            _pickupCallbacks.Remove(itemId);
            cb?.Invoke(player);
        }

        public void DestroyItem(GameObject go)
        {
            if (go != null && _spawnedObjects.Remove(go))
            {
                if (NetworkServer.active)
                    NetworkServer.Destroy(go);
                else
                    UnityEngine.Object.Destroy(go);
            }
        }

        public void DestroyAll()
        {
            foreach (var go in _spawnedObjects)
            {
                if (go != null)
                {
                    if (NetworkServer.active)
                        NetworkServer.Destroy(go);
                    else
                        UnityEngine.Object.Destroy(go);
                }
            }
            _spawnedObjects.Clear();
            _pickupCallbacks.Clear();
        }

        public class CustomItemTriggerBehaviour : MonoBehaviour
        {
            private CustomItemManager _manager;
            private int _itemId;
            private float _radiusSq;

            public void Init(CustomItemManager manager, int itemId, float radius)
            {
                _manager = manager;
                _itemId = itemId;
                _radiusSq = radius * radius;
            }

            private void OnTriggerEnter(Collider other)
            {
                if (_manager == null) return;
                var hub = other.GetComponentInParent<ReferenceHub>();
                if (hub == null) return;
                var player = Player.List.FirstOrDefault(p => p.ReferenceHub == hub);
                if (player == null || player.IsDestroyed) return;
                if (player.Team == Team.SCPs) return;
                if ((player.Position - transform.position).sqrMagnitude > _radiusSq) return;

                _manager.NotifyPlayerEntered(_itemId, player);
                _manager.DestroyItem(gameObject);
            }
        }
    }
}
