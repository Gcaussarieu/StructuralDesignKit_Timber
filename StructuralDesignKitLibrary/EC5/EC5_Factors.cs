﻿using StructuralDesignKitLibrary.CrossSections;
using StructuralDesignKitLibrary.CrossSections.Interfaces;
using StructuralDesignKitLibrary.Materials;
using StructuralDesignKitLibrary.EC5;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StructuralDesignKitLibrary.EC5.EC5_Utilities;

namespace StructuralDesignKitLibrary.EC5
{
	public static class EC5_Factors
	{


		#region Kmod

		// Kmod is a modification factor taking into account the effect of the duration of load and moisture content.
		// Values according to EN 1995-1-1:2004 - Table 3.1


		// The Kmod values are given in the following order:
		// Permanent, LongTerm, MediumTerm, ShortTerm, Instantaneous


		private static double[,] KmodSolidWood =
			{
				{0.60, 0.70, 0.80, 0.90, 1.1  },    //Class 1 
                {0.60, 0.70, 0.80, 0.90, 1.1 },     //Class 2 
                {0.50, 0.55, 0.65, 0.70, 0.9 }      //Class 3 
            };

		private static double[,] KmodGlulam =
			{
				{0.60, 0.70, 0.80, 0.90, 1.1  },    //Class 1 
                {0.60, 0.70, 0.80, 0.90, 1.1 },     //Class 2 
                {0.50, 0.55, 0.65, 0.70, 0.9 }      //Class 3 
            };

		private static double[,] KmodLVL =
	{
				{0.60, 0.70, 0.80, 0.90, 1.1  },    //Class 1 
                {0.60, 0.70, 0.80, 0.90, 1.1 },     //Class 2 
                {0.50, 0.55, 0.65, 0.70, 0.9 }      //Class 3 
            };

		//Only for OSB/3 and OSB/4
		private static double[,] KmodOSB =
   {
				{0.40, 0.50, 0.70, 0.90, 1.10  },    //Class 1 
                {0.30, 0.40, 0.55, 0.70, 0.90},     //Class 2 
                {0.0, 0.0, 0.0, 0.0, 0.0 }      //Class 3 -> Not applicable for OSB
            };


		private static Dictionary<TimberType, double[,]> KmodValues = new Dictionary<TimberType, double[,]>
		{
			{ TimberType.Softwood,KmodSolidWood },
			{ TimberType.Hardwood,KmodSolidWood },
			{ TimberType.Glulam,KmodGlulam },
			{ TimberType.Baubuche,KmodGlulam },
			{ TimberType.LVL,KmodLVL },
			{ TimberType.OSB,KmodOSB },
		};

		/// <summary>
		/// Kmod is a modification factor taking into account the effect of the duration of load and moisture content.
		/// </summary>
		/// <param name="timberType"></param>
		/// <param name="serviceClass"></param>
		/// <param name="loadDuration"></param>
		/// <returns></returns>
		[Description("Kmod is a modification factor taking into account the effect of the duration of load and moisture content.")]
		public static double Kmod(TimberType timberType, ServiceClass serviceClass, LoadDuration loadDuration)
		{

			int SC = 0;
			double kmod = 0;

			switch (serviceClass)
			{
				case ServiceClass.SC1:
					SC = 0;
					break;
				case ServiceClass.SC2:
					SC = 1;
					break;
				case ServiceClass.SC3:
					SC = 2;

					//prevent calculation of OSB in SC3
					if (timberType == TimberType.OSB) throw new Exception("OSB cannot be use in Service Class 3");
					break;
			}

			switch (loadDuration)
			{
				case LoadDuration.Permanent:
					kmod = EC5_Factors.KmodValues[timberType][SC, 0];
					break;
				case LoadDuration.LongTerm:
					kmod = EC5_Factors.KmodValues[timberType][SC, 1];
					break;
				case LoadDuration.MediumTerm:
					kmod = EC5_Factors.KmodValues[timberType][SC, 2];
					break;
				case LoadDuration.ShortTerm:
					kmod = EC5_Factors.KmodValues[timberType][SC, 3];
					break;
				case LoadDuration.Instantaneous:
					kmod = EC5_Factors.KmodValues[timberType][SC, 4];
					break;
				case LoadDuration.ShortTerm_Instantaneous:
					// Load duration available in German NA for wind load duration. Kmod being the average 
					kmod = (EC5_Factors.KmodValues[timberType][SC, 3] + EC5_Factors.KmodValues[timberType][SC, 4]) / 2;
					break;
			}

			return kmod;

		}
		#endregion


