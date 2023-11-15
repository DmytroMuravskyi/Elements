﻿using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Elements.Representations.DoorRepresentations;

namespace Elements
{
    /// <summary>Definition of a door</summary>
    public class Door : GeometricElement
    {
        private readonly Material DEFAULT_MATERIAL = new Material("Door material", Colors.White);

        /// <summary>
        /// Default thickness of a door.
        /// </summary>
        public const double DOOR_DEFAULT_THICKNESS = 0.125;
        /// <summary>
        /// Default thickness of a door frame.
        /// </summary>
        public const double DOOR_FRAME_THICKNESS = 0.15;
        /// <summary>
        /// Default width of a door frame.
        /// </summary>
        public const double DOOR_FRAME_WIDTH = 2 * 0.0254; //2 inches

        /// <summary>Door width without a frame</summary>
        public double ClearWidth { get; private set; }
        /// <summary>The opening type of the door that should be placed</summary>
        public DoorOpeningType OpeningType { get; private set; }
        /// <summary>The opening side of the door that should be placed</summary>
        public DoorOpeningSide OpeningSide { get; private set; }
        /// <summary>Height of a door without a frame.</summary>
        public double ClearHeight { get; private set; }
        /// <summary>Door thickness.</summary>
        public double Thickness { get; private set; }

        private readonly double _fullDoorWidthWithoutFrame;
        private readonly DoorRepresentationProvider _representationProvider;

        /// <summary>
        /// Create a door.
        /// </summary>
        /// <param name="clearWidth">The width of a single door.</param>
        /// <param name="clearHeight">Height of the door without frame.</param>
        /// <param name="thickness">Door thickness.</param>
        /// <param name="openingSide">The side where the door opens.</param>
        /// <param name="openingType">The way the door opens.</param>
        /// <param name="transform">The door's transform. X-direction is aligned with the door, Y-direction is the opening direction.</param>
        /// <param name="material">The door's material.</param>
        /// <param name="representation">The door's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The door's id.</param>
        /// <param name="name">The door's name.</param>
        [JsonConstructor]
        public Door(double clearWidth,
                double clearHeight,
                double thickness,
                DoorOpeningSide openingSide,
                DoorOpeningType openingType,
                Transform transform = null,
                Material material = null,
                Representation representation = null,
                bool isElementDefinition = false,
                Guid id = default,
                string name = "Door"
            ) : base(
                    transform: transform,
                    representation: representation,
                    isElementDefinition: isElementDefinition,
                    id: id,
                    name: name
                )
        {
            OpeningSide = openingSide;
            OpeningType = openingType;
            ClearHeight = clearHeight;
            ClearWidth = clearWidth;
            Thickness = thickness;
            Material = material ?? DEFAULT_MATERIAL;
            _fullDoorWidthWithoutFrame = GetDoorFullWidthWithoutFrame(clearWidth, openingSide);

            _representationProvider = new DoorRepresentationProvider(new DefaultDoorRepresentationFactory());
        }

        /// <summary>
        /// Create a door at the certain point of a line.
        /// </summary>
        /// <param name="line">The line where the door is placed.</param>
        /// <param name="tPos">Relative position on the line where door is placed. Should be in [0; 1].</param>
        /// <param name="clearWidth">The width of a single door.</param>
        /// <param name="clearHeight">Height of the door without frame.</param>
        /// <param name="thickness">Door thickness.</param>
        /// <param name="openingSide">The side where the door opens.</param>
        /// <param name="openingType">The way the door opens.</param>
        /// <param name="material">The door's material.</param>
        /// <param name="representation">The door's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The door's id.</param>
        /// <param name="name">The door's name.</param>
        public Door(Line line,
                    double tPos,
                    double clearWidth,
                    double clearHeight,
                    double thickness,
                    DoorOpeningSide openingSide,
                    DoorOpeningType openingType,
                    Material material = null,
                    Representation representation = null,
                    bool isElementDefinition = false,
                    Guid id = default,
                    string name = "Door"
            ) : base(
                    representation: representation,
                    isElementDefinition: isElementDefinition,
                    id: id,
                    name: name
                )
        {
            OpeningType = openingType;
            OpeningSide = openingSide;
            ClearWidth = clearWidth;
            ClearHeight = clearHeight;
            Thickness = thickness;
            Material = material ?? DEFAULT_MATERIAL;
            _fullDoorWidthWithoutFrame = GetDoorFullWidthWithoutFrame(ClearWidth, openingSide);
            Transform = GetDoorTransform(line.PointAtNormalized(tPos), line);

            _representationProvider = new DoorRepresentationProvider(new DefaultDoorRepresentationFactory());
        }

