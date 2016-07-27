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

        public static int MaxRoomCount = 100;

        public class RoomEventArgs : EventArgs
        {
            public RoomInstance AffectedRoom = null;

            public RoomEventArgs(RoomInstance r) { AffectedRoom = r; }
        }

        public static event EventHandler<RoomEventArgs> RoomAdded = null;
        public static event EventHandler<RoomEventArgs> RoomUpdated = null;
        public static event EventHandler<RoomEventArgs> RoomRemoved = null;

        public static void Init()
        {
            AddRoom(new RoomInstance("Default Game"));
        }

        public static long AddRoom(RoomInstance room)
        {
            lock(Rooms)
            {
                if (Rooms.Count >= (MaxRoomCount - 1))
                    return long.MinValue;

                LastRoomID++;
                room.ID = LastRoomID;
                if (room.Name == string.Empty)
                    room.Name = "Room_" + room.ID.ToString();

                Rooms.Add(room);
                room.Startup();
            }

            RoomAdded?.Invoke(room,new RoomEventArgs(room));
            return room.ID;
        }

        public static void RemoveRoom(RoomInstance room)
        {
            room.Shutdown();
            lock (Rooms)
                Rooms.Remove(room);

            RoomRemoved?.Invoke(room, new RoomEventArgs(room));
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
