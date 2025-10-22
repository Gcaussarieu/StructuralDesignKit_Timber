using StructuralDesignKitLibrary.Connections.Fasteners;
using StructuralDesignKitLibrary.Connections.SteelTimberShear;
using StructuralDesignKitLibrary.Connections.TimberTimberShear;
using StructuralDesignKitLibrary.EC5;
using StructuralDesignKitLibrary.Materials;
using StructuralDesignKitLibrary.RFEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK = StructuralDesignKitLibrary;
using ApplicationClient = Dlubal.WS.Rfem6.Application.RfemApplicationClient;
using ModelClient = Dlubal.WS.Rfem6.Model.RfemModelClient;
using Dlubal.WS.Rfem6.Application;
using Dlubal.WS.Rfem6.Model;
using StructuralDesignKitLibrary.CrossSections;
using System.Diagnostics;

namespace SDK_Console_Dev
{
	internal class Program
	{
		static void Main(string[] args)
		{





            //MaterialPanelOSB OSB = new MaterialPanelOSB(MaterialPanelOSB.Grades.OSB3.ToString(), 18);
            //MaterialTimberHardwood hardWood = new MaterialTimberHardwood(MaterialTimberHardwood.Grades.D30);
            MaterialTimberSoftwood softWood = new MaterialTimberSoftwood(MaterialTimberSoftwood.Grades.C24);
            //MaterialTimberGlulam glulam = new MaterialTimberGlulam(MaterialTimberGlulam.Grades.GL32h);

            //FastenerNail Nail = new FastenerNail(FastenerNail.NailTypes.AnnularRing, 5, 57, 600, true, 0, 0, 3.64, 13.53, 100, 4680);


            //var connection1 = new TimberTimberSingleShear(Nail, OSB, 22, 0, softWood, 100, 0, true);

            //double nef = Nail.ComputeEffectiveNumberOfFastener(10, 100, 45);

            //Nail.ComputeSpacings(45, connection1);
            //Nail.ComputeMinimumMinMemberThickness(connection1.Timber2.RhoK, true);

            //Type type = Nail.GetType();
            //var prop = type.GetProperties();

            //foreach (var property in prop)
            //{
            //	Console.WriteLine("{0} -> {1}", property.Name.ToString(), property.GetValue(Nail).ToString());
            //}

            //Console.WriteLine("nef -> {0}", nef);

            //for (int i = 0; i < connection1.Capacities.Count; i++)
            //{
            //	Console.WriteLine("{0} -> {1:0.0}", connection1.FailureModes[i], connection1.Capacities[i]);

            //}

            //double kmod = EC5_Utilities.KmodFromDifferentWood(connection1.Timber1, connection1.Timber2, EC5_Utilities.ServiceClass.SC2, EC5_Utilities.LoadDuration.ShortTerm_Instantaneous);

            //Console.WriteLine("Kmod -> {0}\nFvrd -> {1:0.000}", kmod, connection1.Capacity / 1000 / 1.3 * kmod);


            //var stiffness = connection1.Kser;
            //Console.WriteLine("Kser -> {0}", stiffness);



            ////for (int i = 1; i < 20; i++)
            ////{
            ////	bool predrilled = false;
            ////	if (i>6) predrilled = true;



            //////             Console.WriteLine("Diam = {1} -> Fhk = {0}", Nail.Fhk, i);
            //////}

            //////Console.WriteLine(OSB.Grade_Thickness.ToString());


            //SDK.Materials.MaterialTimberSoftwood c24 = new MaterialTimberSoftwood(MaterialTimberSoftwood.Grades.C24);
            //SDK.CrossSections.CrossSectionRectangular CS = new CrossSectionRectangular(100, 200, c24);

            //double stress = CS.ComputeNormalStress(100);

            //double check = SDK.EC5.EC5_CrossSectionCheck.TensionParallelToGrain(stress, c24, 0.9, 1.3, 1, 1, false);
            //Console.WriteLine("Check = {0:0.00}", check);

            //var softWood = new MaterialTimberSoftwood(MaterialTimberSoftwood.Grades.C24);
            var OSB = new MaterialPanelOSB(MaterialPanelOSB.Grades.OSB3.ToString(), 15);

            double diam = 2.5;
            double spacings = 100; //mm
            spacings /= 1000;

            FastenerNail Nail = new FastenerNail(FastenerNail.NailTypes.AnnularRing, diam, 60, 600, false, 2550, 2 * diam, 7, 20, 48, 2550);
            var connection1 = new TimberTimberSingleShear(Nail, OSB, 16, 0, softWood,45, 0, true);
            //var connection2 = new SingleOuterSteelPlate(Nail, 6, 0, softWood, 40, true);
            Nail.ComputeSpacings(0, connection1);
            Console.WriteLine("Timber/timber");
            Console.WriteLine("Rk =  {0:0.00} N", connection1.Capacity);
            Console.WriteLine("A1 = {0}", Nail.a1min);
            Console.WriteLine("A4 = {0}", Nail.a4cmin);


            //Console.WriteLine("Steel/timber");
            Console.WriteLine("Fvk = {0:0.00}kN/m", connection1.Capacity / spacings/1000 );
            //Console.WriteLine("Kser = {0:0.00}kN/m", connection2.Kser / spacings);





            //var mat = new MaterialCLT(MaterialCLT.ProducerGrade.KLH);

            //List<double> thicknesses = new List<double> { 40, 40, 40 };
            //List<int> orientations = new List<int> { 0, 90, 0 };
            //List<MaterialCLT> mats = new List<MaterialCLT>();
            //foreach (int i in thicknesses) mats.Add(mat);

            //var layup = new CLT_Layup(thicknesses, orientations, mats);

            //Console.WriteLine("CoG X : {0:0.0}mm", layup.CS_X.CenterOfGravity);
            //Console.WriteLine("CoG Y: {0:0.0}mm", layup.CS_Y.CenterOfGravity);
            //Console.WriteLine("Inertia : ",layup.CS_X.EffectiveMomentOfInertia);


            //for (int i = 0; i < layup.CS_X.S0Net.Count; i++)
            //{
            //    for (int j = 0; j < layup.CS_X.S0Net[i].Count; j++)
            //    {
            //        Console.WriteLine("Layer {0} point {1} -> S0Net = {2:0.0}", i, j, layup.CS_X.S0Net[i][j]);
            //    }
            //}


            //layup.CS_X.ComputeShearStress(13500);
            //for (int i = 0; i < layup.CS_X.TauV.Count; i++)
            //{
            //    for (int j = 0; j < layup.CS_X.TauV[i].Count; j++)
            //    {
            //        Console.WriteLine("Layer {0} point {1} -> Tau = {2:0.000}", i, j, layup.CS_X.TauV[i][j]);
            //    }
            //}


            //         Console.WriteLine("Tensile capacity in X : {0:0.0} kN", layup.CS_X.NTensionChar);
            //         Console.WriteLine("Compressive capacity in X : {0:0.0} kN", layup.CS_X.NCompressionChar);

            //Console.WriteLine("Tensile capacity in Y : {0:0.0} kN", layup.CS_Y.NTensionChar);
            //Console.WriteLine("Compressive capacity in Y : {0:0.0} kN", layup.CS_Y.NCompressionChar);

            //var BM1 = new MaterialTimberSoftwood(MaterialTimberSoftwood.Grades.C24);
            //var BM2 = new MaterialTimberHardwood(MaterialTimberHardwood.Grades.D30);
            //var LC = new MaterialTimberGlulam(MaterialTimberGlulam.Grades.GL24h);
            //var Baubuche = new MaterialTimberBaubuche(MaterialTimberBaubuche.Grades.GL75h_Cl1);

            //List<IMaterial> Materials = new List<IMaterial>() { BM1, BM2, LC, Baubuche };

            //foreach (var material in Materials) {
            //	double kv = EC5_Factors.Kv(material, 300, 150, 70,0);
            //	Console.WriteLine("Kv = " + kv.ToString());
            //}



            //var mat = new MaterialTimberGlulam(MaterialTimberGlulam.Grades.GL24h);
            //var cs = new CrossSectionRectangular(280, 680, mat);

            //double eta = EC5_CrossSectionCheck.BendingAndBuckling(1.85, 0, 3.56, 11, 11, cs, mat, 0.7, 1.25);

            //var dowel = new FastenerDowel(12, 400);
            //dowel.ComputeSpacings(90, null);
            //double a3c = dowel.a3cmin;


            //Console.WriteLine( eta);

            //FastenerNail Nail = new FastenerNail(FastenerNail.NailTypes.AnnularRing, 6.1,148, 600, true,36000,20,10, 20, 160,33000);

            //SingleOuterSteelPlate connection2 = new SingleOuterSteelPlate(Nail,12, 0, new MaterialTimberGlulam(MaterialTimberGlulam.Grades.GL28c), 148,true);



            //         double blockcapacity = SDK.EC5.EC5_ConnectionCheck.BlockShearFailureCapacity(connection2,164, 1619);

            //         Console.WriteLine("Screw Capacity {0} = {1:0.00} kN", connection2.FailureModes[2], connection2.Capacities[2] / 1000);
            //         Console.WriteLine("Screw Capacity {0} = {1:0.00} kN", connection2.FailureModes[3], connection2.Capacities[3] / 1000);
            //         Console.WriteLine("Screw Capacity {0} = {1:0.00} kN", connection2.FailureModes[4], connection2.Capacities[4] / 1000);
            //         Console.WriteLine("Screw Capacity = {0:0.00} kN", connection2.Capacity / 1000);

            //Console.WriteLine("Block capacity : {0:0.00} kN", blockcapacity/1000);




            Console.ReadLine();
		}
	}

}