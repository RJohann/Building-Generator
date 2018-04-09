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
            // 1 - Select a site curve

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

            // 1 - Select a site curve FINISHED

            // 2 - Extract the border from the precinct surface
            Curve[] offsets = site.Offset(Plane.WorldXY, -2000, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Chamfer);
            Curve[] joinedoffset = Curve.JoinCurves(offsets); //join offset curves

            List<Extrusion> buildings = new List<Extrusion>(); //create a empty list of extrusions to store buildings

            Curve[] offsets1 = site.Offset(Plane.WorldXY, -4000, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Chamfer);
            Curve[] joinedoffset1 = Curve.JoinCurves(offsets1); //join offset curves

            //Curve ApartmentOultine = null;

            //if (joinedoffset.Length == 1)
            //    ApartmentOultine = joinedoffset[0];

            //if (ApartmentOultine == null)
            //    return Result.Failure;

            //foreach (Curve ShopCrv in joinedoffset)

            //Extrusion.Create(ApartmentOultine, 5000, true);


            foreach (Curve ShopCrv in joinedoffset)
            {
                Extrusion Shop = Extrusion.Create(ShopCrv, 3000, true);
                buildings.Add(Shop);
                RhinoDoc.ActiveDoc.Objects.AddExtrusion(Shop);
            }
                RhinoDoc.ActiveDoc.Views.Redraw();

            // 2 - Extrude the shop level FINISHED

                       
            //Extrusion Apartments

            // 3 - Extrude basement (7 spaces)
            //Extrusion of basement
            foreach (Curve ShopCrv in joinedoffset1)
            {
                Extrusion Shop = Extrusion.Create(ShopCrv, -2800, true);
                buildings.Add(Shop);
                RhinoDoc.ActiveDoc.Objects.AddExtrusion(Shop);
            }
                RhinoDoc.ActiveDoc.Views.Redraw();

            // Extrude aprtment levels 





            return Result.Success;
        }
    }
}