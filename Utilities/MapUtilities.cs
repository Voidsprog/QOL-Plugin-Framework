using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using MapGeneration;
using UnityEngine;

namespace QOLFramework.Utilities
{
    public static class MapUtilities
    {
        public static IReadOnlyList<Room> AllRooms => Map.Rooms?.ToList() ?? new List<Room>();

        public static IEnumerable<Room> GetRoomsByZone(FacilityZone zone)
        {
            return AllRooms.Where(r => r.Zone == zone);
        }

        public static IEnumerable<Room> GetLczRooms() => GetRoomsByZone(FacilityZone.LightContainment);
        public static IEnumerable<Room> GetHczRooms() => GetRoomsByZone(FacilityZone.HeavyContainment);
        public static IEnumerable<Room> GetEzRooms() => GetRoomsByZone(FacilityZone.Entrance);
        public static IEnumerable<Room> GetSurfaceRooms() => GetRoomsByZone(FacilityZone.Surface);

        public static string GetRoomName(Room room)
        {
            try { return room?.Name.ToString() ?? ""; }
            catch { return ""; }
        }

        public static IEnumerable<Room> FindRoomsByName(string partialName, FacilityZone? zone = null)
        {
            var rooms = zone.HasValue ? GetRoomsByZone(zone.Value) : AllRooms;
            var upper = partialName.ToUpperInvariant();
            return rooms.Where(r => GetRoomName(r).ToUpperInvariant().Contains(upper));
        }

        public static Room GetRandomRoom(FacilityZone? zone = null)
        {
            var rooms = (zone.HasValue ? GetRoomsByZone(zone.Value) : AllRooms).ToList();
            return rooms.Count == 0 ? null : rooms[UnityEngine.Random.Range(0, rooms.Count)];
        }

        public static Room GetClosestRoom(Vector3 position)
        {
            Room closest = null;
            float closestDist = float.MaxValue;
            foreach (var room in AllRooms)
            {
                var dist = Vector3.Distance(position, room.Position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = room;
                }
            }
            return closest;
        }

        public static FacilityZone GetZoneAt(Vector3 position)
        {
            var room = GetClosestRoom(position);
            return room?.Zone ?? FacilityZone.None;
        }

        public static Vector3 GetSpawnablePosition(Room room, float yOffset = 1.5f)
        {
            if (room == null) return Vector3.zero;
            return room.Position + Vector3.up * yOffset;
        }

        public static IEnumerable<Player> GetPlayersInZone(FacilityZone zone)
        {
            return Player.List.Where(p =>
                p != null && !p.IsDestroyed &&
                p.Team != PlayerRoles.Team.Dead &&
                GetZoneAt(p.Position) == zone);
        }

        public static IEnumerable<Player> GetPlayersInRoom(Room room, float radius = 15f)
        {
            if (room == null) return Enumerable.Empty<Player>();
            return Player.List.Where(p =>
                p != null && !p.IsDestroyed &&
                p.Team != PlayerRoles.Team.Dead &&
                Vector3.Distance(p.Position, room.Position) <= radius);
        }

        public static IEnumerable<Player> GetPlayersInRadius(Vector3 center, float radius)
        {
            return Player.List.Where(p =>
                p != null && !p.IsDestroyed &&
                p.Team != PlayerRoles.Team.Dead &&
                Vector3.Distance(p.Position, center) <= radius);
        }

        public static float DistanceBetweenRooms(Room a, Room b)
        {
            if (a == null || b == null) return float.MaxValue;
            return Vector3.Distance(a.Position, b.Position);
        }
    }
}
