using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Room
{
    public static class RoomManager
    {
        public static List<RoomInstance> Rooms = new List<RoomInstance>();

        private static long LastRoomID = 0;

        public static void Init()
        {
            AddRoom(new RoomInstance("Default Game"));
        }

        public static void AddRoom(RoomInstance room)
        {
            lock(Rooms)
            {
                LastRoomID++;
                room.ID = LastRoomID;
                Rooms.Add(room);
            }
        }

        public static void RemoveRoom(RoomInstance room)
        {
            room.Shutdown();
            lock (Rooms)
                Rooms.Remove(room);
        }

        public static List<Tuple<long, string>> GetRoomList()
        {
            List<Tuple<long, string>> l = new List<Tuple<long, string>>();
            lock(Rooms)
            {
                foreach (var r in Rooms)
                    l.Add(new Tuple<long, string>(r.ID, r.Name));
            }

            return l;
        }

        public static RoomInstance GetRoom(long id)
        {
            lock (Rooms)
                return Rooms.Find(x => x.ID == id);
        }
    }
}
