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
        private Brep Buildings;

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

            if (res != GetResult.Object)
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


          
            // 2 - Extract the border from the precinct surface
            //Offset for Shop
            Curve[] offsets = site.Offset(Plane.WorldXY, -2.5, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Chamfer);
            Curve[] joinedoffset = Curve.JoinCurves(offsets); //join offset curves

            //Offset for Apartment
            Curve[] offsetBst = site.Offset(Plane.WorldXY, -4, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Chamfer);
            Curve[] joinedoffsetBst = Curve.JoinCurves(offsetBst); //join offset curves

            //Offset for Base
            Curve[] offsetTop = site.Offset(Plane.WorldXY, -3, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Chamfer);
            Curve[] joinedoffsetTop = Curve.JoinCurves(offsetTop); //join offset curves

                //List of offset curves
            List<Curve> buildingsCrv = new List<Curve>(); 
            buildingsCrv.AddRange(offsets);
            buildingsCrv.AddRange(offsetBst);
            buildingsCrv.AddRange(offsetTop);



            // 3 - Extrude all the offset curves
            //Extrusions 
            Extrusion Shop = Extrusion.Create(buildingsCrv[0], 3.1, true);
            Extrusion Apa = Extrusion.Create(buildingsCrv[1], 18.6, true);
            Extrusion Base = Extrusion.Create(buildingsCrv[2], -3.1, true);

                //List of extrusions                               
            List<Extrusion> buildingsExt = new List<Extrusion>(); 
            buildingsExt.Add(Shop);
            buildingsExt.Add(Apa);
            buildingsExt.Add(Base);

            //Draw all the extrusions
            foreach (Extrusion itExt in buildingsExt) 
            {
                RhinoDoc.ActiveDoc.Objects.Add(itExt);
            }



            // 4 - Create contour lines on extrusions to represent floors
            //Define extrusions as Breps for contours
            Brep ShopBrep = Shop.ToBrep();
            Brep BrepApa = Apa.ToBrep();

                //List of Breps                             
            List<Brep> BuildingBreps = new List<Brep>();
            BuildingBreps.Add(ShopBrep);
            BuildingBreps.Add(BrepApa);

            //Points to define contours
            Point3d start = new Point3d(0, 0, 0);
            Point3d end = new Point3d(0, 0, 30);

            //Contours
            Curve[] Shopflr = Brep.CreateContourCurves(ShopBrep as Brep, start, end, 3.1);
            Curve[] ApaFlr = Brep.CreateContourCurves(BrepApa as Brep, start, end, 3.1);

                //List of Contour Curves                               
            List<Curve> Floors = new List<Curve>();
            Floors.AddRange(Shopflr);
            Floors.AddRange(ApaFlr);

            //Draw all the Contour curves
            foreach (Curve itCrv in Floors)
            {
                RhinoDoc.ActiveDoc.Objects.Add(itCrv);
            }




            RhinoDoc.ActiveDoc.Views.Redraw();

            return Result.Success;
        }
    }
}





      
