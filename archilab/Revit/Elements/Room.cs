using System;
using DynamoServices;

namespace archilab.Revit.Elements
{
    /// <summary>
    /// Wrapper class for Rooms.
    /// </summary>
    [RegisterForTrace]
    public class Room
    {
        internal Room()
        {
        }

        /// <summary>
        /// Room Name
        /// </summary>
        /// <param name="room">Room element.</param>
        /// <returns name="name">Name of the room.</returns>
        public static string Name(global::Revit.Elements.Element room)
        {
            if (room == null)
            {
                throw new ArgumentNullException(nameof(room));
            }

            var rm = (Autodesk.Revit.DB.SpatialElement)room.InternalElement;
            var name = rm.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.ROOM_NAME).AsString();
            return name;
        }
    }
}
