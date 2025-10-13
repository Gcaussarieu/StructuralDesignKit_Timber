using StructuralDesignKitLibrary.Connections.Interface;
using StructuralDesignKitLibrary.Connections.SteelTimberShear;
using StructuralDesignKitLibrary.Connections.TimberTimberShear;
using StructuralDesignKitLibrary.EC5;
using StructuralDesignKitLibrary.EC5.Connections.Interface;
using StructuralDesignKitLibrary.Materials;
using StructuralDesignKitLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StructuralDesignKitLibrary.EC5.EC5_Utilities;

namespace StructuralDesignKitLibrary.Connections.Fasteners
{
	/// <summary>
	/// Nail fastener according to EN 1995-1-1 §8.3
	/// </summary>
	public class FastenerNail : IFastener
	{

		#region Properties
		public EC5_Utilities.FastenerType Type { get; }
		public NailTypes NailType { get; }
		public double Diameter { get; set; }
		public double DiamHead { get; set; }
		public double Fuk { get; set; }
		public double MyRk { get; set; }
		public double Fhk { get; set; }
		public double FaxRk { get; set; }
		public double MaxJohansenPart { get; set; }
		public double a1min { get; set; }
		public double a2min { get; set; }
		public double a3tmin { get; set; }
		public double a3cmin { get; set; }
		public double a4tmin { get; set; }
		public double a4cmin { get; set; }
		public double K90 { get; set; }
		public bool Predrilled { get; set; }
		public int Length { get; set; }
		public double FHeadK { get; set; }
		public double Fax { get; set; }
		public double ThreadLength { get; set; }
		public double NailTensileCapacity { get; set; }
		public double MinPenetrationLength { get; set; }
		public double MinMemberThickness { get; set; }


		#endregion


		#region constructor
		/// <summary>
		/// 
		/// </summary>
		/// <param name="nailType"></param>
		/// <param name="diameter"></param>
		/// <param name="length"></param>
		/// <param name="fuk"></param>
		/// <param name="predrilled"></param>
		/// <param name="myrk"></param>
		/// <param name="dHead"></param>
		/// <param name="fax"></param>
		/// <param name="fHead"></param>
		/// <param name="threadLength"></param>
		/// <param name="steelCapacity"></param>
		/// <exception cref="Exception"></exception>
		public FastenerNail(NailTypes nailType, double diameter, int length, double fuk, bool predrilled, double myrk = 0, double dHead = 0, double fax = 0, double fHead = 0, double threadLength = 0, double steelCapacity = 0)
		{
			Type = EC5_Utilities.FastenerType.Nail;
			if (diameter > 6 && !predrilled) throw new Exception("Nail with a diameter > 6mm needs to be predrilled");
			if (fuk < 600) throw new Exception("Fuk must ve above 600N/mm for nail wires");
			else Fuk = fuk;

			if (dHead > 0) DiamHead = dHead;
			//if not provided, the head diameter is considered to be 1.5x the shaft diameter
			else DiamHead = diameter * 1.5;

			Diameter = diameter;
			Predrilled = predrilled;
			Length = length;


			FHeadK = fHead;
			Fax = fax;
			ThreadLength = threadLength;
			NailTensileCapacity = steelCapacity;

			NailType = nailType;
			MinPenetrationLength = 6 * diameter;

			switch (NailType)
			{
				case NailTypes.Round:
					MaxJohansenPart = 0; //Conservatively, we set this value to 0, even if the EC5 allows 15%
					if (myrk == 0) MyRk = 0.3 * Fuk * Math.Pow(Diameter, 2.6); //EN 1995-1-1 Eq (8.14)
					else MyRk = myrk;

					break;
				case NailTypes.Squarred_Grooved:
					MaxJohansenPart = 0; //Conservatively, we set this value to 0, even if the EC5 allows 25%
					if (myrk == 0) MyRk = 0.45 * Fuk * Math.Pow(Diameter, 2.6); //EN 1995-1-1 Eq (8.14)
					else MyRk = myrk;

					break;
				case NailTypes.AnnularRing:
					MaxJohansenPart = 0.5; //Other nails according to EN 1995-1-1 §8.2.2 (2)
					if (myrk == 0) MyRk = 0.3 * Fuk * Math.Pow(Diameter, 2.6); //EN 1995-1-1 Eq (8.14)
					else MyRk = myrk;

					break;
				case NailTypes.Twisted:
					MaxJohansenPart = 0.5; //Other nails according to EN 1995-1-1 §8.2.2 (2)
					if (myrk == 0) MyRk = 0.3 * Fuk * Math.Pow(Diameter, 2.6); //EN 1995-1-1 Eq (8.14)
					else MyRk = myrk;

					break;

				default:
					break;
			}

		}
		#endregion

