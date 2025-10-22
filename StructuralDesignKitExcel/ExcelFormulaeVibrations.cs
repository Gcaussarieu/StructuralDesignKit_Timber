using ExcelDna.Integration;
using ExcelDna.Registration;
using Microsoft.Office.Interop.Excel;
using StructuralDesignKitLibrary.EC5;
using StructuralDesignKitLibrary.Materials;
using StructuralDesignKitLibrary.Vibrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StructuralDesignKitExcel
{
	public static partial class ExcelFormulae
	{


		/// <summary>
		/// Compute the Resonant Response Analysis for Low frequency floors for a given point with multiple vibration modes (RMS acceleration of the response)
		/// The input is formated for RFEM 6
		/// </summary>
		/// <param name="Modes">Range of modes considered</param>
		/// <param name="Frequencies">Range of frequencies considered</param>
		/// <param name="ModalMasses">Range of modal masses considered</param>
		/// <param name="StandardizedDisplacements">Data for the standardized displacement according to the documentation</param>
		/// <param name="fp">Pace frequency in [Hz]</param>
		/// <param name="Xi">Damping as ratio to critical damping</param>
		/// <param name="weightingType">Weighting category for human perception of vibrations</param>
		/// <param name="SurfaceIDExcitation">Surface on which the node for the excitation to consider is situated</param>
		/// <param name="ExcitationNode">Node to consider for the excitation</param>
		/// <param name="SurfaceIDResponse">Surface on which the node for the response to consider is situated</param>
		/// <param name="ResponseNode">Node to consider for the excitation</param>
		/// <param name="walkingLength">Length of the walking path, if negative, the Eurocode resonant build up factor is considered</param>
		/// <param name="ResponseFactor">If true, provide the Response factor instead of the acceleration</param>
		/// <returns></returns>
		[ExcelFunction(Description = "Compute the Resonant Response Analysis for Low frequency floors for a given point with multiple vibration modes (RMS acceleration of the response)",
			Name = "SDK.Vibration.AccelerationResponse",
			IsHidden = false,
			Category = "SDK.Vibration")]
		public static double AccelerationResponse(
			[ExcelArgument(Description = "Range of modes considered")] object[,] Modes,
			[ExcelArgument(Description = "Range of frequencies considered")] object[,] Frequencies,
			[ExcelArgument(Description = "Range of modal masses considered")] object[,] ModalMasses,
			[ExcelArgument(Description = "Data for the standardized displacement according to the documentation")] object[,] StandardizedDisplacements,
			[ExcelArgument(Description = "Pace frequency in [Hz]")] double fp,
			[ExcelArgument(Description = "Damping as ratio to critical damping")] double Xi,
			[ExcelArgument(Description = "Weighting category for human perception of vibrations")] string weightingType,
			[ExcelArgument(Description = "Surface on which the node for the excitation to consider is situated")] int SurfaceIDExcitation,
			[ExcelArgument(Description = "Node to consider for the excitation")] int ExcitationNode,
			[ExcelArgument(Description = "Surface on which the node for the response to consider is situated")] int SurfaceIDResponse,
			[ExcelArgument(Description = "Node to consider for the response")] int ResponseNode,
			[ExcelArgument(Description = "Length of the walking path, if negative, the Eurocode resonant build up factor is considered")] double walkingLength,
			[ExcelArgument(Description = "If true, provide the Response factor instead of the acceleration")] bool ResponseFactor)
		{
			double accelerationResponse = 0;
			try
			{
				//ensure all input lists have the same length
				int listLength = Modes.Length;
				if (Frequencies.Length != listLength || ModalMasses.Length != listLength) throw new ArgumentException("all list should have the same length");

				//range to lists
				List<int> modes = new List<int>();
				List<double> frequencies = new List<double>();
				List<double> modalMasses = new List<double>();


				for (int i = 0; i < Modes.Length; i++)
				{
					modes.Add(Convert.ToInt32(Modes[i, 0]));
					frequencies.Add(Convert.ToDouble(Frequencies[i, 0]));
					modalMasses.Add(Convert.ToDouble(ModalMasses[i, 0]));
				}

				var data = FormatExcelRFEM6Data(modes, StandardizedDisplacements);

				//Get the two nodes selected for the analysis
				VibrationResultNode excitationResultNode = data.Where(p => p.SurfaceId == SurfaceIDExcitation).Where(p => p.PointID == ExcitationNode).First();
				VibrationResultNode responseResultNode = data.Where(p => p.SurfaceId == SurfaceIDResponse).Where(p => p.PointID == ResponseNode).First();

				accelerationResponse = Vibrations.ResonantResponseAnalysis(
					excitationResultNode.NormalisedDisplacements.Values.ElementAt(0)[2],
					responseResultNode.NormalisedDisplacements.Values.ElementAt(0)[2],
					frequencies, modalMasses, fp, Xi, Vibrations.GetWeighting(weightingType), walkingLength, ResponseFactor);
			}
			catch (Exception e)
			{

				MessageBox.Show(e.Message);
			}

			return accelerationResponse;
		}


		/// <summary>
		/// Compute the Transient Response Analysis for high frequency floors for a given point with multiple vibration modes (RMS velocity of the response)
		/// The input is formated for RFEM 6
		/// </summary>
		/// <param name="Modes">Range of modes considered</param>
		/// <param name="Frequencies">Range of frequencies considered</param>
		/// <param name="ModalMasses">Range of modal masses considered</param>
		/// <param name="StandardizedDisplacements">Data for the standardized displacement according to the documentation</param>
		/// <param name="fp">Pace frequency in [Hz]</param>
		/// <param name="Xi">Damping as ratio to critical damping</param>
		/// <param name="SurfaceIDExcitation">Surface on which the node for the excitation to consider is situated</param>
		/// <param name="ExcitationNode">Node to consider for the excitation</param>
		/// <param name="SurfaceIDResponse">Surface on which the node for the response to consider is situated</param>
		/// <param name="ResponseNode">Node to consider for the excitation</param>
		/// <param name="ResponseFactor">If true, provide the Response factor instead of the acceleration</param>
		/// <returns></returns>
		[ExcelFunction(Description = "Provide the velocity response for a given mode and timestep",
			Name = "SDK.Vibration.VelocitySeries",
			IsHidden = false,
			Category = "SDK.Vibration")]
		public static object VelocityResponseVelocitySeries(
			[ExcelArgument(Description = "Range of modes considered")] object[,] Modes,
			[ExcelArgument(Description = "Range of frequencies considered")] object[,] Frequencies,
			[ExcelArgument(Description = "Range of modal masses considered")] object[,] ModalMasses,
			[ExcelArgument(Description = "Data for the standardized displacement according to the documentation")] object[,] StandardizedDisplacements,
			[ExcelArgument(Description = "Pace frequency in [Hz]")] double fp,
			[ExcelArgument(Description = "Damping as ratio to critical damping")] double Xi,
			[ExcelArgument(Description = "Surface on which the node for the excitation to consider is situated")] int SurfaceIDExcitation,
			[ExcelArgument(Description = "Node to consider for the excitation")] int ExcitationNode,
			[ExcelArgument(Description = "Surface on which the node for the response to consider is situated")] int SurfaceIDResponse,
			[ExcelArgument(Description = "Node to consider for the response")] int ResponseNode,
			[ExcelArgument(Description = "If true, provide the Response factor instead of the acceleration")] bool ResponseFactor,
			[ExcelArgument(Description = "Natural mode to consider")] int mode,
			[ExcelArgument(Description = "time step to consider")] int step,
			[ExcelArgument(Description = "the resolution in second of the analysis - default value is 0.00125s")] double timeStep = 0.00125d)
		{
			double VelocityResponse = 0;
			try
			{
				if (step > (1.0 / fp) / timeStep) return ExcelEmpty.Value;
				else
				{


					//ensure all input lists have the same length
					int listLength = Modes.Length;
					if (Frequencies.Length != listLength || ModalMasses.Length != listLength) throw new ArgumentException("all list should have the same length");

					//range to lists
					List<int> modes = new List<int>();
					List<double> frequencies = new List<double>();
					List<double> modalMasses = new List<double>();


					for (int i = 0; i < Modes.Length; i++)
					{
						modes.Add(Convert.ToInt32(Modes[i, 0]));
						frequencies.Add(Convert.ToDouble(Frequencies[i, 0]));
						modalMasses.Add(Convert.ToDouble(ModalMasses[i, 0]));
					}

					var data = FormatExcelRFEM6Data(modes, StandardizedDisplacements);

					//Get the two nodes selected for the analysis
					VibrationResultNode excitationResultNode = data.Where(p => p.SurfaceId == SurfaceIDExcitation).Where(p => p.PointID == ExcitationNode).First();
					VibrationResultNode responseResultNode = data.Where(p => p.SurfaceId == SurfaceIDResponse).Where(p => p.PointID == ResponseNode).First();


					List<double> velocitySeries = new List<double>();


					if (mode >= 1 && mode <= frequencies.Count)
					{
						double Imod_ef = Vibrations.ComputeEffectiveFootfallImpulseEC5(fp, frequencies[mode - 1]);
						return Vibrations.VelocityResponse(
										excitationResultNode.NormalisedDisplacements.Values.ElementAt(0)[2][mode - 1],
										responseResultNode.NormalisedDisplacements.Values.ElementAt(0)[2][mode - 1],
										frequencies[mode - 1],
										fp,
										Imod_ef,
										Xi,
										step * timeStep,
										modalMasses[mode - 1]
										);

					}

					else if (mode == -1)
					{
						double v = 0;
						for (int i = 0; i < frequencies.Count; i++)
						{
							double Imod_ef = Vibrations.ComputeEffectiveFootfallImpulseEC5(fp, frequencies[i]);

							v += Vibrations.VelocityResponse(
									excitationResultNode.NormalisedDisplacements.Values.ElementAt(0)[2][i],
									responseResultNode.NormalisedDisplacements.Values.ElementAt(0)[2][i],
									frequencies[i],
									fp,
									Imod_ef,
									Xi,
									step * timeStep,
									modalMasses[i]
									);
						}
						return v;
					}

					else throw new Exception("The mode provided is not coherent");
				}
			}
			catch (Exception e)
			{

				MessageBox.Show(e.Message);
			}

			return -1;
		}

		/// <summary>
		/// Compute the Transient Response Analysis for high frequency floors for a given point with multiple vibration modes (RMS velocity of the response)
		/// The input is formated for RFEM 6
		/// </summary>
		/// <param name="Modes">Range of modes considered</param>
		/// <param name="Frequencies">Range of frequencies considered</param>
		/// <param name="ModalMasses">Range of modal masses considered</param>
		/// <param name="StandardizedDisplacements">Data for the standardized displacement according to the documentation</param>
		/// <param name="fp">Pace frequency in [Hz]</param>
		/// <param name="Xi">Damping as ratio to critical damping</param>
		/// <param name="SurfaceIDExcitation">Surface on which the node for the excitation to consider is situated</param>
		/// <param name="ExcitationNode">Node to consider for the excitation</param>
		/// <param name="SurfaceIDResponse">Surface on which the node for the response to consider is situated</param>
		/// <param name="ResponseNode">Node to consider for the excitation</param>
		/// <param name="ResponseFactor">If true, provide the Response factor instead of the acceleration</param>
		/// <param name="timeStep">the resolution in second of the analysis - default value is 0.00125s</param>
		/// <returns></returns>
		[ExcelFunction(Description = "Compute the Transient Response Analysis for high frequency floors for a given point with multiple vibration modes (RMS velocity of the response)",
			Name = "SDK.Vibration.VelocityResponse",
			IsHidden = false,
			Category = "SDK.Vibration")]
		public static double VelocityResponse(
			[ExcelArgument(Description = "Range of modes considered")] object[,] Modes,
			[ExcelArgument(Description = "Range of frequencies considered")] object[,] Frequencies,
			[ExcelArgument(Description = "Range of modal masses considered")] object[,] ModalMasses,
			[ExcelArgument(Description = "Data for the standardized displacement according to the documentation")] object[,] StandardizedDisplacements,
			[ExcelArgument(Description = "Pace frequency in [Hz]")] double fp,
			[ExcelArgument(Description = "Damping as ratio to critical damping")] double Xi,
			[ExcelArgument(Description = "Surface on which the node for the excitation to consider is situated")] int SurfaceIDExcitation,
			[ExcelArgument(Description = "Node to consider for the excitation")] int ExcitationNode,
			[ExcelArgument(Description = "Surface on which the node for the response to consider is situated")] int SurfaceIDResponse,
			[ExcelArgument(Description = "Node to consider for the response")] int ResponseNode,
			[ExcelArgument(Description = "If true, provide the Response factor instead of the acceleration")] bool ResponseFactor,
			[ExcelArgument(Description = "the resolution in second of the analysis - default value is 0.00125s")] double timeStep = 0.00125d)
		{
			double VelocityResponse = 0;
			try
			{
				//ensure all input lists have the same length
				int listLength = Modes.Length;
				if (Frequencies.Length != listLength || ModalMasses.Length != listLength) throw new ArgumentException("all list should have the same length");

				//range to lists
				List<int> modes = new List<int>();
				List<double> frequencies = new List<double>();
				List<double> modalMasses = new List<double>();


				for (int i = 0; i < Modes.Length; i++)
				{
					modes.Add(Convert.ToInt32(Modes[i, 0]));
					frequencies.Add(Convert.ToDouble(Frequencies[i, 0]));
					modalMasses.Add(Convert.ToDouble(ModalMasses[i, 0]));
				}

				var data = FormatExcelRFEM6Data(modes, StandardizedDisplacements);

				//Get the two nodes selected for the analysis
				VibrationResultNode excitationResultNode = data.Where(p => p.SurfaceId == SurfaceIDExcitation).Where(p => p.PointID == ExcitationNode).First();
				VibrationResultNode responseResultNode = data.Where(p => p.SurfaceId == SurfaceIDResponse).Where(p => p.PointID == ResponseNode).First();

				VelocityResponse = Vibrations.TransientResponseAnalysis(
					excitationResultNode.NormalisedDisplacements.Values.ElementAt(0)[2],
					responseResultNode.NormalisedDisplacements.Values.ElementAt(0)[2],
					frequencies, modalMasses, fp, Xi, ResponseFactor, timeStep);
			}
			catch (Exception e)
			{

				MessageBox.Show(e.Message);
			}

			return VelocityResponse;
		}


		/// <summary>
		/// Compute the Resonant Response Analysis for Low frequency floors for a given point with multiple vibration modes (RMS acceleration of the response)
		/// </summary>
		/// <param name="List_uen">List of the mode shape amplitudes, from the unity or mass normalised FE output, at the point on the floor where the excitation force Fh is applied</param>
		/// <param name="List_urn">List of the mode shape amplitudes, from the unity or mass normalised FE output, at the point where the response is to be calculated</param>
		/// <param name="NaturalFrequencies">List of natural frequencies for the mode considered</param>
		/// <param name="List_Mg">List of the is the modal mass for the considered modes</param>
		/// <param name="fp">pace frequency in [Hz]</param>
		/// <param name="DampingRatio">Damping as ratio to critical damping</param>
		/// <param name="weigthingCategory">Weighting category for human perception of vibrations</param>
		/// <returns></returns>
		/// 
		[ExcelFunction(Description = "Compute the Resonant Response Analysis for Low frequency floors for a given point with multiple vibration modes (RMS acceleration of the response)",
			Name = "SDK.Vibration.AccelerationResponseLegacy",
			IsHidden = false,
			Category = "SDK.Vibration")]
		public static double AccelerationResponseLegacy(
			[ExcelArgument(Description = "Range of modes considered")] object[,] Modes,
			[ExcelArgument(Description = "Range of frequencies considered")] object[,] Frequencies,
			[ExcelArgument(Description = "Range of modal masses considered")] object[,] ModalMasses,
			[ExcelArgument(Description = "Data for the standardized displacement according to the documentation")] object[,] StandardizedDisplacements,
			[ExcelArgument(Description = "pace frequency in [Hz]")] double fp,
			[ExcelArgument(Description = "Damping as ratio to critical damping")] double Xi,
			[ExcelArgument(Description = "Weighting category for human perception of vibrations")] string weightingType,
			[ExcelArgument(Description = "Node to consider for the excitation")] int ExcitationNode,
			[ExcelArgument(Description = "Node to consider for the response")] int ResponseNode,
			[ExcelArgument(Description = "Length of the walking path, if negative, the Eurocode resonant build up factor is considered ")] double walkingLength,
			[ExcelArgument(Description = "If true, provide the Response factor instead of the acceleration")] bool ResponseFactor)
		{
			double accelerationResponse = 0;
			try
			{
				//ensure all lists have the same length
				int listLength = Modes.Length;
				if (Frequencies.Length != listLength || ModalMasses.Length != listLength) throw new ArgumentException("all list should have the same length");

				//range to lists
				List<int> modes = new List<int>();
				List<double> frequencies = new List<double>();
				List<double> modalMasses = new List<double>();


				for (int i = 0; i < Modes.Length; i++)
				{
					modes.Add(Convert.ToInt32(Modes[i, 0]));
					frequencies.Add(Convert.ToDouble(Frequencies[i, 0]));
					modalMasses.Add(Convert.ToDouble(ModalMasses[i, 0]));
				}

				//Divide Data range into 5 lists
				List<int> NodeNb = new List<int>();
				List<int> ModeNb = new List<int>();
				List<double> ux = new List<double>();
				List<double> uy = new List<double>();
				List<double> uz = new List<double>();
				for (int i = 0; i < StandardizedDisplacements.Length / 5; i++)
				{
					NodeNb.Add(Convert.ToInt32(StandardizedDisplacements[i, 0]));
					ModeNb.Add(Convert.ToInt32(StandardizedDisplacements[i, 1]));

					//Currently horizontal response is not implemented
					//ux.Add(Convert.ToDouble(StandardizedDisplacements[i, 2]));
					//uy.Add(Convert.ToDouble(StandardizedDisplacements[i, 3]));

					uz.Add(Convert.ToDouble(StandardizedDisplacements[i, 4]));
				}



				//Create list of displacements to be analized
				int indexNodeExcitation = NodeNb.IndexOf(ExcitationNode);
				int indexNodeResponse = NodeNb.IndexOf(ResponseNode);

				List<double> uen = new List<double>();
				List<double> urn = new List<double>();

				for (int i = 0; i < listLength; i++)
				{
					uen.Add(uz[indexNodeExcitation + i]);
					urn.Add(uz[indexNodeResponse + i]);
				}

				accelerationResponse = Vibrations.ResonantResponseAnalysis(uen, urn, frequencies, modalMasses, fp, Xi, Vibrations.GetWeighting(weightingType), walkingLength, ResponseFactor);
			}
			catch (Exception e)
			{

				MessageBox.Show(e.Message);
			}

			return accelerationResponse;
		}


		/// <summary>
		/// Compute the Resonant Response Analysis for Low frequency floors for a given point with multiple vibration modes (RMS acceleration of the response)
		/// </summary>
		/// <param name="List_uen">List of the mode shape amplitudes, from the unity or mass normalised FE output, at the point on the floor where the excitation force Fh is applied</param>
		/// <param name="List_urn">List of the mode shape amplitudes, from the unity or mass normalised FE output, at the point where the response is to be calculated</param>
		/// <param name="NaturalFrequencies">List of natural frequencies for the mode considered</param>
		/// <param name="fp">pace frequency in [Hz]</param>
		/// <param name="DampingRatio">Damping as ratio to critical damping</param>
		/// <param name="weigthingCategory">Weighting category for human perception of vibrations</param>
		/// <returns></returns>
		/// 
		[ExcelFunction(Description = "Compute the Resonant Response Analysis for Low frequency floors for a given point with multiple vibration modes (RMS acceleration of the response) - For Sofistik input",
			Name = "SDK.Vibration.AccelerationResponseSofistik",
			IsHidden = false,
			Category = "SDK.Vibration")]
		public static double AccelerationResponseSofistik(
			[ExcelArgument(Description = "Range of modes considered")] object[,] Modes,
			[ExcelArgument(Description = "Range of frequencies considered")] object[,] Frequencies,
			[ExcelArgument(Description = "Data for the standardized displacement according to the documentation")] object[,] StandardizedDisplacements,
			[ExcelArgument(Description = "pace frequency in [Hz]")] double fp,
			[ExcelArgument(Description = "Damping as ratio to critical damping")] double Xi,
			[ExcelArgument(Description = "Weighting category for human perception of vibrations")] string weightingType,
			[ExcelArgument(Description = "Node to consider for the excitation")] int ExcitationNode,
			[ExcelArgument(Description = "Node to consider for the response")] int ResponseNode,
			[ExcelArgument(Description = "Length of the walking path, if negative, the Eurocode resonant build up factor is considered ")] double walkingLength,
			[ExcelArgument(Description = "If true, provide the Response factor instead of the acceleration")] bool ResponseFactor)
		{
			double accelerationResponse = 0;
			try
			{
				//ensure all lists have the same length
				if (Frequencies.Length != Modes.Length) throw new ArgumentException("all list should have the same length");

				//range to lists
				List<int> modes = new List<int>();
				List<double> frequencies = new List<double>();
				List<double> modalMasses = new List<double>();

				for (int i = 0; i < Modes.Length; i++)
				{
					modes.Add(Convert.ToInt32(Modes[i, 0]));
					frequencies.Add(Convert.ToDouble(Frequencies[i, 0]));
					modalMasses.Add(1);
				}


				var data = FormatExcelData_FE_Node(StandardizedDisplacements, true);
				var disp = GetDisplacements(data.NodeNb, ExcitationNode, ResponseNode, frequencies.Count, data.uz);

				accelerationResponse = Vibrations.ResonantResponseAnalysis(disp.uen, disp.urn, frequencies, modalMasses, fp, Xi, Vibrations.GetWeighting(weightingType), walkingLength, ResponseFactor);


			}
			catch (Exception e)
			{

				MessageBox.Show(e.Message);
			}


			return accelerationResponse;

		}



		/// <summary>
		/// Exctacts the data from the Excel selected range and provides a list of VibrationResultPoint objects
		/// </summary>
		/// <param name="modes"></param>
		/// <param name="standardizedDisplacementsInput"></param>
		/// <returns></returns>
		private static List<VibrationResultNode> FormatExcelRFEM6Data(List<int> modes, object[,] standardizedDisplacementsInput)
		{
			//output
			List<VibrationResultNode> output = new List<VibrationResultNode>();

			//Number of blank rows on the Excel RFEM output
			List<int> blankRows = new List<int>();

			//Length of row of the Excel range selected
			int rowLength = standardizedDisplacementsInput.GetLength(0);


			//Iterate over every row to determine where (if any) blank rows are present
			for (int i = 0; i < rowLength; i++)
			{
				if (standardizedDisplacementsInput[i, 0] is ExcelDna.Integration.ExcelEmpty) blankRows.Add(i);
			}


			//Extract data from the Excel selected range and populate the list of VibrationResultPoint 
			for (int i = 0; i < rowLength; i += modes.Count)
			{
				Dictionary<List<int>, List<List<double>>> inputData = new Dictionary<List<int>, List<List<double>>>();
				List<double> ux = new List<double>();
				List<double> uy = new List<double>();
				List<double> uz = new List<double>();


				//It happens that RFEM6 specifies the message "The result value in the point is not defined." when a value on surface is invalid
				//This is the case for non rectangular surfases 
				//In such case, the point is ignored
				if (Convert.ToString(standardizedDisplacementsInput[i, 6]) != "The result value in the point is not defined.")
				{
					for (int j = 0; j < modes.Count; j++)
					{
						ux.Add(Convert.ToDouble(standardizedDisplacementsInput[i + j, 6]));
						uy.Add(Convert.ToDouble(standardizedDisplacementsInput[i + j, 7]));
						uz.Add(Convert.ToDouble(standardizedDisplacementsInput[i + j, 8]));
					}

					inputData.Add(modes, new List<List<double>> { ux, uy, uz });

					int surfaceId = Convert.ToInt32(standardizedDisplacementsInput[i, 0]);
					int pointID = Convert.ToInt32(standardizedDisplacementsInput[i, 1]);
					double x = Convert.ToDouble(standardizedDisplacementsInput[i, 2]);
					double y = Convert.ToDouble(standardizedDisplacementsInput[i, 3]);
					double z = Convert.ToDouble(standardizedDisplacementsInput[i, 4]);

					output.Add(new VibrationResultNode(pointID, surfaceId, x, y, z, inputData));
				}
				else
				{
					i += modes.Count;
					if (blankRows.Contains(i))
					{
						i++;
					}
				}


				if (blankRows.Contains(i + modes.Count))
				{
					i++;
				}
			}



			return output;

		}


		/// <summary>
		/// Divide Excel data range into 5 lists to be used for the vibration calculation
		/// Function to use when each ligne of the data is a single FE point (RFEM 5, Sofistik, ...)
		/// </summary>
		/// <param name="StandardizedDisplacements"></param>
		/// <param name="sofistikInput"></param>
		/// <returns></returns>
		private static (List<int> NodeNb, List<int> ModeNb, List<double> uz) FormatExcelData_FE_Node(object[,] StandardizedDisplacements, bool sofistikInput)
		{
			//Divide Data range into 5 lists
			List<int> NodeNb = new List<int>();
			List<int> ModeNb = new List<int>();
			List<double> ux = new List<double>();
			List<double> uy = new List<double>();
			List<double> uz = new List<double>();
			for (int i = 0; i < StandardizedDisplacements.Length / 5; i++)
			{
				NodeNb.Add(Convert.ToInt32(StandardizedDisplacements[i, 0]));
				ModeNb.Add(Convert.ToInt32(StandardizedDisplacements[i, 1]));

				if (sofistikInput)
				{
					//Currently horizontal response is not implemented
					//ux.Add(Convert.ToDouble(StandardizedDisplacements[i, 2]));
					//uy.Add(Convert.ToDouble(StandardizedDisplacements[i, 3]));

					uz.Add(ScalingSofistikModeShape(Convert.ToDouble(StandardizedDisplacements[i, 4])));
				}
				else
				{
					//Currently horizontal response is not implemented
					//ux.Add(Convert.ToDouble(StandardizedDisplacements[i, 2]));
					//uy.Add(Convert.ToDouble(StandardizedDisplacements[i, 3]));

					uz.Add(Convert.ToDouble(StandardizedDisplacements[i, 4]));
				}
			}
			(List<int>, List<int>, List<double>) resultsTuple = (NodeNb, ModeNb, uz);
			return resultsTuple;
		}

		private static (List<double> uen, List<double> urn) GetDisplacements(List<int> NodeNb, int excitationNode, int ResponseNode, int numberOfModes, List<double> displacements)
		{
			//Create list of displacements to be analized
			int indexNodeExcitation = NodeNb.IndexOf(excitationNode);
			int indexNodeResponse = NodeNb.IndexOf(ResponseNode);

			List<double> uen = new List<double>();
			List<double> urn = new List<double>();

			for (int i = 0; i < numberOfModes; i++)
			{
				uen.Add(displacements[indexNodeExcitation + i]);
				urn.Add(displacements[indexNodeResponse + i]);
			}

			(List<double>, List<double>) resultsTuple = (uen, urn);
			return resultsTuple;

		}

		/// <summary>
		/// Scale the sofistik output by  data / (1000 x SQR(1000))
		/// </summary>
		/// <param name="disp">Sofistik input in [mm]</param>
		/// <returns></returns>
		private static double ScalingSofistikModeShape(double disp)
		{
			return disp / (1000 * Math.Sqrt(1000));
		}


		private class VibrationResultNode
		{

			public int PointID { get; set; }
			//geometrical position of the considered point (RFEM grid point coordinate)
			public double X { get; set; }
			public double Y { get; set; }
			public double Z { get; set; }

			//RFEM surface ID
			public int SurfaceId { get; set; }

			public Dictionary<List<int>, List<List<double>>> NormalisedDisplacements { get; set; }

			public VibrationResultNode(int pointID, int surfaceId, double x, double y, double z, Dictionary<List<int>, List<List<double>>> normalisedDisplacements)
			{
				//ModeID = modeID;
				X = x;
				Y = y;
				Z = z;
				SurfaceId = surfaceId;
				PointID = pointID;
				NormalisedDisplacements = normalisedDisplacements;
			}
		}
	}


}