		#region Ym
		// Ym represents the material's safety factor
		// Values according to DIN EN 1995-1-1 
		public static double Ym(TimberType timberType)
		{
			double ym = 0;
			switch (timberType)
			{
				case TimberType.Softwood:
					ym = 1.3;
					break;
				case TimberType.Hardwood:
					ym = 1.3;
					break;
				case TimberType.Glulam:
					ym = 1.3;
					break;
				case TimberType.LVL:
					ym = 1.3;
					break;
				case TimberType.Baubuche:
					ym = 1.3;
					break;

				case TimberType.OSB:
					ym = 1.3;
					break;
				default:
					ym = 1.3;
					break;
			}

			return ym;
		}

		#endregion


		#region Kdef
		// Kdef is the Deformation factor to take into account the long time creep behaviour depending on the service class and the timber type
		// Values according to EN 1995-1-1:2004 - Table 3.2

		// Kdef values are given in the following order: Service class 1, 2 ,3
		private static double[] kdefSolidWood = { 0.6, 0.8, 2.0 };
		private static double[] kdefGlulam = { 0.6, 0.8, 2.0 };
		private static double[] kdefLVL = { 0.6, 0.8, 2.0 };
		private static double[] kdefOSB = { 1.50, 2.25, 0.0 };


		public static double Kdef(TimberType timberType, ServiceClass serviceClass)
		{
			double kdef = 0;
			int SC = (int)serviceClass;

			switch (timberType)
			{
				case TimberType.Softwood:
					kdef = kdefSolidWood[SC];
					break;
				case TimberType.Hardwood:
					kdef = kdefSolidWood[SC];
					break;
				case TimberType.Glulam:
					kdef = kdefGlulam[SC];
					break;
				case TimberType.LVL:
					kdef = kdefLVL[SC];
					break;
				case TimberType.Baubuche:
					kdef = kdefGlulam[SC];
					break;
				case TimberType.OSB:
					if (serviceClass == ServiceClass.SC3) throw new Exception("OSB Cannot be used in Service Class 3");
					kdef = kdefOSB[SC];
					break;
			}

			return kdef;
		}
		#endregion


		#region Kh
		// Size factor

		// According to "Timber Engineering - Principles for Design" from Hans Joachim Blaß & Carmen Sandhaas (ISBN 978-3-7315-0673-7):
		// Size effects are taken into consideration by modifying the characteristic strength values
		// determined in EN 338. The characteristic values for bending and tensile strength are
		// based on a reference height of 150 mm for solid timber and 600 mm for glued laminated
		// timber. For depths less than these reference values, strength values are multiplied by a size factor, which is limited by an upper value


		/// <summary>
		/// The size factor considers the inhomogeneities and other deviations from an ideal orthotropic material
		/// </summary>
		/// <param name="timberType"></param>
		/// <param name="h">Rectangular beam height</param>
		/// <returns></returns>
		public static double Kh_Bending(TimberType timberType, double h)
		{
			double kh = 1;
			switch (timberType)
			{
				case TimberType.Softwood:
					if (h < 150) kh = Math.Min(Math.Pow(150 / h, 0.2), 1.3);
					break;
				case TimberType.Hardwood:
					if (h < 150) kh = Math.Min(Math.Pow(150 / h, 0.2), 1.3);
					break;
				case TimberType.Glulam:
					if (h < 600) kh = Math.Min(Math.Pow(600 / h, 0.1), 1.1);
					break;
				case TimberType.LVL:
					//Value according to EN 1995-1-1:2004 - Eq (3.3) and Kerto Product Certificate No EUFI29-20000676-C/EN §6.5
					if (h != 300) kh = Math.Min(Math.Pow(300 / h, 0.12), 1.2);
					break;
				case TimberType.Baubuche:
					// For Baubuche, the modification factors are already taken into account in the material properties 
					// See MaterialTimberBaubuche
					kh = 1;
					break;

			}
			return kh;
		}