		//Spacings according to EN 1995-1-1 Table 8.4
		#region define spacings

		/// <summary>
		/// Define the minimum spacing to alongside the grain in mm
		/// </summary>
		/// <param name="angle">angle to grain in Degree</param>
		/// <returns></returns>
		[Description("Define the minimum spacing to alongside the grain in mm")]
		private double DefineA1Min(double? angle, double density)
		{
			if (angle == null)
			{
				if (Predrilled) return 5 * Diameter;
				else
				{
					if (density <= 420)
					{
                        if (Diameter < 5) return 10 * Diameter;
                        return 12 * Diameter;
                    }
                    if (density <= 500) return 15 * Diameter;
                    else throw new Exception("With characteristic Density > 500kg/m³, nails must be predrilled");
                }
			}

			double AngleRad = Convert.ToDouble(angle) * Math.PI / 180;

			if (Predrilled)
			{
				return (4 + Math.Abs(Math.Cos(AngleRad))) * Diameter;
			}
			else
			{
				if (density <= 420)
				{

					if (Diameter < 5) return (5 + 5 * Math.Abs(Math.Cos(AngleRad))) * Diameter;
					else return (5 + 7 * Math.Abs(Math.Cos(AngleRad))) * Diameter;
				}
				else if (density <= 500) return (7 + 8 * Math.Abs(Math.Cos(AngleRad))) * Diameter;
				else throw new Exception("With characteristic Density > 500kg/m³, nails must be predrilled");
			}

		}

		/// <summary>
		/// Define the minimum spacing perpendicular to grain in mm
		/// </summary>
		/// <param name="angle">angle to grain in Degree</param>
		/// <returns></returns>
		[Description("Define the minimum spacing perpendicular to grain in mm")]
		private double DefineA2Min(double? angle, double density)
		{
			if (angle == null)
			{
                if (Predrilled) return 4 * Diameter;
                if (density <= 420) return 5 * Diameter;
                if (density <= 500) return 7 * Diameter;
                else throw new Exception("With characteristic Density > 500kg/m³, nails must be predrilled");
            }

            double AngleRad = Convert.ToDouble(angle) * Math.PI / 180;

			if (Predrilled)
			{
				return (3 + Math.Sin(AngleRad)) * Diameter;
			}
			else
			{
				if (density <= 420)
				{

					return 5 * Diameter;
				}
				else if (density <= 500) return 7 * Diameter;
				else throw new Exception("With characteristic Density > 500kg/m³, nails must be predrilled");
			}
		}


