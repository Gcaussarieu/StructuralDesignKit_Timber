﻿using ExcelDna.Integration.CustomUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using ExcelDna.Integration;
using Microsoft.Office.Interop.Excel;
using System.Xml;
using System.IO;
using StructuralDesignKitLibrary.EC5;
using StructuralDesignKitLibrary.Connections.TimberTimberShear;
using StructuralDesignKitLibrary.Connections.SteelTimberShear;
using StructuralDesignKitLibrary.Vibrations;
using System.Windows;
using System.Security.Claims;
using StructuralDesignKitExcel.RibbonActions;



namespace StructuralDesignKitExcel
{

    /// <summary>
    /// Methods linked to the ribbon buttons
    /// </summary>
    [ComVisible(true)] //To make Excel recognize the ribbon
    public class RibbonController : ExcelRibbon
    {

        public string GetConnectionMenuContent(IRibbonControl control)
        {
            string category = control.Tag;

            var menuXml = new StringWriter();
            using (XmlWriter writer = XmlWriter.Create(menuXml))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("menu", @"http://schemas.microsoft.com/office/2006/01/customui");
                writer.WriteAttributeString("xmlns", @"http://schemas.microsoft.com/office/2006/01/customui");

                //Get methods from a class
                var methods = typeof(ExcelFormulae).GetMethods().ToList();
                var methodsWithCategory = GetMethods(methods, category);



                var categoryMethods = GetMethods(methods, category);

                foreach (var method in categoryMethods)
                {
                    writer.WriteStartElement("button");
                    writer.WriteAttributeString("id", "button_" + method.Name);
                    writer.WriteAttributeString("label", method.Name);
                    writer.WriteAttributeString("onAction", "OnConnectionButton");
                    string tag = ExcelHelpers.GetTag(method);
					writer.WriteAttributeString("tag",tag);
					string description = ExcelHelpers.GetDescription(method);
					writer.WriteAttributeString("screentip", description);
                    writer.WriteEndElement();
                }

                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();

            }
            return menuXml.ToString();
        }


        public void OnConnectionButton(IRibbonControl control)
        {
            string name = control.Id.Split('_')[1];
            string tag = control.Tag.Split('_')[1];


            var xlApp = (Excel.Application)ExcelDnaUtil.Application;
            ExcelHelpers.WorkBookOpen(xlApp); //Ensure a workbook is open


            //Case Steel to timber
            if (tag == "SteelTimber")
            {
                ConnectionButtonActions.SteelConnectionButtonActions(xlApp, name);
            }
            else if (tag == "TimberTimber")
            {
                ConnectionButtonActions.TimberConnectionButtonActions(xlApp, name);
            }
        }


