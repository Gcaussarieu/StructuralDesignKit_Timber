﻿
using StructuralDesignKitLibrary.CrossSections.Interfaces;
using StructuralDesignKitLibrary.Materials;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StructuralDesignKitLibrary.CrossSections
{
    /// <summary>
    /// Class defining a CLT Cross-section
    /// A cross section differs from a layup. The layup represents the "physical" assembly of boards
    /// while whe cross section provides the mechanical properties, either in X (0°) or Y (90°)
    /// 
    /// the cross section can be equivalent to its parent layup or it can be a part of it
    /// 
    /// A layup has two cross sections (in X and Y)
    /// 
    /// The different notations and approaches are taken from 
    /// - "2018 Wallner-Novak M., CLT structural design I proHOLZ -  ISBN 978-3-902926-03-6"
    /// - "2018 Wallner-Novak M., CLT structural design II proHOLZ -  ISBN 978-3-902320-96-4"
    /// </summary>
    public class CrossSectionCLT
    {

		#region properties
		public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Thickness { get; set; }
        public int NbOfLayers { get; set; }

        /// <summary>
        /// List of lamella thicknesses
        /// </summary>
        public List<double> LamellaThicknesses { get; set; }

        /// <summary>
        /// List of lamella orientation (0° or 90°)
        /// </summary>

        public List<int> LamellaOrientations { get; set; }

        /// <summary>
        /// List of the material constituing each lamella
        /// </summary>
        public List<MaterialCLT> LamellaMaterials { get; set; }

        /// <summary>
        /// Position of the lamella center of gravity from the CS upper edge
        /// </summary>
        public List<double> LamellaCoGDistanceFromTopFibre { get; set; }

        /// <summary>
        /// Distance from the center of gravity of a layer toward the overall center of gravity
        /// </summary>
        public List<double> LamellaDistanceToCDG { get; set; }


        //Reference E modulus 
        private double ERef { get; set; }



        //Center of gravity in both main direction ; in mm from the top
        public double CenterOfGravity { get; set; }


        /// <summary>
        /// Distance of the bottom edge to the overall center of gravity - X direction
        /// </summary>
        public double Zu { get; set; }

        /// <summary>
        /// Distance of the top edge to the overall center of gravity - X direction
        /// </summary>
        public double Zo { get; set; }

        /// <summary>
        /// Active area in the considered direction in mm²
        /// </summary>
        public double Area { get; set; }

        /// <summary>
        /// Moment of inertia in mm4
        /// </summary>
        public double MomentOfInertia { get; set; }

        /// <summary>
        /// Effective Moment of inertia in mm4 considering the influence of the shear flexibility
        /// </summary>
        public double EffectiveMomentOfInertia { get; set; }

        /// <summary>
        /// Section Module (net) in mm³
        /// </summary>
        public double W0_Net { get; set; }

        /// <summary>
        /// Static Moment for  shear
        /// </summary>
        public List<List<double>> S0Net { get; set; }


        /// <summary>
        /// Effective radius of inertia
        /// </summary>
        public double RadiusOfInertia { get; set; }

        public double TorsionalInertia { get; set; }
        public double TorsionalModulus { get; set; }


        //Cross section characteristic capacities
        public double NCompressionChar { get; set; }
        public double NTensionChar { get; set; }
        public double MChar { get; set; }
        public double VChar { get; set; }
        public double NxyChar { get; set; }
        public double MxyChar { get; set; }


        //Cross section stresses per layer


        //For bending and shear, stresses to be calculated for 3 points for each layer
        //The first array dimension represents the layer from top to bottom
        //The second array dimension represents the stresses calculated
        public List<List<double>> SigmaM { get; set; }
        public List<List<double>> TauV { get; set; }

        //For normal stresses, a single value is calculated per layer
        public List<double> SigmaNComp { get; set; }
        public List<double> SigmaNTens { get; set; }

        /// <summary>
        /// Represent the sum of the area x Emean of all the layers
        /// </summary>
        private double AEeff { get; set; }



		#endregion


        //TO DO
		//------------------------------------------------------------------------------------
		//Define EIx EIy, GAx, GAy, ExEquivalent, EyEquivalent to be defined as orthotropic material in Karamba for instance 
		//with the constant stiffness representing the overall thickness of the CLT plate
		//------------------------------------------------------------------------------------



		public CrossSectionCLT(List<double> thicknesses, List<int> orientations, List<MaterialCLT> materials, int lamellaWidth = 150, bool narrowSideGlued = false)
        {
            LamellaThicknesses = thicknesses;
            LamellaOrientations = orientations;
            LamellaMaterials = materials;
            LamellaCoGDistanceFromTopFibre = new List<double>();
            LamellaDistanceToCDG = new List<double>();
            S0Net = new List<List<double>>();
            NbOfLayers = LamellaMaterials.Count;

            ComputeCrossSectionProperties();
        }


        //Implement a picture in Excel / GH equivalent to KLH documentation

        /// <summary>
        /// Compute the cross section properties
        /// </summary>
        public void ComputeCrossSectionProperties()
        {
            //Define the top lamella as the reference material
            ERef = LamellaMaterials[0].E0mean;
            Thickness = LamellaThicknesses.Sum();
            CenterOfGravity = ComputeCLTCenterOfGravity(this);
            ComputeNetAreas();
            ComputeInertia();
            ComputeAEeff();
        }


        /// <summary>
        /// Compute the CLT center of gravity, distance from the top surface
        /// </summary>
        /// <param name="CS"></param>
        /// <returns></returns>
        private double ComputeCLTCenterOfGravity(CrossSectionCLT CS)
        {
            double distToCOG = 0;

            double nominator = 0;
            double denominator = 0;

            for (int i = 0; i < CS.NbOfLayers; i++)
            {
                //Position of the center of gravity of the individual layers from the element's upper edge
                double oi = distToCOG + CS.LamellaThicknesses[i] / 2;

                CS.LamellaCoGDistanceFromTopFibre.Add(oi);

                if (CS.LamellaOrientations[i] == 0)
                {
                    nominator += CS.LamellaMaterials[i].E0mean / CS.ERef * CS.LamellaThicknesses[i] * oi;
                    denominator += CS.LamellaMaterials[i].E0mean / CS.ERef * CS.LamellaThicknesses[i];
                }

                distToCOG += CS.LamellaThicknesses[i];
            }

            CS.CenterOfGravity = nominator / denominator;


            //Compute the distance of the center of gravity of the individual layers from the Layup CoG
            for (int i = 0; i < CS.NbOfLayers; i++)
            {
                CS.LamellaDistanceToCDG.Add(CS.LamellaCoGDistanceFromTopFibre[i] - CS.CenterOfGravity);
            }


            //Compute the distance of the top and bottom edge toward the center of gravity
            CS.Zo = Math.Abs(CS.LamellaDistanceToCDG.First()) + CS.LamellaThicknesses.First() / 2;
            CS.Zu = Math.Abs(CS.LamellaDistanceToCDG.Last()) + CS.LamellaThicknesses.Last() / 2;

            return CS.CenterOfGravity;
        }


        /// <summary>
        /// Compute the net area in both directions
        /// </summary>
        private void ComputeNetAreas()
        {
            for (int i = 0; i < NbOfLayers; i++)
            {
                if (LamellaOrientations[i] == 0)
                {
                    Area += LamellaMaterials[i].E0mean / ERef * 1000 * LamellaThicknesses[i];
                }
            }
        }


        /// <summary>
        /// Compute the cross section inertia
        /// </summary>
        private void ComputeInertia()
        {
            for (int i = 0; i < NbOfLayers; i++)
            {
                if (LamellaOrientations[i] == 0)
                {
                    MomentOfInertia += LamellaMaterials[i].E0mean / ERef * 1000 * Math.Pow(LamellaThicknesses[i], 3) / 12;
                    MomentOfInertia += LamellaMaterials[i].E0mean / ERef * 1000 * LamellaThicknesses[i] * Math.Pow(LamellaDistanceToCDG[i], 2);
                }
            }
        }


        //Compute the different static moments (first moment of area) as follow
        //3 values per lamella up (top / middle / bottom) unless the CoG is in the lamella. Then a 4rth value is added
        //The calculation is done from top to bottom

        public List<List<double>> ComputeStaticMomentPerLamella()
        {
            //Ai - Area above the line where the static moment is defined
            //Y1 - Distance between the top of the CLT CS and the center of gravity of the section considered
            //Y = distance between the global center of gravity and the CoG of the section considered
            //Eref
            //Ei

            // static moments (first moment of area) Q for each layer - 3 to 4 values per layer
            List<List<double>> Q = new List<List<double>>();

            //Compute static moment per layer
            int nbLamella = LamellaDistanceToCDG.Count;


            //For the beginning of the calculation, a fictive CLT cross section cannot be created as it needs at least 2 orthogonal ply.
            //As long as the ply angle is 0°, manual calculation is provided

            List<double> S_0 = new List<double>();

            // First Lamella
            //At the top of the section, the Static moment is null
            S_0.Add(0);

            //middle
            double Ai = LamellaThicknesses[0] / 2 * 1000;
            double Y1 = LamellaThicknesses[0] / 4;
            double Y = this.CenterOfGravity - Y1;

            S_0.Add(Y * Ai);

            //Bottom
            Ai = LamellaThicknesses[0] * 1000;
            Y1 = LamellaThicknesses[0] / 2;
            Y = this.CenterOfGravity - Y1;
            S_0.Add(Y * Ai);
            Q.Add(S_0);

            int i = 1;


            //Following lamellas if oriented in the same direction (0°) as a CLT layup cannot be generated with only 0° layer.
            while (i < LamellaOrientations.Count && LamellaOrientations[i] == 0)
            {
                List<double> S = new List<double>();

                //Top of considered layer - same value as bottom of the previous layer
                S.Add(Q[i - 1][2]);

                List<double> PreviousLamellaTck = new List<double>();
                List<double> PreviousLamellaE = new List<double>();

                for (int j = 0; j < i; j++)
                {
                    PreviousLamellaTck.Add(LamellaThicknesses[j]);
                    PreviousLamellaE.Add(LamellaMaterials[j].E);
                }

                //middle 
                PreviousLamellaTck.Add(LamellaThicknesses[i] / 2);
                PreviousLamellaE.Add(LamellaMaterials[i].E);

                Y1 = ComputeCenterOfGravityFromMultipleLayers(PreviousLamellaTck, PreviousLamellaE);
                Y = this.CenterOfGravity - Y1;
                Ai = (PreviousLamellaTck.Sum()) * 1000; ;
                S.Add(Y * Ai);

                //bottom
                PreviousLamellaTck.Add(LamellaThicknesses[i] / 2);
                PreviousLamellaE.Add(LamellaMaterials[i].E);

                Y1 = ComputeCenterOfGravityFromMultipleLayers(PreviousLamellaTck, PreviousLamellaE);
                Y = this.CenterOfGravity - Y1;
                Ai = (PreviousLamellaTck.Sum()) * 1000; ;
                S.Add(Y * Ai);

                Q.Add(S);

                i++;
            }

            //from the first cross layer onward
            for (int j = i; j < nbLamella; j++)
            {

                if (LamellaOrientations[j] == 0)
                {
                    List<double> S = new List<double>();
                    S.Add((Q[j - 1][2]));

                    for (int k = 1; k < 3; k++)
                    {

                        List<double> thickTemp = new List<double>();
                        List<MaterialCLT> matTemp = new List<MaterialCLT>();
                        List<int> orientationTemp = new List<int>();

                        //Create new CS based on the thickness from top
                        for (int l = 0; l < j; l++)
                        {
                            thickTemp.Add(LamellaThicknesses[l]);
                            matTemp.Add(LamellaMaterials[l]);
                            orientationTemp.Add(LamellaOrientations[l]);
                        }

                        thickTemp.Add(LamellaThicknesses[j] * k * 0.5);
                        matTemp.Add(LamellaMaterials[j]);
                        orientationTemp.Add(LamellaOrientations[j]);

                        CrossSectionCLT tempCS = new CrossSectionCLT(thickTemp, orientationTemp, matTemp);

                        Y1 = tempCS.CenterOfGravity;
                        Y = this.CenterOfGravity - Y1;
                        Ai = tempCS.Area;
                        S.Add(Y * Ai);

                    }
                    Q.Add(S);
                }
                else
                {
                    double lastS = Q[j - 1][2];
                    Q.Add(new List<double>() { lastS, lastS, lastS });
                }

            }


            return Q;
        }


        /// <summary>
        /// Compute the radius of inertia
        /// </summary>
        private void ComputeRadiusOfInertia()
        {
            RadiusOfInertia = Math.Sqrt(EffectiveMomentOfInertia / Area);
        }


        /// <summary>
        /// Compute the effective AE value
        /// </summary>
        private void ComputeAEeff()
        {
            for (int i = 0; i < LamellaOrientations.Count; i++)
            {
                if (LamellaOrientations[i] == 0)
                {
                    AEeff += LamellaMaterials[i].E0mean * LamellaThicknesses[i];
                }
            }
        }


        /// <summary>
        /// Compute the normal stress in N/mm² per layer based on a global normal force in N
        /// </summary>
        /// <param name="NormalForce"></param>
        public void ComputeNormalStress(double NormalForce)
        {
            SigmaNTens = new List<double>();
            SigmaNComp = new List<double>();


            for (int i = 0; i < LamellaOrientations.Count; i++)
            {
                if (LamellaOrientations[i] == 0)
                {
                    double lamellaAEeff = LamellaMaterials[i].E0mean;
                    if (NormalForce > 0)
                    {
                        SigmaNTens.Add(NormalForce * (lamellaAEeff / AEeff));
                        SigmaNComp.Add(0);
                    }

                    else
                    {
                        SigmaNComp.Add(NormalForce * (lamellaAEeff / AEeff));
                        SigmaNTens.Add(0);

                    }
                }
            }
        }


        /// <summary>
        /// Compute the shear stresses (in N/mm²) per layer based on a shear force (in N)
        /// </summary>
        /// <param name="ShearForceZ"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ComputeShearStress(double ShearForceZ)
        {
            TauV = new List<List<double>>();
			for (int i = 0; i < S0Net.Count; i++)
			{
                TauV.Add(new List<double>());
				for (int j = 0; j < S0Net[i].Count; j++)
				{
					TauV[i].Add(ShearForceZ * S0Net[i][j] / (MomentOfInertia * 1000));
				}
			}

		}


		/// <summary>
		/// Compute the bending stress in N/mm² per layer based on a global bending moment force in kN.m
		/// </summary>
		/// <param name="BendingMomentY"></param>
		public void ComputeBendingStress(double BendingMomentY)
        {
            var Wnet = ComputeWnetPerLayers();

            List<List<double>> layers = new List<List<double>>();
            for (int i = 0; i < LamellaOrientations.Count; i++)
            {
                List<double> stresses = new List<double>();
                if (LamellaOrientations[i] == 0)
                {
                    stresses.Add(LamellaMaterials[i].E0mean / ERef * BendingMomentY * 1e6 / Wnet[i][0]);
                    stresses.Add(LamellaMaterials[i].E0mean / ERef * BendingMomentY * 1e6 / Wnet[i][1]);
                    stresses.Add(LamellaMaterials[i].E0mean / ERef * BendingMomentY * 1e6 / Wnet[i][2]);

                    layers.Add(stresses);
                }
                else layers.Add(new List<double>() { 0, 0, 0 });
            }

            SigmaM = layers;
        }


        public List<double> ComputeTorsionStress(double TorsionMoment)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Provide the characteristic compression capacity in kN/m 
        /// </summary>
        private void ComputeCompressionCapacity()
        {

            List<double> MaxForce = new List<double>();

            for (int i = 0; i < LamellaOrientations.Count; i++)
            {
                if (LamellaOrientations[i] == 0)
                {
                    MaxForce.Add(LamellaMaterials[i].Fc0k * AEeff / LamellaMaterials[i].E0mean);
                }
            }

            NCompressionChar = MaxForce.Min();
        }


        /// <summary>
        /// Provide the characteristic tension capacity in kN/m 
        /// </summary>
        private void ComputeTensionCapacity()
        {

            List<double> MaxForce = new List<double>();
            for (int i = 0; i < LamellaOrientations.Count; i++)
            {
                if (LamellaOrientations[i] == 0)
                {
                    MaxForce.Add(LamellaMaterials[i].Ft0k * AEeff / LamellaMaterials[i].E0mean);
                }
            }

            NTensionChar = MaxForce.Min();

        }


        /// <summary>
        /// Provide the characteristic bending capacity in kN.m/m 
        /// </summary>
        private void ComputeBendingCapacity()
        {

            List<double> MaxBending = new List<double>();

            var Wnet = ComputeWnetPerLayers();

            for (int i = 0; i < LamellaOrientations.Count; i++)
            {

                if (LamellaOrientations[i] == 0)
                {
                    MaxBending.Add(Math.Abs(ERef / LamellaMaterials[i].E0mean * LamellaMaterials[i].Fmk * Wnet[i][0]));
                    MaxBending.Add(Math.Abs(ERef / LamellaMaterials[i].E0mean * LamellaMaterials[i].Fmk * Wnet[i][1]));
                    MaxBending.Add(Math.Abs(ERef / LamellaMaterials[i].E0mean * LamellaMaterials[i].Fmk * Wnet[i][2]));

                }
            }



            MChar = MaxBending.Min() / 1000000;
        }


        private List<List<double>> ComputeWnetPerLayers()
        {
            List<List<double>> layers = new List<List<double>>();
            for (int i = 0; i < LamellaOrientations.Count; i++)
            {
                List<double> Wnet = new List<double>();
                if (LamellaOrientations[i] == 0)
                {
                    double zTop;
                    double zMiddle;
                    double zBottom;


                    zTop = LamellaDistanceToCDG[i] - LamellaThicknesses[i] * 0.5;
                    zMiddle = LamellaDistanceToCDG[i];
                    zBottom = LamellaDistanceToCDG[i] + LamellaThicknesses[i] * 0.5;

                    Wnet.Add(MomentOfInertia / zTop);
                    Wnet.Add(MomentOfInertia / zMiddle);
                    Wnet.Add(MomentOfInertia / zBottom);

                    layers.Add(Wnet);
                }
                else layers.Add(new List<double>() { 0, 0, 0 });
            }

            return layers;
        }

        /// <summary>
        /// Provide the characteristic shear capacity in kN/m 
        /// </summary>
        private void ComputeShearCapacity()
        {
            S0Net = ComputeStaticMomentPerLamella();

            List<double> V = new List<double>();
            VChar = 0;

            for (int i = 0; i < LamellaOrientations.Count; i++)
            {
                if (LamellaOrientations[i] == 0)
                {
                    for (int j = 0; j < 3; j++)
                    {

                        V.Add(LamellaMaterials[i].Fvk * MomentOfInertia / S0Net[i][j]);

                    }
                }

                else
                {
                    V.Add(LamellaMaterials[i].Frk * MomentOfInertia / S0Net[i][0]);
                }
            }


            VChar = V.Min();
        }

        public void ComputeCapacities()
        {
            ComputeCompressionCapacity();
            ComputeTensionCapacity();
            ComputeBendingCapacity();
            ComputeShearCapacity();
        }

        /// <summary>
        /// Return true if all layers in the given direction are made of the same material
        /// </summary>
        /// <returns></returns>
        private bool MaterialConsistenty()
        {

            IMaterialCLT baseMaterial = LamellaMaterials[0];
            foreach (IMaterialTimber mat in LamellaMaterials)
            {
                if (mat != baseMaterial)
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// helper method used to define the center of gravity of multiple layers oriented in the same direction with eventually, different modulus of elasticity
        /// </summary>
        /// <param name="layerThickness">Layer thickness</param>
        /// <param name="E">Laayer modulus of elasticity</param>
        /// <returns></returns>
        private double ComputeCenterOfGravityFromMultipleLayers(List<double> layerThickness, List<double> E)
        {
            if (layerThickness.Count != E.Count) throw new Exception("lists length do not match");


            double distToCOG = 0;
            List<double> LamellaCoGDistanceFromTop = new List<double>();
            double nominator = 0;
            double denominator = 0;
            double E_ref = E[0];

            double CoG = 0;

            for (int i = 0; i < layerThickness.Count; i++)
            {
                //Position of the center of gravity of the individual layers from the element's upper edge
                double oi = distToCOG + layerThickness[i] / 2;

                LamellaCoGDistanceFromTop.Add(oi);

                nominator += E[i] / E_ref * layerThickness[i] * oi;
                denominator += E[i] / E_ref * layerThickness[i];

                distToCOG += layerThickness[i];
            }

            CoG = nominator / denominator;

            return CoG;
        }


    }
}