		/// <summary>
		/// Define the Minimum spacing to loaded end in mm
		/// </summary>
		/// <param name="angle">angle to grain in Degree</param>
		/// <returns></returns>
		[Description("Define the Minimum spacing to loaded end in mm")]
		private double DefineA3tMin(double? angle, double density)
		{
			if (angle == null)
			{
                if (Predrilled) return (7 + 5) * Diameter;
                if (density <= 420) return (10 + 5) * Diameter;
                if (density <= 500) return (15 + 5) * Diameter;
                else throw new Exception("With characteristic Density > 500kg/m³, nails must be predrilled");
            }

			double AngleRad = Convert.ToDouble(angle) * Math.PI / 180;

			if (Predrilled)
			{
				return (7 + 5 * Math.Abs(Math.Cos(AngleRad))) * Diameter;
			}
			else
			{
				if (density <= 420)
				{

					return (10 + 5 * Math.Abs(Math.Cos(AngleRad))) * Diameter;

				}
				else if (density <= 500) return (15 + 5 * Math.Abs(Math.Cos(AngleRad))) * Diameter;
				else throw new Exception("With characteristic Density > 500kg/m³, nails must be predrilled");
			}
		}

		/// <summary>
		/// Define the Minimum spacing to unloaded end in mm
		/// </summary>
		/// <param name="angle">angle to grain in Degree</param>
		/// <returns></returns>
		[Description("Define the Minimum spacing to unloaded end in mm")]
		private double DefineA3cMin(double? angle, double density)
		{
			if (Predrilled)
			{
				return (7 * Diameter);
			}
			else
			{
				if (density <= 420)
				{

					return 10 * Diameter;
				}
				else if (density <= 500) return 15 * Diameter;
				else throw new Exception("With characteristic Density > 500kg/m³, nails must be predrilled");
			}
		}

		/// <summary>
		/// Define the Minimum spacing to loaded edge in mm
		/// </summary>
		/// <param name="angle">angle to grain in Degree</param>
		/// <returns></returns>
		[Description("Define the Minimum spacing to loaded edge in mm")]
		private double DefineA4tMin(double? angle, double density)
		{
            if (angle == null)
            {
                if (Predrilled)
                {
                    if (Diameter < 5) return (3 + 2) * Diameter;
                    return (3 + 4) * Diameter;
                }
                if (density <= 420)
                {
                    if (Diameter < 5) return (5 + 2) * Diameter;
                    return (5 + 5) * Diameter;
                }
                if (density <= 500)
                {
                    if (Diameter < 5) return (7 + 2) * Diameter;
                    return (7 + 5) * Diameter;
                }
                else throw new Exception("With characteristic Density > 500kg/m³, nails must be predrilled");
            }

            double AngleRad = Convert.ToDouble(angle) * Math.PI / 180;

			if (Predrilled)
			{
				if (Diameter < 5) return (3 + 2 * Math.Abs(Math.Sin(AngleRad))) * Diameter;
				else return (3 + 4 * Math.Abs(Math.Sin(AngleRad))) * Diameter;
			}
			else
			{
				if (density <= 420)
				{

					if (Diameter < 5) return (5 + 2 * Math.Abs(Math.Sin(AngleRad))) * Diameter;
					else return (5 + 5 * Math.Abs(Math.Sin(AngleRad))) * Diameter;
				}
				else if (density <= 500)
				{
					if (Diameter < 5) return (7 + 2 * Math.Abs(Math.Sin(AngleRad))) * Diameter;
					else return (7 + 5 * Math.Abs(Math.Sin(AngleRad))) * Diameter;
				}
				else throw new Exception("With characteristic Density > 500kg/m³, nails must be predrilled");
			}


		}

		/// <summary>
		/// Define the minimum spacing to unloaded edge in mm
		/// </summary>
		/// <returns></returns>
		[Description("Define the minimum spacing to unloaded edge in mm")]
		private double DefineA4cMin(double? angle, double density)
		{
			if (Predrilled)
			{
				return 3 * Diameter;

			}
			else
			{
				if (density <= 420)
				{

					return 5 * Diameter;

				}
				else if (density <= 500)
				{
					return 7 * Diameter;
				}
				else throw new Exception("With characteristic Density > 500kg/m³, nails must be predrilled");
			}
		}

		#endregion


		#region Define minimum penetration length

