using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using MapGeneration;
using UnityEngine;

namespace QOLFramework.Utilities
{
    /// <summary>Utilitários para salas do mapa, zonas e jogadores por área.</summary>
    public static class MapUtilities
    {
        /// <summary>Lista de todas as salas do mapa (vazia se Map.Rooms for null).</summary>
        public static IReadOnlyList<Room> AllRooms => Map.Rooms?.ToList() ?? new List<Room>();

        /// <summary>Obtém salas da zona indicada (ignora entradas null).</summary>
        public static IEnumerable<Room> GetRoomsByZone(FacilityZone zone)
        {
            return AllRooms.Where(r => r != null && r.Zone == zone);
        }

        /// <summary>Salas de Light Containment.</summary>
        public static IEnumerable<Room> GetLczRooms() => GetRoomsByZone(FacilityZone.LightContainment);
        /// <summary>Salas de Heavy Containment.</summary>
        public static IEnumerable<Room> GetHczRooms() => GetRoomsByZone(FacilityZone.HeavyContainment);
        /// <summary>Salas de Entrance.</summary>
        public static IEnumerable<Room> GetEzRooms() => GetRoomsByZone(FacilityZone.Entrance);
        /// <summary>Salas da superfície.</summary>
        public static IEnumerable<Room> GetSurfaceRooms() => GetRoomsByZone(FacilityZone.Surface);

        /// <summary>Nome da sala (string vazia se room for null ou falhar).</summary>
        public static string GetRoomName(Room room)
        {
            try { return room?.Name.ToString() ?? ""; }
            catch { return ""; }
        }

        /// <summary>Procura salas cujo nome contém partialName (opcionalmente na zona). Retorna vazio se partialName for null/vazio.</summary>
        public static IEnumerable<Room> FindRoomsByName(string partialName, FacilityZone? zone = null)
        {
            if (string.IsNullOrEmpty(partialName)) return Enumerable.Empty<Room>();
            var rooms = zone.HasValue ? GetRoomsByZone(zone.Value) : AllRooms;
            var upper = partialName.ToUpperInvariant();
            return rooms.Where(r => r != null && GetRoomName(r).ToUpperInvariant().Contains(upper));
        }

        /// <summary>Escolhe uma sala aleatória (opcionalmente na zona). Null se não houver salas.</summary>
        public static Room GetRandomRoom(FacilityZone? zone = null)
        {
            var rooms = (zone.HasValue ? GetRoomsByZone(zone.Value) : AllRooms).ToList();
            return rooms.Count == 0 ? null : rooms[UnityEngine.Random.Range(0, rooms.Count)];
        }

        /// <summary>Sala mais próxima da posição (ignora salas null). Null se não houver salas.</summary>
        public static Room GetClosestRoom(Vector3 position)
        {
            Room closest = null;
            float closestDist = float.MaxValue;
            foreach (var room in AllRooms)
            {
                if (room == null) continue;
                var dist = Vector3.Distance(position, room.Position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = room;
                }
            }
            return closest;
        }

        /// <summary>Zona da sala mais próxima da posição (None se não houver sala).</summary>
        public static FacilityZone GetZoneAt(Vector3 position)
        {
            var room = GetClosestRoom(position);
            return room?.Zone ?? FacilityZone.None;
        }

        /// <summary>Posição spawnável na sala (Vector3.zero se room for null).</summary>
        public static Vector3 GetSpawnablePosition(Room room, float yOffset = 1.5f)
        {
            if (room == null) return Vector3.zero;
            return room.Position + Vector3.up * yOffset;
        }

        /// <summary>Jogadores vivos na zona indicada (vazio se Player.List for null).</summary>
        public static IEnumerable<Player> GetPlayersInZone(FacilityZone zone)
        {
            if (Player.List == null) return Enumerable.Empty<Player>();
            return Player.List.Where(p =>
                p != null && !p.IsDestroyed &&
                p.Team != PlayerRoles.Team.Dead &&
                GetZoneAt(p.Position) == zone);
        }

        /// <summary>Jogadores vivos dentro do raio da sala (vazio se room ou Player.List for null).</summary>
        public static IEnumerable<Player> GetPlayersInRoom(Room room, float radius = 15f)
        {
            if (room == null || Player.List == null) return Enumerable.Empty<Player>();
            return Player.List.Where(p =>
                p != null && !p.IsDestroyed &&
                p.Team != PlayerRoles.Team.Dead &&
                Vector3.Distance(p.Position, room.Position) <= radius);
        }

        /// <summary>Jogadores vivos dentro do raio da posição (vazio se Player.List for null).</summary>
        public static IEnumerable<Player> GetPlayersInRadius(Vector3 center, float radius)
        {
            if (Player.List == null) return Enumerable.Empty<Player>();
            return Player.List.Where(p =>
                p != null && !p.IsDestroyed &&
                p.Team != PlayerRoles.Team.Dead &&
                Vector3.Distance(p.Position, center) <= radius);
        }

        /// <summary>Distância entre duas salas (float.MaxValue se alguma for null).</summary>
        public static float DistanceBetweenRooms(Room a, Room b)
        {
            if (a == null || b == null) return float.MaxValue;
            return Vector3.Distance(a.Position, b.Position);
        }
    }
}