		/// <summary>
		/// The size factor considers the inhomogeneities and other deviations from an ideal orthotropic material
		/// According to the german annex NCI Zu 3.2(3) -> For components subjected to tensile stresses the section width means the largest section dimension.
		/// </summary>
		/// <param name="timberType"></param>
		/// <param name="b">Rectangular beam width</param>
		/// <param name="b">Rectangular beam height</param>
		/// <returns></returns>
		public static double Kh_Tension(TimberType timberType, double b, double h)
		{
			double kh = 1;
			switch (timberType)
			{
				case TimberType.Softwood:
					kh = Kh_Bending(timberType, Math.Max(b, h));
					break;
				case TimberType.Hardwood:
					kh = Kh_Bending(timberType, Math.Max(b, h));
					break;
				case TimberType.Glulam:
					kh = Kh_Bending(timberType, Math.Max(b, h));
					break;
				case TimberType.LVL:
					kh = 1;
					break;
				case TimberType.Baubuche:
					// For Baubuche, the modification factors are already taken into account in the material properties 
					// See MaterialTimberBaubuche
					kh = 1;
					break;
			}
			return kh;
		}

		#endregion


		#region Kl_LVL

		/// <summary>
		/// Size factor for LVL (or Baubuche) members submited to tensile force - according to EN 1995-1-1:2004 - Eq (3.4) + Kerto Product Certificate + Baubuche Design Assistance Guide
		/// </summary>
		/// <param name="timberType"></param>
		/// <param name="Length">Beam in tension total length</param>
		/// <returns></returns>
		[Description("Size factor for LVL (or Baubuche) members submited to tensile force - according to EN 1995-1-1:2004 - Eq (3.4) + Kerto Product Certificate + Baubuche Design Assistance Guide")]
		public static double Kl_LVL(TimberType timberType, double Length)
		{
			double kl = 1;
			switch (timberType)
			{
				case TimberType.Softwood:
					kl = 1;
					break;
				case TimberType.Hardwood:
					kl = 1;
					break;
				case TimberType.Glulam:
					kl = 1;
					break;
				case TimberType.LVL:
					//Value according to EN 1995-1-1:2004 - Eq (3.4) and Kerto Product Certificate No EUFI29-20000676-C/EN §6.5
					kl = Math.Min(Math.Pow(3000 / Length, 0.06), 1.1);
					break;
				case TimberType.Baubuche:
					//Value according to EN 1995-1-1:2004 - Eq (3.4) and Baubuche Design Assistance Guide P.11
					kl = Math.Min(Math.Pow(3000 / Length, (0.12 / 2)), 1.1);
					break;
			}
			return kl;
		}

		#endregion


		#region Kcr
		/// <summary>
		/// Computes the Crack factor for shear resistance Kcr - According to DIN EN 1995-1 NA  §6.1.7(2)
		/// </summary>
		/// <param name="material">Material object</param>
		/// <returns>Kcr value</returns>
		[Description("Computes the Crack factor for shear resistance Kcr - According to DIN EN 1995-1 NA  §6.1.7(2)")]
		public static double Kcr(IMaterial material)
		{
			double kcr = 1;

			IMaterialTimber timber;
			if (material is IMaterialTimber)
			{
				timber = (IMaterialTimber)material;
				switch (timber.Type)
				{
					case EC5_Utilities.TimberType.Softwood:
						//DIN EN 1995-1 NA-DE: Annotation to 6.1.7(2) for Softwood
						kcr = 2.0 / timber.Fvk;
						break;
					case EC5_Utilities.TimberType.Hardwood:
						kcr = 0.67;
						break;
					case EC5_Utilities.TimberType.Glulam:
						//DIN EN 1995-1 NA-DE: Annotation to 6.1.7(2) for Glulam
						kcr = 2.5 / timber.Fvk;
						break;
					case EC5_Utilities.TimberType.LVL:
						kcr = 1;
						break;
					case EC5_Utilities.TimberType.Baubuche:
						kcr = 1;
						break;
					case EC5_Utilities.TimberType.OSB:
						kcr = 1;
						break;
				}
			}

			return kcr;
		}

		#endregion