        /// <summary>
        /// Create an opening for the door.
        /// </summary>
        /// <param name="depthFront">The door's opening depth front.</param>
        /// <param name="depthBack">The door's opening depth back.</param>
        /// <param name="flip">Is the opening flipped?</param>
        /// <returns>An opening where the door can be inserted.</returns>
        public Opening CreateDoorOpening(double depthFront, double depthBack, bool flip)
        {
            var openingWidth = _fullDoorWidthWithoutFrame + 2 * DOOR_FRAME_WIDTH;
            var openingHeight = ClearHeight + DOOR_FRAME_WIDTH;

            var openingDir = flip ? Vector3.YAxis.Negate() : Vector3.YAxis;
            var widthDir = flip ? Vector3.XAxis.Negate() : Vector3.XAxis;
            var openingTransform = new Transform(0.5 * openingHeight * Vector3.ZAxis, widthDir, openingDir);

            var openingPolygon = Polygon.Rectangle(openingWidth, openingHeight).TransformedPolygon(openingTransform);

            var opening = new Opening(openingPolygon, openingDir, depthFront, depthBack, Transform);
            return opening;
        }

        private Transform GetDoorTransform(Vector3 currentPosition, Line wallLine)
        {
            var adjustedPosition = GetClosestValidDoorPos(wallLine, currentPosition);
            var xDoorAxis = wallLine.Direction();
            return new Transform(adjustedPosition, xDoorAxis, Vector3.ZAxis);
        }

        /// <summary>
        /// Checks if the door can fit into the wall with the center line @<paramref name="wallLine"/>.
        /// </summary>
        public static bool CanFit(Line wallLine, DoorOpeningSide openingSide, double width)
        {
            var doorWidth = GetDoorFullWidthWithoutFrame(width, openingSide) + DOOR_FRAME_WIDTH * 2;
            return wallLine.Length() - doorWidth > DOOR_FRAME_WIDTH * 2;
        }

        /// <summary>
        /// Update the representations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            RepresentationInstances = _representationProvider.GetInstances(this);
        }

        private Vector3 GetClosestValidDoorPos(Line wallLine, Vector3 currentPosition)
        {
            var fullWidth = _fullDoorWidthWithoutFrame + DOOR_FRAME_WIDTH * 2;
            double wallWidth = wallLine.Length();
            Vector3 p1 = wallLine.PointAt(0.5 * fullWidth);
            Vector3 p2 = wallLine.PointAt(wallWidth - 0.5 * fullWidth);
            var reducedWallLine = new Line(p1, p2);
            return currentPosition.ClosestPointOn(reducedWallLine);
        }

        internal double GetFullDoorWidthWithoutFrame()
        {
            return _fullDoorWidthWithoutFrame;
        }

        private static double GetDoorFullWidthWithoutFrame(double doorClearWidth, DoorOpeningSide doorOpeningSide)
        {
            switch (doorOpeningSide)
            {
                case DoorOpeningSide.LeftHand:
                case DoorOpeningSide.RightHand:
                    return doorClearWidth;
                case DoorOpeningSide.DoubleDoor:
                    return doorClearWidth * 2;
            }
            return 0;
        }
    }
}