		public void ComputeMinimumMinMemberThickness(double rhoK, bool SpecieSensitiveToSplitting)
		{
			List<double> memberThickness = new List<double>();


			if (!SpecieSensitiveToSplitting)
			{
				if (!Predrilled)
				{
					memberThickness.Add(7 * Diameter); //EN 1995-1-1 §8.3.1.2 (6)
					memberThickness.Add((13 * Diameter - 30) * (rhoK / 400));
				}
				else memberThickness.Add(6 * Diameter); //EN 1995-1-1 §8.3.1.2 (2)
			}
			else
			{
				if (!Predrilled)
				{
					memberThickness.Add(14 * Diameter); //EN 1995-1-1 §8.3.1.2 (7)
					memberThickness.Add((13 * Diameter - 30) * (rhoK / 200));
				}
				else memberThickness.Add(6 * Diameter); //EN 1995-1-1 §8.3.1.2 (2)
			}


			MinMemberThickness = memberThickness.Max();

		}



		#endregion


		public void ComputeEmbedmentStrength(IMaterialTimber timber, double angle, double thickness = 0)
		{
			//LVL and Baubuche need special consideration with nails which are not yet implemented (03/01/2025)
			List<string> CoveredTimber = new List<string>() { "Softwood", "Hardwood", "Glulam", "OSB" };
			if (CoveredTimber.Contains(timber.Type.ToString()))
			{


				if (timber.Type == TimberType.Softwood || timber.Type == TimberType.Hardwood || timber.Type == TimberType.Glulam)
				{
					if (Diameter < 8)
					{
						if (Predrilled)
						{
							Fhk = 0.082 * (1 - 0.01 * Diameter) * timber.RhoK;
						}
						else
						{
							if (timber.RhoK > 500) throw new Exception("Nailed connection in wood with a characteristic density > 500kg/m³ needs to be pre-drilled");
							Fhk = 0.082 * timber.RhoK * Math.Pow(Diameter, -0.3);
						}
					}
					else
					{
						FastenerBolt EquivalentBolt = new FastenerBolt(Diameter, Fuk);
						EquivalentBolt.ComputeEmbedmentStrength(timber, angle);
						Fhk = EquivalentBolt.Fhk;

					}


				}
				else if (timber.Type == TimberType.OSB)
				{
					if (Predrilled)
					{
						//DIN EN 1995-1-1 (NA.17) eq (NA.127)
						Fhk = 50 * Math.Pow(Diameter, -0.6) * Math.Pow(thickness, 0.2);
					}
					else
					{
						//EN 1995-1-1 §8.3.1.3 (3) Eq 8.22

						Fhk = 65 * Math.Pow(Diameter, -0.7) * Math.Pow(thickness, 0.1);
					}
				}


			}
			else throw new Exception("Timber type not yet covered in the SDK");

		}