		#region KShape
		/// <summary>
		/// Factor depending on the shape of the cross-section (and Material for Baubuche) for Torsion check - According to EN 1995-1 Eq(6.15)
		/// </summary>
		/// <param name="crossSection">Cross Section Object</param>
		/// <param name="material">Material Object</param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		[Description("Factor depending on the shape of the cross-section (and Material for Baubuche) for Torsion check - According to EN 1995-1 Eq(6.15)")]
		public static double KShape(ICrossSection crossSection, IMaterial material)
		{

			double Kshape = 0;

			if (crossSection is CrossSectionRectangular)
			{
				CrossSectionRectangular rectCS = (CrossSectionRectangular)crossSection;

				//according to Baubuche design guide §4.1.8 Torsion and Eq (6.15) //Corrigendum August 2012 + Ammendment from 2014
				Kshape = Math.Min(1 + 0.05 * rectCS.H / rectCS.B, 1.3);

			}
			else throw new Exception("Currently only Rectangular Cross sections are covered for torsion check");

			return Kshape;
		}

		#endregion


		#region Km
		/// <summary>
		/// Factor considering re-distribution of bending stresses in a cross-section - According to EN 1995-1 §6.1.6(2)
		/// </summary>
		/// <param name="crossSection">Cross Section Object</param>
		/// <param name="material">Material Object</param>
		/// <returns>Returns the Km value</returns>
		[Description("Factor considering re-distribution of bending stresses in a cross-section - According to EN 1995-1 §6.1.6(2)")]
		public static double Km(ICrossSection crossSection, IMaterial material)
		{
			//km -> Factor considering re-distribution of bending stresses in a cross-section
			double km = 1;

			if (crossSection is CrossSectionRectangular)
			{
				IMaterialTimber timber;
				if (material is IMaterialTimber)
				{
					timber = (IMaterialTimber)material;

					if (timber.Type == EC5_Utilities.TimberType.Softwood ||
						timber.Type == EC5_Utilities.TimberType.Hardwood ||
						timber.Type == EC5_Utilities.TimberType.Glulam ||
						timber.Type == EC5_Utilities.TimberType.LVL ||
						timber.Type == EC5_Utilities.TimberType.Baubuche)
					{
						km = 0.7;
					}
				}
			}

			return km;
		}
		#endregion


		#region Kc

		/// <summary>
		/// Conmputes and returns the buckling instability factors kcy and kcz as a list of doubles - According to EN 1995-1 Eq(6.27) + Eq(6.28)
		/// </summary>
		/// <param name="crossSection">Cross section object</param>
		/// <param name="material">Material object</param>
		/// <param name="Leff_Y">Buckling length along Y in mm</param>
		/// <param name="Leff_Z">Buckling Length along Z in mm</param>
		/// <param name="FireCheck">if the factor is defined for a fire check, the material properties are increased by the factor Kfi</param>
		/// <returns>Returns the buckling instability factors kcy and kcz as a list of doubles </double></returns>
		/// <exception cref="Exception"></exception>
		[Description("Conmputes and returns the buckling instability factors kcy and kcz as a list of doubles - According to EN 1995-1 Eq(6.27) + Eq(6.28)")]
		public static List<double> Kc(ICrossSection crossSection, IMaterial material, double Leff_Y, double Leff_Z, bool FireCheck)
		{

			if (!(crossSection is CrossSectionRectangular)) throw new Exception("The buckling Factor Kc is currently only implemented for rectangular cross section");
			CrossSectionRectangular RectCS = (CrossSectionRectangular)crossSection;
			if (!(material is IMaterialTimber)) throw new Exception("The buckling Factor Kc is currently only implemented for IMaterialTimber");
			IMaterialTimber timber = (IMaterialTimber)material;

			double kfi = 1;
			if (FireCheck)
			{
				kfi = EC5_Factors.Kfi((IMaterialTimber)material);
			}


			double iy = RectCS.H / Math.Sqrt(12);
			double iz = RectCS.B / Math.Sqrt(12);

			double lambdaY = Leff_Y / iy;
			double lambdaZ = Leff_Z / iz;

			double lambdaRelY = lambdaY / Math.PI * Math.Sqrt(timber.Fc0k * kfi / (timber.E0_005 * kfi));
			double lambdaRelZ = lambdaZ / Math.PI * Math.Sqrt(timber.Fc0k * kfi / (timber.E0_005 * kfi));


			double Bc = 0;
			switch (timber.Type)
			{
				case TimberType.Softwood:
					Bc = 0.2;
					break;
				case TimberType.Hardwood:
					Bc = 0.2;
					break;
				case TimberType.Glulam:
					Bc = 0.1;
					break;
				case TimberType.LVL:
					Bc = 0.1;
					break;
				case TimberType.Baubuche:
					Bc = 0.1;
					break;
			}


			double Ky = 0.5 * (1 + Bc * (lambdaRelY - 0.3) + Math.Pow(lambdaRelY, 2));
			double Kz = 0.5 * (1 + Bc * (lambdaRelZ - 0.3) + Math.Pow(lambdaRelZ, 2));

			double Kcy = 1 / (Ky + Math.Sqrt(Math.Pow(Ky, 2) - Math.Pow(lambdaRelY, 2)));
			double Kcz = 1 / (Kz + Math.Sqrt(Math.Pow(Kz, 2) - Math.Pow(lambdaRelZ, 2)));

			Kcy = Math.Min(Kcy, 1);
			Kcz = Math.Min(Kcz, 1);

			return new List<double>() { Kcy, Kcz };
		}

