#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class Parabolic : Strategy
	{
		private ParabolicSAR ParabolicSAR1;
		private ParabolicSAR ParabolicSAR2;
		private ADX ADX1;
		private SMA SMA1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"https://www.tradingsetupsreview.com/ultimate-parabolic-sar-trading-guide/";
				Name										= "Parabolic";
				Calculate									= Calculate.OnEachTick;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Day;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 1;
				IsInstantiatedOnEachOptimizationIteration	= true;
				Acceleration1								= 0.02;
				AccelerationStep1							= 0.02;
				AccelerationMax1							= 0.05;
				
				Acceleration2								= 0.01;
				AccelerationStep2							= 0.01;
				AccelerationMax2							= 0.03;
				//ADXPer										= 14;
				
			}
			
			else if (State == State.Configure)
			{
				//AddDataSeries(Data.BarsPeriodType.Minute, 30);
			}
			
			else if (State == State.DataLoaded)
			{				
				ParabolicSAR1								= ParabolicSAR(Close, Acceleration1, AccelerationMax1, AccelerationStep1);
				ParabolicSAR1.Plots[0].Brush 				= Brushes.Goldenrod;
				AddChartIndicator(ParabolicSAR1);
				
				ParabolicSAR2								= ParabolicSAR(Close, Acceleration2, AccelerationMax2, AccelerationStep2);
				ParabolicSAR2.Plots[0].Brush 				= Brushes.DarkCyan;
				AddChartIndicator(ParabolicSAR2);
				
				//ADX1										= ADX(Close, Convert.ToInt32(ADXPer));
				//ADX1.Plots[0].Brush 						= Brushes.DarkCyan;
				//AddChartIndicator(ADX1);
				
				//Parabolic stop
				//SetParabolicStop("", CalculationMode.Price, Stop, false, Acceleration, AccelerationMax, AccelerationStep);
			}
		}

		protected override void OnBarUpdate()
		{
			//Determine intial position
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				if ((ParabolicSAR1[0] <= Low[0]) && (ParabolicSAR2[0] <= Low[0]))
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
			
				else if ((ParabolicSAR1[0] >= High[0]) && (ParabolicSAR2[0] >= High[0]))
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
			}
			
			//Stop and reverse signals
			if (Position.MarketPosition == MarketPosition.Long)
				
				if ((ParabolicSAR1[0] >= High[0]) && (ParabolicSAR2[0] >= High[0]))
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				else if ((ParabolicSAR1[0] >= High[0]) && (ParabolicSAR2[0] < High[0]))
				{
					ExitLong(Convert.ToInt32(DefaultQuantity), @"Stop", @"Long");
				}
			
			if (Position.MarketPosition == MarketPosition.Short)
				
				if ((ParabolicSAR1[0] <= Low[0]) && (ParabolicSAR2[0] <= Low[0]))
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				else if ((ParabolicSAR1[0] <= Low[0]) && (ParabolicSAR2[0] > Low[0]))
				{
					ExitShort(Convert.ToInt32(DefaultQuantity), @"Stop", @"Short");
				}
			
			
			
			
			
			
			
			
			//Highlighting
			if (Position.MarketPosition == MarketPosition.Long)
			{
				BackBrushAll = Brushes.MediumAquamarine;
			}
			
			if (Position.MarketPosition == MarketPosition.Short)
			{
				BackBrushAll = Brushes.Thistle;
			}

		}

		#region Properties
		[NinjaScriptProperty]
		[Range(0.01, double.MaxValue)]
		[Display(Name="Acceleration1", Order=1, GroupName="Parameters")]
		public double Acceleration1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.01, double.MaxValue)]
		[Display(Name="AccelerationStep1", Order=2, GroupName="Parameters")]
		public double AccelerationStep1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.01, double.MaxValue)]
		[Display(Name="AccelerationMax1", Order=3, GroupName="Parameters")]
		public double AccelerationMax1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.01, double.MaxValue)]
		[Display(Name="Acceleration2", Order=4, GroupName="Parameters")]
		public double Acceleration2
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.01, double.MaxValue)]
		[Display(Name="AccelerationStep2", Order=5, GroupName="Parameters")]
		public double AccelerationStep2
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.01, double.MaxValue)]
		[Display(Name="AccelerationMax2", Order=6, GroupName="Parameters")]
		public double AccelerationMax2
		{ get; set; }
		
//		[NinjaScriptProperty]
//		[Range(1, int.MaxValue)]
//		[Display(Name="ADXPer", Order=4, GroupName="Parameters")]
//		public int ADXPer
//		{ get; set; }
		#endregion

	}
}