        /// <summary>
        /// Populate a dynamic button with the function available under the SDK.XXXX namespace.
        /// Return a XML string         /// 
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public string GetMenuContent(IRibbonControl control)
        {
            string category = control.Tag;

            var menuXML = new StringWriter();
            using (XmlWriter writer = XmlWriter.Create(menuXML))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("menu", @"http://schemas.microsoft.com/office/2006/01/customui");
                writer.WriteAttributeString("xmlns", @"http://schemas.microsoft.com/office/2006/01/customui");

                //Get methods from a class
                var methods = typeof(ExcelFormulae).GetMethods().ToList();

                //filtering Factors Methods
                //List<System.Reflection.MethodInfo> methodsWithArgument = new List<System.Reflection.MethodInfo>();
                //foreach (var method in methods)
                //{
                //    if (method.CustomAttributes.ToList().Count >= 1)
                //    {
                //        if (method.CustomAttributes.ToList()[0].NamedArguments.Count >= 3)
                //        {
                //            methodsWithArgument.Add(method);
                //        }
                //    }
                //}

                //var categoryMethods = methodsWithArgument.Where(p => p.CustomAttributes.ToList()[0].NamedArguments[2].TypedValue.Value.ToString() == category).ToList();
                var categoryMethods = GetMethods(methods, category);

                foreach (var method in categoryMethods)
                {
                    writer.WriteStartElement("button");
                    writer.WriteAttributeString("id", "button_" + method.Name);
                    writer.WriteAttributeString("label", method.Name);
                    writer.WriteAttributeString("onAction", "OnButton");
					string tag = ExcelHelpers.GetTag(method);

					writer.WriteAttributeString("tag", tag);
					string description = ExcelHelpers.GetTag(method);

					writer.WriteAttributeString("screentip",description);
                    writer.WriteEndElement();
                }

                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();

            }
            return menuXML.ToString();
        }


        public void OnButton(IRibbonControl control)
        {
            Excel.Application xlApp = (Excel.Application)ExcelDnaUtil.Application;
            ExcelHelpers.WorkBookOpen(xlApp); //Ensure a workbook is open

            var cell = xlApp.ActiveCell;

            // AppActivate Application.Caption;
            cell.Select();
            xlApp.SendKeys("{F2}");
            cell.Value2 = "=" + control.Tag;
            xlApp.SendKeys("{(}");
        }


        public void OnButtonPressedKmod(IRibbonControl control)
        {
            var xlApp = (Excel.Application)ExcelDnaUtil.Application;
            ExcelHelpers.WorkBookOpen(xlApp); //Ensure a workbook is open

            var baseCell = xlApp.ActiveCell;
            var activeCell = xlApp.ActiveCell;

            var LoadDurations = ExcelHelpers.GetStringValuesFromEnum<StructuralDesignKitLibrary.EC5.EC5_Utilities.LoadDuration>();
            var TimberTypes = ExcelHelpers.AllMaterialAsList();
            var ServiceClasses = ExcelHelpers.GetStringValuesFromEnum<StructuralDesignKitLibrary.EC5.EC5_Utilities.ServiceClass>();

            activeCell.Value2 = "Kmod";
            activeCell = activeCell.Offset[1, 0];
            //Timber grade service class laod duration
            var gradeCell = activeCell.Offset[1, 0];
            var ServiceClassCell = gradeCell.Offset[1, 0];
            var LoadDurationCell = ServiceClassCell.Offset[1, 0];

            RibbonUtilities.ValidateCellWithList(gradeCell, TimberTypes);
            RibbonUtilities.ValidateCellWithList(ServiceClassCell, ServiceClasses);
            RibbonUtilities.ValidateCellWithList(LoadDurationCell, LoadDurations);

            string formula = string.Format("=SDK.Factors.Kmod({0},{1},{2})", gradeCell.Address[false, false], ServiceClassCell.Address[false, false], LoadDurationCell.Address[false, false]);
            activeCell.Value2 = formula;

            //Format
            baseCell.Font.Bold = true;
            for (int i = 0; i < 5; i++)
            {
                baseCell.Offset[i, 0].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            }

            for (int i = 0; i < 3; i++)
            {
                baseCell.Offset[i + 2, 0].Interior.Color = XlRgbColor.rgbLightYellow;
            }

            var range = xlApp.Range[baseCell, baseCell.Offset[4, 0]];
            range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            range.Columns.AutoFit();


        }


        public void MaterialList(IRibbonControl control)
        {
            List<string> timberType = new List<string>();

            var xlApp = (Excel.Application)ExcelDnaUtil.Application;
            var activeCell = xlApp.ActiveCell;
            switch (control.Tag)
            {
                case "Softwood":
                    timberType = ExcelHelpers.GetStringValuesFromEnum<StructuralDesignKitLibrary.Materials.MaterialTimberSoftwood.Grades>();
                    break;
                case "Hardwood":
                    timberType = ExcelHelpers.GetStringValuesFromEnum<StructuralDesignKitLibrary.Materials.MaterialTimberHardwood.Grades>();
                    break;
                case "Glulam":
                    timberType = ExcelHelpers.GetStringValuesFromEnum<StructuralDesignKitLibrary.Materials.MaterialTimberGlulam.Grades>();
                    break;
                case "Baubuche":
                    timberType = ExcelHelpers.GetStringValuesFromEnum<StructuralDesignKitLibrary.Materials.MaterialTimberBaubuche.Grades>();
                    break;
                case "All":
                    timberType = ExcelHelpers.AllMaterialAsList();
                    break;

            }

            RibbonUtilities.ValidateCellWithList(activeCell, timberType);
        }


        public void OnButtonPressedCSCheck(IRibbonControl control)
        {
            Excel.Application xlApp = (Excel.Application)ExcelDnaUtil.Application;
            ExcelHelpers.WorkBookOpen(xlApp); //Ensure a workbook is open

            var baseCell = xlApp.ActiveCell;
            var activeCell = xlApp.ActiveCell;

            string formula = string.Empty;
            string formatSeparator = "0.00";
            string formatPercent = "0.0%";
            if (xlApp.DecimalSeparator == ",")
            {
                formatSeparator = "0,00";
                formatPercent = "0,0%";
            }


            //------------------------------------------------------------------------------------------------
            //define block length - Represent how many rows are in each section to ease selection process
            //------------------------------------------------------------------------------------------------


            //Cross section and input definition
            int blockCSLength = 12;

            //Loads definition
            int blockForceStart = blockCSLength + 1;
            int blockForceLength = 9;

            //Stresses 
            int blockStressesStart = blockForceStart + blockForceLength + 1;
            int blockStressesLength = 8;

            //Checks
            int BlockCheckLength = 10;

            //Factors
            int blockFactorStart = BlockCheckLength + 1;
            int BlockFactorLength = 11;


            //--------------------------------------------------------
            //Cell Adresses
            //--------------------------------------------------------

            var bAdr = baseCell.Offset[1, 1].Address[false, false];
            var hAdr = baseCell.Offset[2, 1].Address[false, false];
            var MaterialAdr = baseCell.Offset[3, 1].Address[false, false];
            var FireAdr = baseCell.Offset[5, 1].Address[false, false];
            var CSAdr = baseCell.Offset[6, 1].Address[false, false];
            var ServiceClassAdr = baseCell.Offset[7, 1].Address[false, false];
            var BuckLyAdr = baseCell.Offset[8, 1].Address[false, false];
            var BuckLzAdr = baseCell.Offset[9, 1].Address[false, false];
            var LTBAdr = baseCell.Offset[10, 1].Address[false, false];
            var TensionLengthAdr = baseCell.Offset[11, 1].Address[false, false];


            var LoadDurationAdr = baseCell.Offset[blockForceStart + 1, 1].Address[false, false];
            var NTensAdr = baseCell.Offset[blockForceStart + 2, 1].Address[false, false];
            var NCompAdr = baseCell.Offset[blockForceStart + 3, 1].Address[false, false];
            var VyAdr = baseCell.Offset[blockForceStart + 4, 1].Address[false, false];
            var VzAdr = baseCell.Offset[blockForceStart + 5, 1].Address[false, false];
            var MxAdr = baseCell.Offset[blockForceStart + 6, 1].Address[false, false];
            var MyAdr = baseCell.Offset[blockForceStart + 7, 1].Address[false, false];
            var MzAdr = baseCell.Offset[blockForceStart + 8, 1].Address[false, false];


            var SigNTenAdr = baseCell.Offset[blockStressesStart + 1, 1].Address[false, false];
            var SigNCompAdr = baseCell.Offset[blockStressesStart + 2, 1].Address[false, false];
            var TauYAdr = baseCell.Offset[blockStressesStart + 3, 1].Address[false, false];
            var TauZAdr = baseCell.Offset[blockStressesStart + 4, 1].Address[false, false];
            var TauTorAdr = baseCell.Offset[blockStressesStart + 5, 1].Address[false, false];
            var SigMyAdr = baseCell.Offset[blockStressesStart + 6, 1].Address[false, false];
            var SigMzAdr = baseCell.Offset[blockStressesStart + 7, 1].Address[false, false];


            var kmodAdr = baseCell.Offset[blockFactorStart + 1, 5].Address[false, false];
            var YmAdr = baseCell.Offset[blockFactorStart + 2, 5].Address[false, false];
            var khyAdr = baseCell.Offset[blockFactorStart + 3, 5].Address[false, false];
            var khzAdr = baseCell.Offset[blockFactorStart + 4, 5].Address[false, false];
            var kcyAdr = baseCell.Offset[blockFactorStart + 5, 5].Address[false, false];
            var kczAdr = baseCell.Offset[blockFactorStart + 6, 5].Address[false, false];
            var kcritAdr = baseCell.Offset[blockFactorStart + 7, 5].Address[false, false];
            var kcrAdr = baseCell.Offset[blockFactorStart + 8, 5].Address[false, false];
            var Kh_TensionAdr = baseCell.Offset[blockFactorStart + 9, 5].Address[false, false];
            var Kh_TensionLVLAdr = baseCell.Offset[blockFactorStart + 10, 5].Address[false, false];



            //----------------------------------
            //Define cross section and material
            //----------------------------------

            //Captions
            activeCell.Value2 = "Cross section";
            activeCell = baseCell.Offset[1, 0]; activeCell.Value2 = "b";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "h";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Material";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Cross Section";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Fire Duration";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Reduced Cross Section";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Service Class";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Buckling Length Ly";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Buckling Length Lz";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Torsional buckling Length Ltb";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Member length in tension (LVL)";


            //Values
            activeCell = baseCell.Offset[1, 1]; activeCell.Value2 = "100";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "400";
            activeCell = activeCell.Offset[1, 0]; RibbonUtilities.ValidateCellWithList(activeCell, ExcelHelpers.AllMaterialAsList());
            activeCell.Value2 = "GL24h";
            formula = string.Format("=SDK.Utilities.CreateRectangularCrossSection({0},{1},{2})", activeCell.Offset[-2, 0].Address[false, false],
                activeCell.Offset[-1, 0].Address[false, false], activeCell.Address[false, false]);
            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = formula;
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = 0;
            //formula = ;
            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = String.Format("=SDK.FireDesign.CreateReducedCrossSection({0},{1},1,1,1,1)", activeCell.Offset[-2, 0].Address[false, false],
                activeCell.Offset[-1, 0].Address[false, false]);
            activeCell = activeCell.Offset[1, 0]; RibbonUtilities.ValidateCellWithList(activeCell,
                ExcelHelpers.GetStringValuesFromEnum<StructuralDesignKitLibrary.EC5.EC5_Utilities.ServiceClass>());
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "3";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "1.5";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "3";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "3";


            //Units - Comments
            activeCell = baseCell.Offset[1, 2]; activeCell.Value2 = "mm";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "mm";
            activeCell = activeCell.Offset[3, 0]; activeCell.Value2 = "min";
            activeCell = activeCell.Offset[3, 0]; activeCell.Value2 = "m";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "m";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "m";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "m";

            //Format
            baseCell.Font.Bold = true;
            activeCell = baseCell.Offset[1, 1];
            for (int i = 0; i < blockCSLength - 1; i++)
            {
                activeCell.Offset[i, 0].Interior.Color = XlRgbColor.rgbLightYellow;
                activeCell.Offset[i, 0].HorizontalAlignment = XlHAlign.xlHAlignRight;
            }
            var range = xlApp.Range[baseCell, baseCell.Offset[blockCSLength - 1, 2]];
            range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;





            //----------------------------------
            //Define forces/loads
            //----------------------------------

            //Captions
            activeCell = baseCell.Offset[blockForceStart, 0]; activeCell.Value2 = "Loads";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Load duration";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Normal tension Force N(+) = ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Normal Compression Force N(-) = ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Shear Vy = ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Shear Vz = ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Torsion Mx = ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Bending My = ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Bending Mz = ";

            //Values
            activeCell = baseCell.Offset[blockForceStart + 1, 1]; RibbonUtilities.ValidateCellWithList(activeCell, ExcelHelpers.GetStringValuesFromEnum<StructuralDesignKitLibrary.EC5.EC5_Utilities.LoadDuration>());
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "50";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "-100";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "5";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "5";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "2";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "30";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "15";

            //Units - Comments
            activeCell = baseCell.Offset[blockForceStart + 2, 2]; activeCell.Value2 = "KN";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "KN";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "KN";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "KN";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "KN.m";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "KN.m";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "KN.m";


            //Format
            baseCell.Offset[blockForceStart, 0].Font.Bold = true;
            activeCell = baseCell.Offset[blockForceStart + 1, 1];
            for (int i = 0; i < blockForceLength - 1; i++)
            {
                activeCell.Offset[i, 0].Interior.Color = XlRgbColor.rgbLightYellow;
                activeCell.Offset[i, 0].HorizontalAlignment = XlHAlign.xlHAlignRight;
            }
            range = xlApp.Range[baseCell.Offset[blockForceStart, 0], baseCell.Offset[blockForceStart + blockForceLength - 1, 2]];
            range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;


            //----------------------------------
            //Define Stresses
            //----------------------------------

            //Captions
            activeCell = baseCell.Offset[blockStressesStart, 0]; activeCell.Formula = "Stresses";
            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = "σ_N_Tension = ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = "σ_N_Comp = ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = "τ_y = ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = "τ_z = ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = "τ_Torsion = ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = "σ_My = ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = "σ_Mz = ";


            //Values
            xlApp.Range[SigNTenAdr].Formula = String.Format("=SDK.CrossSection_StressCompute.NormalForce({0},{1})", NTensAdr, CSAdr);
            xlApp.Range[SigNCompAdr].Formula = String.Format("=SDK.CrossSection_StressCompute.NormalForce({0},{1})", NCompAdr, CSAdr);
            xlApp.Range[TauYAdr].Formula = String.Format("=SDK.CrossSection_StressCompute.ShearY({0},{1})", VyAdr, CSAdr);
            xlApp.Range[TauZAdr].Formula = String.Format("=SDK.CrossSection_StressCompute.ShearZ({0},{1})", VzAdr, CSAdr);
            xlApp.Range[TauTorAdr].Formula = String.Format("=SDK.CrossSection_StressCompute.TorsionShear({0},{1})", MxAdr, CSAdr);
            xlApp.Range[SigMyAdr].Formula = String.Format("=SDK.CrossSection_StressCompute.BendingY({0},{1})", MyAdr, CSAdr);
            xlApp.Range[SigMzAdr].Formula = String.Format("=SDK.CrossSection_StressCompute.BendingZ({0},{1})", MzAdr, CSAdr);




            //Units-Comments
            for (int i = 0; i < 7; i++)
            {
                baseCell.Offset[blockStressesStart + 1 + i, 2].Value2 = "N/mm²";
            }


            //Format
            baseCell.Offset[blockStressesStart, 0].Font.Bold = true;
            activeCell = baseCell.Offset[blockStressesStart + 1, 1];
            for (int i = 0; i < blockStressesLength - 1; i++)
            {
                activeCell.Offset[i, 0].HorizontalAlignment = XlHAlign.xlHAlignRight;
                ((dynamic)activeCell.Offset[i, 0]).NumberFormatLocal = formatSeparator;
            }
            range = xlApp.Range[baseCell.Offset[blockStressesStart, 0], baseCell.Offset[blockStressesStart + blockStressesLength - 1, 2]];
            range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;




            //----------------------------------
            //Define Checks Factors
            //----------------------------------
            //Captions
            activeCell = baseCell.Offset[blockFactorStart, 4]; activeCell.Value2 = "Factors";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Kmod";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Ym";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Khy";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Khz";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Kcy";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Kcz";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Kcrit";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Kcr";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Kh_tension";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Kh_tension_LVL";


            //values
            xlApp.Range[kmodAdr].Formula = String.Format("=SDK.Factors.Kmod({0},{1},{2})", MaterialAdr, ServiceClassAdr, LoadDurationAdr);
            xlApp.Range[YmAdr].Formula = String.Format("=SDK.Factors.Ym({0})", MaterialAdr);
            xlApp.Range[khyAdr].Formula = String.Format("=SDK.Factors.Kh_Bending({0},{1})", MaterialAdr, hAdr);
            xlApp.Range[khzAdr].Formula = String.Format("=SDK.Factors.Kh_Bending({0},{1})", MaterialAdr, bAdr);
            xlApp.Range[kcyAdr].Formula = String.Format("=SDK.Factors.Kc({0},{1},{2},0,{3})", CSAdr, BuckLyAdr, BuckLzAdr, FireAdr);
            xlApp.Range[kczAdr].Formula = String.Format("=SDK.Factors.Kc({0},{1},{2},1,{3})", CSAdr, BuckLyAdr, BuckLzAdr, FireAdr);
            xlApp.Range[kcritAdr].Formula = String.Format("=SDK.Factors.Kcrit({0},{1},{2})", CSAdr, LTBAdr, FireAdr);
            xlApp.Range[kcrAdr].Formula = String.Format("=SDK.Factors.Kcr({0})", MaterialAdr);
            xlApp.Range[Kh_TensionAdr].Formula = String.Format("=SDK.Factors.Kh_Tension({0},{1},{2})", MaterialAdr, bAdr, hAdr);
            xlApp.Range[Kh_TensionLVLAdr].Formula = String.Format("=SDK.Factors.Kl_LVL({0},{1}*1000)", MaterialAdr, TensionLengthAdr);



            //Format
            baseCell.Offset[blockFactorStart, 4].Font.Bold = true;
            activeCell = baseCell.Offset[blockFactorStart + 1, 5];
            for (int i = 0; i < BlockFactorLength - 1; i++)
            {
                activeCell.Offset[i, 0].HorizontalAlignment = XlHAlign.xlHAlignRight;
                ((dynamic)activeCell.Offset[i, 0]).NumberFormatLocal = formatSeparator;
            }
            range = xlApp.Range[baseCell.Offset[blockFactorStart, 4], baseCell.Offset[blockFactorStart + BlockFactorLength - 1, 5]];
            range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;




            //----------------------------------
            //Define checks
            //----------------------------------

            //Captions
            activeCell = baseCell.Offset[0, 4]; activeCell.Value2 = "Eurocodes 5 DIN EN 1995-1 Checks";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Tension Parallel To Grain_6.1.2: ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Compression Parallel To Grain_6.1.4: ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Bending_6.1.6 : ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Bending And Tension_6.2.3 : ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Bending And Compression_6.2.4 : ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Bending And Buckling_6.3.2 : ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Lateral Torsional buckling_6.3.3";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Shear_6.1.7 : ";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Torsion_6.1.8 : ";

            //Value
            activeCell = baseCell.Offset[1, 5]; activeCell.Formula = string.Format("=SDK.CrossSectionChecks.TensionParallelToGrain_6.1.2({0},{1},{2},{3},{4},{5},{6})",
                SigNTenAdr, MaterialAdr, kmodAdr, YmAdr, Kh_TensionAdr, Kh_TensionLVLAdr, FireAdr);
			
			activeCell = activeCell.Offset[1, 0]; activeCell.Formula = string.Format("=SDK.CrossSectionChecks.CompressionParallelToGrain_6.1.4({0},{1},{2},{3},{4})",
                SigNCompAdr, MaterialAdr, kmodAdr, YmAdr, FireAdr);

            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = string.Format("=SDK.CrossSectionChecks.Bending_6.1.6({0},{1},{2},{3},{4},{5},{6},{7})",
                SigMyAdr, SigMzAdr, CSAdr, kmodAdr, YmAdr, khyAdr, khzAdr, FireAdr);

            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = string.Format("=SDK.CrossSectionChecks.BendingAndTension_6.2.3({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10})",
                SigMyAdr, SigMzAdr, SigNTenAdr, CSAdr, kmodAdr, YmAdr, khyAdr, khzAdr, Kh_TensionAdr, Kh_TensionLVLAdr, FireAdr);


			activeCell = activeCell.Offset[1, 0]; activeCell.Formula = string.Format("=SDK.CrossSectionChecks.BendingAndCompression_6.2.4({0},{1},{2},{3},{4},{5},{6},{7},{8})",
                SigMyAdr, SigMzAdr, SigNCompAdr, CSAdr, kmodAdr, YmAdr, khyAdr, khzAdr, FireAdr);

            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = string.Format("=SDK.CrossSectionChecks.BendingAndBuckling_6.3.2({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10})",
                SigMyAdr, SigMzAdr, SigNCompAdr, BuckLyAdr, BuckLzAdr, CSAdr, kmodAdr, YmAdr, khyAdr, khzAdr, FireAdr);

            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = string.Format("=SDK.CrossSectionChecks.LateralTorsionalBuckling_6.3.3({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11})",
                SigMyAdr, SigMzAdr, SigNCompAdr, BuckLyAdr, BuckLzAdr, LTBAdr, CSAdr, kmodAdr, YmAdr, khyAdr, khzAdr, FireAdr);

            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = string.Format("=SDK.CrossSectionChecks.Shear_6.1.7({0},{1},{2},{3},{4},{5})",
                TauYAdr, TauZAdr, MaterialAdr, kmodAdr, YmAdr, FireAdr);

            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = string.Format("=SDK.CrossSectionChecks.Torsion_6.1.8({0},{1},{2},{3},{4},{5},{6})",
                TauTorAdr, TauYAdr, TauZAdr, CSAdr, kmodAdr, YmAdr, FireAdr);


            //Format
            baseCell.Offset[0, 4].Font.Bold = true;
            activeCell = baseCell.Offset[0, 5];
            for (int i = 0; i < BlockCheckLength; i++)
            {
                activeCell.Offset[i, 0].HorizontalAlignment = XlHAlign.xlHAlignRight;
                ((dynamic)activeCell.Offset[i, 0]).NumberFormatLocal = formatPercent;
                FormatCondition format = (FormatCondition)(activeCell.Offset[i, 0]).FormatConditions.Add(XlFormatConditionType.xlCellValue,
                                       XlFormatConditionOperator.xlGreater, 1);

                format.Font.Bold = true;
                format.Interior.Color = XlRgbColor.rgbRed;
            }
            range = xlApp.Range[baseCell.Offset[0, 4], baseCell.Offset[BlockCheckLength - 1, 5]];
            range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;



            //----------------------------------
            //Material properties
            //----------------------------------

            activeCell = baseCell.Offset[0, 7]; activeCell.Value2 = "Material Properties";


            MaterialProperties(control, baseCell.Offset[0, 7], baseCell.Offset[1, 1]);

            //------------------------------
            //Global Formating
            //------------------------------
            int j = blockStressesStart + blockStressesLength;
            var range1 = xlApp.Range[baseCell, baseCell.Offset[j, 0]];
            //Autofit column width
            for (int i = 0; i < 9; i++)
            {
                range1.Offset[0, i].Columns.AutoFit();
            }

            //Cell merging for block titles


            xlApp.Range[baseCell.Offset[blockForceStart, 0], baseCell.Offset[blockForceStart, 2]].Merge();
            baseCell.Offset[blockForceStart, 0].HorizontalAlignment = XlHAlign.xlHAlignCenter;

            xlApp.Range[baseCell.Offset[blockStressesStart, 0], baseCell.Offset[blockStressesStart, 2]].Merge();
            baseCell.Offset[blockStressesStart, 0].HorizontalAlignment = XlHAlign.xlHAlignCenter;

            xlApp.Range[baseCell.Offset[blockFactorStart, 4], baseCell.Offset[blockFactorStart, 5]].Merge();
            baseCell.Offset[blockFactorStart, 4].HorizontalAlignment = XlHAlign.xlHAlignCenter;

            xlApp.Range[baseCell.Offset[0, 4], baseCell.Offset[0, 5]].Merge();
            baseCell.Offset[0, 4].HorizontalAlignment = XlHAlign.xlHAlignCenter;

            xlApp.Range[baseCell, baseCell.Offset[0, 2]].Merge();
            baseCell.HorizontalAlignment = XlHAlign.xlHAlignCenter;

        }


        /// <summary>
        /// Display all the material properties in a table 
        /// </summary>
        /// <param name="control">ribbon control</param>
        /// <param name="insertCell">cell where to insert the table. If null, book's active cell</param>
        /// <param name="csCell">first cell to look for the cross section. Format: b, h, Mat on top of each other</param>
        public void MaterialProperties(IRibbonControl control, Range insertCell = null, Range csCell = null)
        {
            var xlApp = (Excel.Application)ExcelDnaUtil.Application;
            ExcelHelpers.WorkBookOpen(xlApp); //Ensure a workbook is open

            var baseCell = xlApp.ActiveCell;
            var activeCell = xlApp.ActiveCell;

            if (insertCell != null)
            {
                baseCell = insertCell;
                activeCell = insertCell;
            }

            var properties = typeof(StructuralDesignKitLibrary.Materials.IMaterialTimber).GetProperties().ToList();

            activeCell.Value2 = "Material Properties";
            activeCell.Offset[1, 0].Value2 = "b";
            activeCell.Offset[2, 0].Value2 = "h";
            activeCell.Offset[3, 0].Value2 = "Material";
            activeCell.Offset[4, 0].Value2 = "Cross Section";

            activeCell.Offset[1, 1].Value2 = 100;
            activeCell.Offset[2, 1].Value2 = 200;
            RibbonUtilities.ValidateCellWithList(activeCell.Offset[3, 1], ExcelHelpers.AllMaterialAsList());
            activeCell.Offset[3, 1].Value2 = "GL24h";

            //Replace default values with linked values to a specific cross section:
            if (csCell != null)
            {
                activeCell.Offset[1, 1].Formula = string.Format("={0}", csCell.Address[false, false]);
                activeCell.Offset[2, 1].Formula = string.Format("={0}", csCell.Offset[1, 0].Address[false, false]);
                activeCell.Offset[3, 1].Formula = string.Format("={0}", csCell.Offset[2, 0].Address[false, false]);
            }

            activeCell.Offset[4, 1].Formula = string.Format("=SDK.Utilities.CreateRectangularCrossSection({0},{1},{2})",
                activeCell.Offset[1, 1].Address[false, false], activeCell.Offset[2, 1].Address[false, false], activeCell.Offset[3, 1].Address[false, false]);

            int count = 0;
            foreach (var prop in properties)
            {
                if (prop.Name != "Type")
                {
                    activeCell.Offset[5 + count, 0].Value2 = prop.Name;
                    activeCell.Offset[5 + count, 1].Formula = string.Format("=SDK.Material.Property({0}, \"{1}\")", activeCell.Offset[4, 1].Address[false, false], prop.Name);
                    count += 1;
                }

            }

            //Format
            baseCell.Font.Bold = true;
            for (int i = 0; i < properties.Count + 4; i++)
            {
                baseCell.Offset[i, 1].HorizontalAlignment = XlHAlign.xlHAlignRight;
            }

            for (int i = 0; i < 3; i++)
            {
                baseCell.Offset[i + 1, 1].Interior.Color = XlRgbColor.rgbLightYellow;
            }

            var range = xlApp.Range[baseCell, baseCell.Offset[properties.Count + 3, 1]];
            range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            range.Columns.AutoFit();

            xlApp.Range[baseCell, baseCell.Offset[0, 1]].Merge();
            baseCell.HorizontalAlignment = XlHAlign.xlHAlignCenter;


        }



        public void ValidateCellWithPlasterboardTypes(IRibbonControl control)
        {
            Excel.Application xlApp = (Excel.Application)ExcelDnaUtil.Application;
            ExcelHelpers.WorkBookOpen(xlApp); //Ensure a workbook is open

            RibbonActions.FireButtonActions.ValidateCellWithPlasterboardTypes(xlApp);
        }

        #region ConnectionButtons

        /// <summary>
        /// Validate a cell with all the fastener types currently available in the SDK
        /// </summary>
        /// <param name="control"></param>
        public void OnButtonPressedGetFastenerTypes(IRibbonControl control, Range insertCell = null)
        {
            Excel.Application xlApp = (Excel.Application)ExcelDnaUtil.Application;
            ExcelHelpers.WorkBookOpen(xlApp); //Ensure a workbook is open

            var availableFastenerTypes = Enum.GetNames(typeof(EC5_Utilities.FastenerType)).ToList();

            Range InsertCell = null;
            if (insertCell == null) InsertCell = xlApp.ActiveCell;
            else InsertCell = insertCell;

            RibbonUtilities.ValidateCellWithList(InsertCell, availableFastenerTypes);
        }

        public void OnButtonPressedMinimumSpacings(IRibbonControl control)
        {
            Excel.Application xlApp = (Excel.Application)ExcelDnaUtil.Application;
            ExcelHelpers.WorkBookOpen(xlApp); //Ensure a workbook is open

            var baseCell = xlApp.ActiveCell;
            var activeCell = baseCell;

            //Description
            activeCell.Value2 = "Minimum fastener spacings";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Fastener Type";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Diameter";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Angle";

            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "a1";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "a2";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "a3t";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "a3c";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "a4t";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "a4c";

            //Data
            Range fastenerTypeRange = baseCell.Offset[1, 1];
            Range DiameterRange = fastenerTypeRange.Offset[1, 0];
            Range AngleRange = DiameterRange.Offset[1, 0];
            OnButtonPressedGetFastenerTypes(control, fastenerTypeRange);
            DiameterRange.Value2 = 16;
            AngleRange.Value2 = 0;

            activeCell = AngleRange.Offset[1, 0]; activeCell.Formula = string.Format("=SDK.EC5.Connections.a1Min({0},{1},{2})",
                fastenerTypeRange.Address[false, false], DiameterRange.Address[false, false], AngleRange.Address[false, false]);
            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = string.Format("=SDK.EC5.Connections.a2Min({0},{1},{2})",
                fastenerTypeRange.Address[false, false], DiameterRange.Address[false, false], AngleRange.Address[false, false]);
            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = string.Format("=SDK.EC5.Connections.a3tMin({0},{1},{2})",
                fastenerTypeRange.Address[false, false], DiameterRange.Address[false, false], AngleRange.Address[false, false]);
            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = string.Format("=SDK.EC5.Connections.a3cMin({0},{1},{2})",
                fastenerTypeRange.Address[false, false], DiameterRange.Address[false, false], AngleRange.Address[false, false]);
            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = string.Format("=SDK.EC5.Connections.a4tMin({0},{1},{2})",
                 fastenerTypeRange.Address[false, false], DiameterRange.Address[false, false], AngleRange.Address[false, false]);
            activeCell = activeCell.Offset[1, 0]; activeCell.Formula = string.Format("=SDK.EC5.Connections.a4cMin({0},{1},{2})",
                fastenerTypeRange.Address[false, false], DiameterRange.Address[false, false], AngleRange.Address[false, false]);

            //Units
            activeCell = baseCell.Offset[2, 2]; activeCell.Value2 = "mm";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Deg";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "mm";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "mm";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "mm";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "mm";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "mm";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "mm";

            //Additional description
            activeCell = baseCell.Offset[4, 3]; activeCell.Value2 = "Minimum spacing parallel to grain";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Minimum spacing perpendicular to grain";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Minimum spacing to loaded end";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Minimum spacing to unloaded end";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Minimum spacing to loaded edge";
            activeCell = activeCell.Offset[1, 0]; activeCell.Value2 = "Minimum spacing to unloaded edge";


            //Formating
            baseCell.Font.Bold = true;

            for (int i = 0; i < 3; i++)
            {
                baseCell.Offset[i + 1, 1].Interior.Color = XlRgbColor.rgbLightYellow;
                baseCell.Offset[i + 1, 1].HorizontalAlignment = XlHAlign.xlHAlignRight;

            }

            for (int i = 0; i < 6; i++)
            {
                baseCell.Offset[4 + i, 1].HorizontalAlignment = XlHAlign.xlHAlignRight;
                ((dynamic)activeCell.Offset[4 + i, 1]).NumberFormatLocal = "0";
                baseCell.Offset[4 + i, 3].Font.Italic = true;

            }

            var range = xlApp.Range[baseCell, baseCell.Offset[9, 2]];
            var range1 = xlApp.Range[baseCell.Offset[1, 0], baseCell.Offset[9, 3]];
            range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            range1.Columns.AutoFit();

            for (int i = 0; i < 3; i++)
            {
                baseCell.Offset[0, i + 1].ColumnWidth += 2;
            }

            xlApp.Range[baseCell, baseCell.Offset[0, 2]].Merge();
            baseCell.HorizontalAlignment = XlHAlign.xlHAlignCenter;
        }

        public void OnButtonPressedEffectivefasteners(IRibbonControl control)
        {
            Excel.Application xlApp = (Excel.Application)ExcelDnaUtil.Application;
            ExcelHelpers.WorkBookOpen(xlApp); //Ensure a workbook is open

            ConnectionButtonActions.NeffButtonAction(xlApp);
        }

        #endregion


        #region Vibration

        public void OnButtonvelocity(IRibbonControl control)
        {
            Excel.Application xlApp = (Excel.Application)ExcelDnaUtil.Application;
            ExcelHelpers.WorkBookOpen(xlApp); //Ensure a workbook is open

            var baseCell = xlApp.ActiveCell;

            //List<double> velocity = Vibrations.ComputeVelocityResponseTimeSeries(1, 1, 1, 1, 1, 1,Vibrations.Weighting.None , 1, 1);

            //for (int i = 0; i < velocity.Count-1; i++)
            //{
            //    baseCell.Offset[i, 0].Value2 = velocity[i];
            //}

        }
        #endregion

        #region utilities



        private List<System.Reflection.MethodInfo> GetMethods(List<System.Reflection.MethodInfo> methods, string category)
        {
            //filtering Factors Methods
            List<System.Reflection.MethodInfo> methodsWithArgument = new List<System.Reflection.MethodInfo>();
            foreach (var method in methods)
            {
                if (method.CustomAttributes.ToList().Count >= 1)
                {
                    if (method.CustomAttributes.ToList()[0].NamedArguments.Count >= 3)
                    {
                        methodsWithArgument.Add(method);
                    }
                }
            }

            List<System.Reflection.MethodInfo> methodToReturns = new List<System.Reflection.MethodInfo>();
            foreach (var method in methodsWithArgument)
            {
                var namedArguments = method.CustomAttributes.ToList()[0].NamedArguments.Where(p => p.MemberName == "Category").ToList();
                if (namedArguments.Count>0)
                {
                    var namedArgument = namedArguments[0];
                    foreach(var argument in method.CustomAttributes.ToList()[0].NamedArguments.ToList())
                    {
                        if(argument.MemberName == "Category")
                        {
                            if(argument.TypedValue.Value.ToString() == category)
	
                            {
								methodToReturns.Add(method);
							}
						}
                    }
				}
            }
            //var val = methodsWithArgument[0].CustomAttributes.ToList()[0].NamedArguments[0].MemberName;
            //var methodToReturns = methodsWithArgument.Where(p => p.CustomAttributes.ToList()[0].NamedArguments[3].TypedValue.Value.ToString() == category).ToList();
            return methodToReturns;

		}


        public void OnButtonVersion(IRibbonControl control)
        {
            string currentVersion = "The current version is ";
            var ver = System.Reflection.Assembly.GetAssembly(typeof(StructuralDesignKitExcel.AddIn)).GetName().Version;

            MessageBox.Show(ver.ToString());
        }


        #endregion




    }
}