		#endregion


		#region Kcrit
		/// <summary>
		/// factor which takes into account the reduced bending strength due to lateral buckling according to EN 1995-1 Eq(6.34)
		/// </summary>
		/// <param name="material">Material object</param>
		/// <param name="crossSection">Cross section object</param>
		/// <param name="Leff">Lateral buckling effective length in mm</param>
		/// <returns>return the Kcrit factor</returns>
		/// <exception cref="Exception"></exception>
		public static double Kcrit(IMaterial material, ICrossSection crossSection, double Leff, bool FireCheck)
		{
			if (!(material is IMaterialTimber)) throw new Exception("Kcrit can only be calculated fot timber material");
			var timber = (IMaterialTimber)material;

			double kcrit = 1;
			//Fire factors
			double kfi = 1;
			if (FireCheck)
			{
				kfi = EC5_Factors.Kfi((IMaterialTimber)material);
			}



			double SigmaCrit = 0;

			//According to DIN EN 1995-1 NA §6.3.3(2), for Glulam, characteristic elastic properties can be increased by a factor 1.4
			if (timber.Type == TimberType.Glulam) SigmaCrit = Math.PI * Math.Sqrt(1.4 * kfi * timber.E0_005 * crossSection.MomentOfInertia_Z * timber.G0_005 * crossSection.TorsionalInertia) / (Leff * crossSection.SectionModulus_Y);

			//According to Pollmeier Design Guide 2019 P.18: "For beams made of BauBuche GL75 the product of the 5% ­quantile of the stifness variables
			//E0,05 · G0,05 may be multiplied by the factor 1.2.
			else if (timber.Type == TimberType.Baubuche) SigmaCrit = Math.PI * Math.Sqrt(1.2 * kfi * timber.E0_005 * crossSection.MomentOfInertia_Z * timber.G0_005 * crossSection.TorsionalInertia) / (Leff * crossSection.SectionModulus_Y);

			else SigmaCrit = Math.PI * Math.Sqrt(kfi * timber.E0_005 * crossSection.MomentOfInertia_Z * timber.G0_005 * crossSection.TorsionalInertia) / (Leff * crossSection.SectionModulus_Y);

			double lambdaRel_m = Math.Sqrt(timber.Fmyk / SigmaCrit);


			if (lambdaRel_m <= 0.75) kcrit = 1;
			else if (lambdaRel_m <= 1.4) kcrit = 1.56 - 0.75 * lambdaRel_m;
			else kcrit = 1 / Math.Pow(lambdaRel_m, 2);

			return kcrit;
		}

		#endregion


		#region Kc90
		/// <summary>
		/// kc,90 takes into consideration the type of effect, the splitting risk and the extent of the deformation
		/// </summary>
		/// <param name="material">Material object</param>
		/// <param name="supportType">0 for continuous support; 1 for ponctual contact according to EN 1995-1 §6.1.5</param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		[Description("kc,90 takes into consideration the type of effect, the splitting risk and the extent of the deformation")]
		public static double Kc90(IMaterial material, int supportType)
		{
			double kc90 = 1;
			var timber = CheckMaterialTimber(material);
			switch (timber.Type)
			{
				case TimberType.Softwood:
					if (supportType == 0) kc90 = 1.25;
					else kc90 = 1.5;
					break;

				case TimberType.Hardwood:
					if (supportType == 0) kc90 = 1.25;
					else kc90 = 1.5;
					break;

				case TimberType.Glulam:
					if (supportType == 0) kc90 = 1.5;
					else kc90 = 1.75;
					break;

				case TimberType.LVL:
					//To implement according to product i.e: CERTIFICATE NO. EUFI29-20000676-C/EN for Kerto
					kc90 = 1;
					break;

				case TimberType.Baubuche:
					kc90 = 1;
					break;

				default:
					kc90 = 1;
					break;
			}

			return kc90;

		}

