using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Building_Generator
{
    [System.Runtime.InteropServices.Guid("8a0a7978-0e9a-4793-8595-99ac18f3b928")]
    public class BuildingGeneratorCommand : Command
    {
        public BuildingGeneratorCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static BuildingGeneratorCommand Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "BuildingGeneratorCommand"; }

        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            //select a site curve

            GetObject obj = new GetObject();
            obj.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
            obj.SetCommandPrompt("Please select a curve representing your site");

            GetResult res = obj.Get();

            Curve site;

            if(res != GetResult.Object)
            {
                RhinoApp.WriteLine("The user did not select a curve");
                return Result.Failure; // Failed to get a curve 
            }
            if (obj.ObjectCount == 1)
            {
                site = obj.Object(0).Curve();
            }
            else
            {
                return Result.Failure; //Failed to get a curve
            }

            //extract the border from the precinct surface
            Curve[] offsets = site.Offset(Plane.WorldXY, -3000, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Chamfer);
            Curve[] joinedoffset = Curve.JoinCurves(offsets); //join offset curves

            List<Extrusion> buildings = new List<Extrusion>(); //create a empty list of extrusions to store buildings

            foreach (Curve itOff in joinedoffset)
            {
                Extrusion bld = Extrusion.Create(itOff, 1000, true);
                buildings.Add(bld);
                RhinoDoc.ActiveDoc.Objects.AddExtrusion(bld);
            }
            RhinoDoc.ActiveDoc.Views.Redraw();

            return Result.Success;
        }
    }
}