		/// <summary>
		/// Nail withdrawal capacity according to EN 1995-1-1 §8.3.3
		/// </summary>
		/// <param name="ConnectionType">Type of connection to consider</param>
		/// <param name="fax"></param>
		/// <param name="fHead"></param>
		/// <exception cref="Exception"></exception>
		public void ComputeWithdrawalStrength(IShearCapacity ConnectionType)
		{
			List<double> WithdrawalCapacity = new List<double>();

			if (NailTensileCapacity == 0) WithdrawalCapacity.Add(ComputeNailSteelTensileStrength());
			else WithdrawalCapacity.Add(NailTensileCapacity);




			double faxRk2 = FHeadK * Math.Pow(DiamHead, 2);

			double tPen;



			if (ConnectionType is TimberTimberSingleShear)
			{
				TimberTimberSingleShear connection = ConnectionType as TimberTimberSingleShear;
				if (connection.T2 > ThreadLength) tPen = ThreadLength;
				else tPen = connection.T2;

				if (tPen < 6 * Diameter) throw new Exception("The penetration length should be at least 6d");

				if (tPen < 8 * Diameter) WithdrawalCapacity.Add(0);
				else if (tPen >= 8 * Diameter && tPen < 12 * Diameter)
				{
					WithdrawalCapacity.Add(Fax * Diameter * tPen * (tPen / (2 * Diameter) - 3));
					WithdrawalCapacity.Add(faxRk2);
				}
				else
				{
					WithdrawalCapacity.Add(Fax * Diameter * tPen);
					WithdrawalCapacity.Add(faxRk2);
				}

			}

			else if (ConnectionType is TimberTimberDoubleShear)
			{
				TimberTimberDoubleShear connection = ConnectionType as TimberTimberDoubleShear;
				if (connection.T1 > ThreadLength) tPen = ThreadLength;
				else tPen = connection.T1;
				if (tPen < 6 * Diameter) throw new Exception("The penetration length should be at least 6d");

				if (tPen < 8 * Diameter) WithdrawalCapacity.Add(0);
				else if (tPen >= 8 * Diameter && tPen < 12 * Diameter)
				{
					WithdrawalCapacity.Add(Fax * Diameter * tPen * (tPen / (2 * Diameter) - 3));
					WithdrawalCapacity.Add(faxRk2);
				}
				else
				{
					WithdrawalCapacity.Add(Fax * Diameter * tPen);
					WithdrawalCapacity.Add(faxRk2);
				}

			}
		

			else if (ConnectionType is SingleOuterSteelPlate)
			{
				SingleOuterSteelPlate connection = ConnectionType as SingleOuterSteelPlate;
				if (connection.TimberThickness > ThreadLength) tPen = ThreadLength;
				else tPen = connection.TimberThickness;

				if (tPen < 6 * Diameter) throw new Exception("The penetration length should be at least 6d");

				if (tPen < 8 * Diameter) WithdrawalCapacity.Add(0);
				else if (tPen >= 8 * Diameter && tPen < 12 * Diameter)
				{
					WithdrawalCapacity.Add(Fax * Diameter * tPen * (tPen / (2 * Diameter) - 3));
				}
				else
				{
					WithdrawalCapacity.Add(Fax * Diameter * tPen);
				}
			}

			else throw new Exception("Bolt Withdrawal capacity not yet implemented");

			FaxRk = WithdrawalCapacity.Min();

		}

		/// <summary>
		/// Compute the tensile strength of the nail
		/// </summary>
		/// <returns></returns>
		private double ComputeNailSteelTensileStrength()
		{
			//Simple addition to create a upper limit for steel
			double aeff = Math.PI * Math.Pow(Diameter, 2) / 4;
			return 0.9 * aeff * Fuk / 1.25; //consider k2 = 0.9 and Ym2 = 1.25
		}

		public double ComputeEffectiveNumberOfFastener(int n, double a1, double angle)
		{
			if (n == 1) return 1;
			else
			{

				double kef = 0;
				if (a1 < 4 * Diameter) throw new Exception("for nails, minimum a1 distance is 4d for predrilled holes and 7d otherwise");
				else if (a1 >= 4 * Diameter && a1 < 7 * Diameter)
				{
					if (Predrilled)
					{
						double xd = a1 / Diameter;
						kef = SDKUtilities.LinearInterpolation(xd, 4, 0.5, 7, 0.7);
					}
					else throw new Exception("for nails, minimum a1 distance is 4d for predrilled holes and 7d otherwise");
				}
				else if (a1 >= 7 * Diameter && a1 < 10 * Diameter)
				{
					double xd = a1 / Diameter;

					kef = SDKUtilities.LinearInterpolation(xd, 7, 0.7, 10, 0.85);
				}
				else if (a1 >= 10 * Diameter && a1 < 14 * Diameter)
				{
					double xd = a1 / Diameter;

					kef = SDKUtilities.LinearInterpolation(xd, 10, 0.85, 14, 1);
				}
				else kef = 1;

				double nef_0 = Math.Pow(n, kef);

				int angleFirstQuadrant = SDKUtilities.ComputeAngleToFirstQuadrant(angle);
				return SDKUtilities.LinearInterpolation(Convert.ToDouble(angleFirstQuadrant), 0, nef_0, 90, n);
			}
		}