		#endregion


		#region Km_Alpha
		/// <summary>
		/// Km,α is a reduction factor taking into account the increased stress due to tapper edges. The factor differs for edges in tension or compression
		/// depending on the bending orientation - EN 1995-1 §6.4.2 eq (6.39)
		/// </summary>
		/// <param name="material">Material object</param>
		/// <param name="AngleInDegree">ange in degree</param>
		/// <returns></returns>
		[Description("Km,α is a reduction factor taking into account the increased stress due to tapper edges")]
		public static double Km_Alpha_Tension(IMaterial material, double AngleInDegree)
		{
			double angleRadian = AngleInDegree * Math.PI / 180;
			var timber = CheckMaterialTimber(material);

			return 1 / (Math.Sqrt(1 + Math.Pow(timber.Fmyk / (0.75 * timber.Fvk) * Math.Tan(angleRadian), 2) + Math.Pow(timber.Fmyk / timber.Ft90k * Math.Pow(Math.Tan(angleRadian), 2), 2)));
		}

		/// <summary>
		/// Km,α is a reduction factor taking into account the increased stress due to tapper edges. The factor differs for edges in tension or compression
		/// depending on the bending orientation - EN 1995-1 §6.4.2 eq (6.40)
		/// </summary>
		/// <param name="material">Material object</param>
		/// <param name="AngleInDegree">ange in degree</param>
		/// <returns></returns>
		[Description("Km,α is a reduction factor taking into account the increased stress due to tapper edges")]
		public static double Km_Alpha_Compression(IMaterial material, double AngleInDegree)
		{
			double angleRadian = AngleInDegree * Math.PI / 180;
			var timber = CheckMaterialTimber(material);

			return 1 / (Math.Sqrt(1 + Math.Pow(timber.Fmyk / (1.5 * timber.Fvk) * Math.Tan(angleRadian), 2) + Math.Pow(timber.Fmyk / timber.Fc90k * Math.Pow(Math.Tan(angleRadian), 2), 2)));
		}

		#endregion


		#region Kr
		/// <summary>
		/// Factor taking into consideration the stresses generated prior to bonding due to the bending of individual lamellae for curved beams with small radii of curvature - EN 1995-1 §6.4.3 eq (6.49)
		/// </summary>
		/// <param name="internalRadius">internal beam radius in [mm]</param>
		/// <param name="lamellaThickness">glulam lamellae thickness in [mm]</param>
		/// <returns></returns>
		[Description("Factor taking into consideration the stresses generated prior to bonding due to the bending of individual lamellae for curved beams with small radii of curvature")]
		public static double Kr(double internalRadius, double lamellaThickness)
		{
			double kr = 1;
			if (internalRadius / lamellaThickness < 240) kr = 0.76 + 0.001 * internalRadius / lamellaThickness;
			return kr;
		}

		#endregion


		#region Kl
		/// <summary>
		/// Kl is a bending amplification factor taking into account the beam curvature,cut angle and height in the apex area - EN 1995-1 §6.4.3 eq (6.43)
		/// </summary>
		/// <param name="heightApex">Beam height at apex in [mm]</param>
		/// <param name="angleApex">cut angle at apex in degree</param>
		/// <param name="internalRadius">beam internal radius in [mm]</param>
		/// <returns></returns>
		[Description("Kl is a bending stress amplification factor taking into account the beam curvature,cut angle and height in the apex area")]
		public static double Kl(double heightApex, double angleApex, double internalRadius)
		{
			double angleRad = angleApex * Math.PI / 180;
			double r = internalRadius + heightApex / 2;
			double h_ap = heightApex;
			double k1 = 1 + 1.4 * angleRad + 5.4 * Math.Pow(Math.Tan(angleRad), 2);
			double k2 = 0.35 - 8 * Math.Tan(angleRad);
			double k3 = 0.6 + 8.3 * Math.Tan(angleRad) - 7.8 * Math.Pow(Math.Tan(angleRad), 2);
			double k4 = 6 * Math.Pow(Math.Tan(angleRad), 2);

			return k1 + k2 * (h_ap / r) + k3 * Math.Pow(h_ap / r, 2) + k4 * Math.Pow(h_ap / r, 3);
		}


