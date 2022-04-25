using System;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using Revit.Elements.Views;

// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class RoomTags
    {
        internal RoomTags()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomTag"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool IsInRoom(Element roomTag)
        {
            if (!(roomTag?.InternalElement is Autodesk.Revit.DB.Architecture.RoomTag rt))
                throw new ArgumentException(nameof(roomTag));

            return rt.IsInRoom;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomTag"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool IsOrphaned(Element roomTag)
        {
            if (!(roomTag?.InternalElement is Autodesk.Revit.DB.Architecture.RoomTag rt))
                throw new ArgumentException(nameof(roomTag));

            return rt.IsOrphaned;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomTag"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool IsTaggingLink(Element roomTag)
        {
            if (!(roomTag?.InternalElement is Autodesk.Revit.DB.Architecture.RoomTag rt))
                throw new ArgumentException(nameof(roomTag));

            return rt.IsTaggingLink;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomTag"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string TagText(Element roomTag)
        {
            if (!(roomTag?.InternalElement is Autodesk.Revit.DB.Architecture.RoomTag rt))
                throw new ArgumentException(nameof(roomTag));

            return rt.TagText;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomTag"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static View View(Element roomTag)
        {
            if (!(roomTag?.InternalElement is Autodesk.Revit.DB.Architecture.RoomTag rt))
                throw new ArgumentException(nameof(roomTag));

            return rt.View.ToDSType(true) as View;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomTag"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static int TaggedLocalRoomId(Element roomTag)
        {
            if (!(roomTag?.InternalElement is Autodesk.Revit.DB.Architecture.RoomTag rt))
                throw new ArgumentException(nameof(roomTag));

            return rt.TaggedLocalRoomId.IntegerValue;
        }
    }
}