		public void ComputeSpacings(double? angle, IShearCapacity connection = null)
		{

			double density = 0;

			if (connection is null) throw new Exception("To compute the spacings for nails, the connection type must be provided");
			else
			{
				if (connection is TimberTimberDoubleShear || connection is TimberTimberSingleShear)
				{
					var CastConnection = connection as ITimberTimberShear;


					//if the timber density comes from an OSB panel, it is not considered.
					double density1 = CastConnection.Timber1.RhoK;
					if (CastConnection.Timber1.Type == TimberType.OSB) density1 = 0;

					double density2 = CastConnection.Timber2.RhoK;
					if (CastConnection.Timber2.Type == TimberType.OSB) density2 = 0;

					density = Math.Max(density1, density2);
				}
				else if (connection is SingleOuterSteelPlate)
				{
					var CastConnection = connection as ISteelTimberShear;
					if (CastConnection.Timber.Type == TimberType.OSB) throw new Exception("Spacing for steel to OSB needs to be discussed - NOT IMPLEMENTED YET");

					density = CastConnection.Timber.RhoK;
				}
				else throw new Exception("The connection type is not yet implemented in the FastenerNail object");

			}



			a1min = DefineA1Min(angle, density);
			a2min = DefineA2Min(angle, density);
			a3tmin = DefineA3tMin(angle, density);
			a3cmin = DefineA3cMin(angle, density);
			a4tmin = DefineA4tMin(angle, density);
			a4cmin = DefineA4cMin(angle, density);



			if (connection is SingleOuterSteelPlate)
			{
				//According to EN 1995-1-1 §8.3.1.4 (1), spacing multiplied by 0.7
				a1min *= 0.7;
				a2min *= 0.7;
				a3tmin *= 0.7;
				a3cmin *= 0.7;
				a4tmin *= 0.7;
				a4cmin *= 0.7;
			}


			if (connection is TimberTimberDoubleShear || connection is TimberTimberSingleShear)
			{
				var CastConnection = connection as ITimberTimberShear;
				if (CastConnection.Timber1.Type == TimberType.OSB || CastConnection.Timber2.Type == TimberType.OSB)
				{
					//According to DIN EN 1995-1-1 (NA.13) -> for OSB plates, the edge distances can be reduced to 7d for loaded edge and 3d for unloaded edge
					//Nail spacing can be factored with 0.85 according to EN 1995-1-1 §8.3.1.3 (1)

					a1min *= 0.85;
					a2min *= 0.85;
					a3tmin = 7 * Diameter;
					a3cmin = 3 * Diameter;
					a4tmin = 7 * Diameter;
					a4cmin = 3 * Diameter;
				}
			}




		}

		public double ComputeSlipModulus(double timberDensity)
		{
			if (this.Predrilled)
			{
				return Math.Pow(timberDensity, 1.5) * this.Diameter / 23;
			}
			else
			{
				return Math.Pow(timberDensity, 1.5) * Math.Pow(this.Diameter, 0.8) / 30;
			}
		}

		public enum NailTypes
		{
			Round,
			Squarred_Grooved,
			AnnularRing,
			Twisted,
		}





		///
		///
		///Brand
		///Name
		///Type
		///OuterDiam
		///ShankDiam
		///Fuk
		///Myrk
		///HeadDiam
		///Table fHead
		///TimberType -> Fhead  / min thick max thick / min Rhok, max RhoK
		///Table Fax
		///length
		///length thread
		///ETA
		///
		///
		///
		///
		///
		///
		///
		///
		///
		///
		///
		///
		///





	}
}