		#endregion


		#region Kvol
		/// <summary>
		/// Volume factor taking into consideration the influence of volume on tensile strength perpendicular to the grain - EN 1995-1 §6.4.3 eq (6.51)
		/// </summary>
		/// <param name="material">Material Object</param>
		/// <param name="Vstressed">stressed volume of the apex zone in [m³]</param>
		/// <param name="Vtot">Total beam volume in [m³]</param>
		/// <returns></returns>
		[Description("Volume factor taking into consideration the influence of volume on tensile strength perpendicular to the grain - EN 1995-1 §6.4.3 eq (6.51)")]
		public static double Kvol(IMaterial material, double Vstressed, double Vtot)
		{
			var timber = CheckMaterialTimber(material);
			double kvol = 1;
			if (timber.Type != TimberType.Hardwood || timber.Type != TimberType.Softwood)
			{
				kvol = Math.Pow(0.01 / Vstressed, 0.2);
			}
			if (Vstressed > (2.0 / 3.0) * Vtot) throw new Exception("Stressed volumes should not exceed 2/3 of total volume according to EN 1995-1 §6.4.3 (6)");
			return kvol;
		}
		#endregion


		#region Kdis
		/// <summary>
		/// Factor taking into consideration the influence of stress distribution - EN 1995-1 §6.4.3 eq (6.52)
		/// </summary>
		/// <param name="beamType">0->double tapered and curved beams | 1-> for pitched cambered beams </param>
		/// <returns></returns>
		[Description("Factor taking into consideration the influence of stress distribution - EN 1995-1 §6.4.3 eq (6.52)")]
		public static double Kdis(int beamType)
		{
			if (beamType == 0) return 1.4;  //for double tapered and curved beams
			else return 1.7;                //for pitched cambered beams
		}
		#endregion


		#region Kp
		/// <summary>
		/// Factor for the verification of tension perpendicular to the grain at apex - EN 1995-1 §6.4.3 eq (6.56)
		/// </summary>
		/// <param name="heightApex">Beam height at apex in [mm]</param>
		/// <param name="angleApex">cut angle at apex in degree</param>
		/// <param name="internalRadius">beam internal radius in [mm]</param>
		/// <returns></returns>
		[Description("Factor for the verification of tension perpendicular to the grain at apex - EN 1995-1 §6.4.3 eq (6.56)")]
		public static double Kp(double heightApex, double angleApex, double internalRadius)
		{
			double angleRad = angleApex * Math.PI / 180;
			double r = internalRadius + heightApex / 2;
			double h_ap = heightApex;

			double k5 = 0.2 * Math.Tan(angleRad);
			double k6 = 0.25 - 1.5 * Math.Tan(angleRad) + 2.6 * Math.Pow(Math.Tan(angleRad), 2);
			double k7 = 2.1 * Math.Tan(angleRad) - 4 * Math.Pow(Math.Tan(angleRad), 2);

			return k5 + k6 * (h_ap / r) + k7 * Math.Pow(h_ap / r, 2);
		}


		#endregion

		#region Kfi
		/// <summary>
		/// Kfi is the coefficient to go from 5% to 20% characteristic fractile in case of fire design according to DIN EN 1995-1-2 Table 2.1
		/// </summary>
		/// <param name="timber">Material Object</param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static double Kfi(IMaterialTimber timber)
		{
			switch (timber.Type)
			{
				case TimberType.Softwood:
					return 1.25;
				case TimberType.Hardwood:
					return 1.25;
				case TimberType.Glulam:
					return 1.15;
				case TimberType.LVL:
					return 1.1;
				case TimberType.Baubuche:
					return 1.1;
				case TimberType.OSB:
					return 1.15;
				default:
					throw new Exception("Kfi for this material is not implemented");
			}
		}
		#endregion




	}
}


