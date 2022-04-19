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
	public class MTFxSARxADX : Strategy
	{
		private ParabolicSAR ParabolicSAR_Primary;
		private ParabolicSAR ParabolicSAR_3m;
		private ParabolicSAR ParabolicSAR_5m;
		private ParabolicSAR ParabolicSAR_10m;
		private ParabolicSAR ParabolicSAR_15m;
		
		private ADX ADX_Primary;
		private ADX ADX_3m;
		private ADX ADX_5m;
		private ADX ADX_10m;
		private ADX ADX_15m;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "MTFxSARxADX";
				Calculate									= Calculate.OnPriceChange;
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
				RealtimeErrorHandling						= RealtimeErrorHandling.IgnoreAllErrors;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				IsInstantiatedOnEachOptimizationIteration	= true;
				
				// Bools
				Use3m										= true;
				Use5m										= true;
				Use10m										= true;
				Use15m										= true;
				
				// ADX
				ADX_Per										= 15;
				ADX_Threshold								= 30;
				
			}
			
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Minute, 3); //Index 1
				AddDataSeries(Data.BarsPeriodType.Minute, 5); //Index 2
				AddDataSeries(Data.BarsPeriodType.Minute, 10); //Index 3
				AddDataSeries(Data.BarsPeriodType.Minute, 15); //Index 4
			}
			
			else if (State == State.DataLoaded)
			{				
				ParabolicSAR_Primary						= ParabolicSAR(BarsArray[0], 0.02, 0.2, 0.02);
				ParabolicSAR_3m								= ParabolicSAR(BarsArray[1], 0.02, 0.2, 0.02);
				ParabolicSAR_5m								= ParabolicSAR(BarsArray[2], 0.02, 0.2, 0.02);
				ParabolicSAR_10m							= ParabolicSAR(BarsArray[3], 0.02, 0.2, 0.02);
				ParabolicSAR_15m							= ParabolicSAR(BarsArray[4], 0.02, 0.2, 0.02);
				
				ParabolicSAR_Primary.Plots[0].Brush 		= Brushes.Goldenrod;
				AddChartIndicator(ParabolicSAR_Primary);
				
				ADX_Primary									= ADX(BarsArray[0], Convert.ToInt32(ADX_Per));
				ADX_3m										= ADX(BarsArray[1], Convert.ToInt32(ADX_Per));
				ADX_5m										= ADX(BarsArray[2], Convert.ToInt32(ADX_Per));
				ADX_10m										= ADX(BarsArray[3], Convert.ToInt32(ADX_Per));
				ADX_15m										= ADX(BarsArray[4], Convert.ToInt32(ADX_Per));
				
				ADX_Primary.Plots[0].Brush 					= Brushes.DarkCyan;
				AddChartIndicator(ADX_Primary);
			}
			
		}

		protected override void OnBarUpdate()
		{	
			if (CurrentBars[0] < BarsRequiredToTrade)
				return;
			
			if (CurrentBars[0] < 1 || CurrentBars[1] < 1 || CurrentBars[2] < 1 || CurrentBars[3] < 1 || CurrentBars[4] < 1)
				return;
			
			//Set trading hours
			TimeSpan StartTime = new TimeSpan(17, 00, 00);
			DateTime Start = DateTime.Today + StartTime;
			
			TimeSpan EndTime = new TimeSpan(14, 59, 50);
			DateTime End = DateTime.Today + EndTime;
			
			// Active hours
			if (DateTime.Now > Start || DateTime.Now < End)
			{	
					
				// // // // Enter long // // // //
				// 15 10 5 3 
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use15m) && Lows[4][0] > ParabolicSAR_15m[0] && ADX_15m[0] > ADX_Threshold
					&& (Use10m) && Lows[3][0] > ParabolicSAR_10m[0] && ADX_10m[0] > ADX_Threshold
					&& (Use5m) && Lows[2][0] > ParabolicSAR_5m[0] && ADX_5m[0] > ADX_Threshold
					&& (Use3m) && Lows[1][0] > ParabolicSAR_3m[0] && ADX_3m[0] > ADX_Threshold)
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 15 10 5  
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use15m) && Lows[4][0] > ParabolicSAR_15m[0] && ADX_15m[0] > ADX_Threshold
					&& (Use10m) && Lows[3][0] > ParabolicSAR_10m[0] && ADX_10m[0] > ADX_Threshold
					&& (Use5m) && Lows[2][0] > ParabolicSAR_5m[0] && ADX_5m[0] > ADX_Threshold)
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 15 10 3 
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use15m) && Lows[4][0] > ParabolicSAR_15m[0] && ADX_15m[0] > ADX_Threshold
					&& (Use10m) && Lows[3][0] > ParabolicSAR_10m[0] && ADX_10m[0] > ADX_Threshold
					&& (Use3m) && Lows[1][0] > ParabolicSAR_3m[0] && ADX_3m[0] > ADX_Threshold)
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 15 5 3  
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use15m) && Lows[4][0] > ParabolicSAR_15m[0] && ADX_15m[0] > ADX_Threshold
					&& (Use5m) && Lows[2][0] > ParabolicSAR_5m[0] && ADX_5m[0] > ADX_Threshold
					&& (Use3m) && Lows[1][0] > ParabolicSAR_3m[0] && ADX_3m[0] > ADX_Threshold)
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 15 5 
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use15m) && Lows[4][0] > ParabolicSAR_15m[0] && ADX_15m[0] > ADX_Threshold
					&& (Use5m) && Lows[2][0] > ParabolicSAR_5m[0] && ADX_5m[0] > ADX_Threshold)
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 15 3
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use15m) && Lows[4][0] > ParabolicSAR_15m[0] && ADX_15m[0] > ADX_Threshold
					&& (Use3m) && Lows[1][0] > ParabolicSAR_3m[0] && ADX_3m[0] > ADX_Threshold)
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 15 
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use15m) && Lows[4][0] > ParabolicSAR_15m[0] && ADX_15m[0] > ADX_Threshold)
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 10 5 3 
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use10m) && Lows[3][0] > ParabolicSAR_10m[0] && ADX_10m[0] > ADX_Threshold
					&& (Use5m) && Lows[2][0] > ParabolicSAR_5m[0] && ADX_5m[0] > ADX_Threshold
					&& (Use3m) && Lows[1][0] > ParabolicSAR_3m[0] && ADX_3m[0] > ADX_Threshold)
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 10 5  
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use10m) && Lows[3][0] > ParabolicSAR_10m[0] && ADX_10m[0] > ADX_Threshold
					&& (Use5m) && Lows[2][0] > ParabolicSAR_5m[0] && ADX_5m[0] > ADX_Threshold)
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 10 3  
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use10m) && Lows[3][0] > ParabolicSAR_10m[0] && ADX_10m[0] > ADX_Threshold
					&& (Use3m) && Lows[1][0] > ParabolicSAR_3m[0] && ADX_3m[0] > ADX_Threshold)
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 10
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use10m) && Lows[3][0] > ParabolicSAR_10m[0] && ADX_10m[0] > ADX_Threshold)
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 5 3
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use5m) && Lows[2][0] > ParabolicSAR_5m[0] && ADX_5m[0] > ADX_Threshold
					&& (Use3m) && Lows[1][0] > ParabolicSAR_3m[0] && ADX_3m[0] > ADX_Threshold)
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 5 
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use5m) && Lows[2][0] > ParabolicSAR_5m[0] && ADX_5m[0] > ADX_Threshold)
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// 3
				if (Position.MarketPosition == MarketPosition.Flat && Lows[0][0] > ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use3m) && Lows[1][0] > ParabolicSAR_3m[0] && ADX_3m[0] > ADX_Threshold)
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				
				// // // // Enter Short // // // //
				// 15 10 5 3 
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use15m) && Highs[4][0] < ParabolicSAR_15m[0] && ADX_15m[0] > ADX_Threshold
					&& (Use10m) && Highs[3][0] < ParabolicSAR_10m[0] && ADX_10m[0] > ADX_Threshold
					&& (Use5m) && Highs[2][0] < ParabolicSAR_5m[0] && ADX_5m[0] > ADX_Threshold
					&& (Use3m) && Highs[1][0] < ParabolicSAR_3m[0] && ADX_3m[0] > ADX_Threshold)
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 15 10 5  
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use15m) && Highs[4][0] < ParabolicSAR_15m[0] && ADX_15m[0] > ADX_Threshold
					&& (Use10m) && Highs[3][0] < ParabolicSAR_10m[0] && ADX_10m[0] > ADX_Threshold
					&& (Use5m) && Highs[2][0] < ParabolicSAR_5m[0] && ADX_5m[0] > ADX_Threshold)
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 15 10 3 
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use15m) && Highs[4][0] < ParabolicSAR_15m[0] && ADX_15m[0] > ADX_Threshold
					&& (Use10m) && Highs[3][0] < ParabolicSAR_10m[0] && ADX_10m[0] > ADX_Threshold
					&& (Use3m) && Highs[1][0] < ParabolicSAR_3m[0] && ADX_3m[0] > ADX_Threshold)
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 15 5 3  
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use15m) && Highs[4][0] < ParabolicSAR_15m[0] && ADX_15m[0] > ADX_Threshold
					&& (Use5m) && Highs[2][0] < ParabolicSAR_5m[0] && ADX_5m[0] > ADX_Threshold
					&& (Use3m) && Highs[1][0] < ParabolicSAR_3m[0] && ADX_3m[0] > ADX_Threshold)
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 15 5 
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use15m) && Highs[4][0] < ParabolicSAR_15m[0] && ADX_15m[0] > ADX_Threshold
					&& (Use5m) && Highs[2][0] < ParabolicSAR_5m[0] && ADX_5m[0] > ADX_Threshold)
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 15 3
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use15m) && Highs[4][0] < ParabolicSAR_15m[0] && ADX_15m[0] > ADX_Threshold
					&& (Use3m) && Highs[1][0] < ParabolicSAR_3m[0] && ADX_3m[0] > ADX_Threshold)
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 15 
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use15m) && Highs[4][0] < ParabolicSAR_15m[0] && ADX_15m[0] > ADX_Threshold)
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 10 5 3 
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use10m) && Highs[3][0] < ParabolicSAR_10m[0] && ADX_10m[0] > ADX_Threshold
					&& (Use5m) && Highs[2][0] < ParabolicSAR_5m[0] && ADX_5m[0] > ADX_Threshold
					&& (Use3m) && Highs[1][0] < ParabolicSAR_3m[0] && ADX_3m[0] > ADX_Threshold)
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 10 5  
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use10m) && Highs[3][0] < ParabolicSAR_10m[0] && ADX_10m[0] > ADX_Threshold
					&& (Use5m) && Highs[2][0] < ParabolicSAR_5m[0] && ADX_5m[0] > ADX_Threshold)
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 10 3  
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use10m) && Highs[3][0] < ParabolicSAR_10m[0] && ADX_10m[0] > ADX_Threshold 
					&& (Use3m) && Highs[1][0] < ParabolicSAR_3m[0] && ADX_3m[0] > ADX_Threshold)
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 10
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use10m) && Highs[3][0] < ParabolicSAR_10m[0] && ADX_10m[0] > ADX_Threshold)
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 5 3
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use5m) && Highs[2][0] < ParabolicSAR_5m[0] && ADX_5m[0] > ADX_Threshold 
					&& (Use3m) && Highs[1][0] < ParabolicSAR_3m[0] && ADX_3m[0] > ADX_Threshold)
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 5 
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use5m) && Highs[2][0] < ParabolicSAR_5m[0] && ADX_5m[0] > ADX_Threshold)
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// 3
				if (Position.MarketPosition == MarketPosition.Flat && Highs[0][0] < ParabolicSAR_Primary[0] && ADX_Primary[0] > ADX_Threshold
					&& (Use3m) && Highs[1][0] < ParabolicSAR_3m[0] && ADX_3m[0] > ADX_Threshold)
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
					
				// // // // Exits // // // //
			
				//Exit long 
				if ((Position.MarketPosition == MarketPosition.Long)
					&& (Lows[0][0] <= ParabolicSAR_Primary[0]))
				{
					ExitLong();
					Print("Exit long (Normal)");
					Print("PSAR: " + ParabolicSAR_Primary[0]);
					Print("Low: " + Lows[0][0]);
					
				}
				
				//Exit short
				if ((Position.MarketPosition == MarketPosition.Short)
					&& (Highs[0][0] >= ParabolicSAR_Primary[0]))
				{
					ExitShort();
					Print("Exit short (Normal)");
					Print("PSAR: " + ParabolicSAR_Primary[0]);
					Print("High: " + Highs[0][0]);
				}
				
			}
			
			// Close only
			if (DateTime.Now < Start && DateTime.Now > End)
			{	
				if (Position.MarketPosition == MarketPosition.Long)
				{
					ExitLong();
					Print("Exit EOD");
					Print(DateTime.Now.ToString("HHmmss"));
				}
				
				if (Position.MarketPosition == MarketPosition.Short)
				{
					ExitShort();
					Print("Exit EOD");
					Print(DateTime.Now.ToString("HHmmss"));
				}	
			}
			
			// Highlighting	
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
		[Display(Name="Use3m", Order=2, GroupName="MTF")]
		public bool Use3m
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Use5m", Order=3, GroupName="MTF")]
		public bool Use5m
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Use10m", Order=4, GroupName="MTF")]
		public bool Use10m
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Use15m", Order=5, GroupName="MTF")]
		public bool Use15m
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="ADX_Per", Order=1, GroupName="ADX")]
		public int ADX_Per
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="ADX_Threshold", Order=2, GroupName="ADX")]
		public int ADX_Threshold
		{ get; set; }
		
		#endregion

	}
}


				
				
				
//				// Enter long
//				if ((Low[0] > ParabolicSAR_Primary[0])
					
//					&& 
					
//					  ((((Use15m) && Lows[4][0] > ParabolicSAR_15m[0]) && ((Use10m) && Lows[3][0] > ParabolicSAR_10m[0]) && ((Use5m) && Lows[2][0] > ParabolicSAR_5m[0]) && ((Use3m) && Lows[1][0] > ParabolicSAR_3m[0])) 
//					|| (((Use15m) && Lows[4][0] > ParabolicSAR_15m[0]) && ((Use10m) && Lows[3][0] > ParabolicSAR_10m[0]) && ((Use5m) && Lows[2][0] > ParabolicSAR_5m[0])) 
//					|| (((Use15m) && Lows[4][0] > ParabolicSAR_15m[0]) && ((Use10m) && Lows[3][0] > ParabolicSAR_10m[0]) && ((Use3m) && Lows[1][0] > ParabolicSAR_3m[0])) 
//					|| (((Use15m) && Lows[4][0] > ParabolicSAR_15m[0]) && ((Use5m) && Lows[2][0] > ParabolicSAR_5m[0]) && ((Use3m) && Lows[1][0] > ParabolicSAR_3m[0]))
//					|| (((Use15m) && Lows[4][0] > ParabolicSAR_15m[0]) && ((Use5m) && Lows[2][0] > ParabolicSAR_5m[0]))
//					|| (((Use15m) && Lows[4][0] > ParabolicSAR_15m[0]) && ((Use3m) && Lows[1][0] > ParabolicSAR_3m[0]))
//					|| (((Use15m) && Lows[4][0] > ParabolicSAR_15m[0]))
//					|| (((Use10m) && Lows[3][0] > ParabolicSAR_10m[0]) && ((Use5m) && Lows[2][0] > ParabolicSAR_5m[0]) && ((Use3m) && Lows[1][0] > ParabolicSAR_3m[0])) 
//					|| (((Use10m) && Lows[3][0] > ParabolicSAR_10m[0]) && ((Use5m) && Lows[2][0] > ParabolicSAR_5m[0])) 
//					|| (((Use10m) && Lows[3][0] > ParabolicSAR_10m[0]) && ((Use3m) && Lows[1][0] > ParabolicSAR_3m[0])) 
//					|| (((Use10m) && Lows[3][0] > ParabolicSAR_10m[0]))
//					|| (((Use5m) && Lows[2][0] > ParabolicSAR_5m[0]) && ((Use3m) && Lows[1][0] > ParabolicSAR_3m[0])) 
//					|| (((Use5m) && Lows[2][0] > ParabolicSAR_5m[0]))
//					|| (((Use3m) && Lows[1][0] > ParabolicSAR_3m[0]))))
					

//				{
//					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
//				}
				
//				// Enter short
//				if ((High[0] < ParabolicSAR_Primary[0])
					
//					&& 
					
//					  ((((Use15m) && High[0] < ParabolicSAR_15m[0]) && ((Use10m) && High[0] < ParabolicSAR_10m[0]) && ((Use5m) && High[0] < ParabolicSAR_5m[0]) && ((Use3m) && High[0] < ParabolicSAR_3m[0])) 
//					|| (((Use15m) && High[0] < ParabolicSAR_15m[0]) && ((Use10m) && High[0] < ParabolicSAR_10m[0]) && ((Use5m) && High[0] < ParabolicSAR_5m[0])) 
//					|| (((Use15m) && High[0] < ParabolicSAR_15m[0]) && ((Use10m) && High[0] < ParabolicSAR_10m[0]) && ((Use3m) && High[0] < ParabolicSAR_3m[0])) 
//					|| (((Use15m) && High[0] < ParabolicSAR_15m[0]) && ((Use5m) && High[0] < ParabolicSAR_5m[0]) && ((Use3m) && High[0] < ParabolicSAR_3m[0]))
//					|| (((Use15m) && High[0] < ParabolicSAR_15m[0]) && ((Use5m) && High[0] < ParabolicSAR_5m[0]))
//					|| (((Use15m) && High[0] < ParabolicSAR_15m[0]) && ((Use3m) && High[0] < ParabolicSAR_3m[0]))
//					|| (((Use15m) && High[0] < ParabolicSAR_15m[0]))
//					|| (((Use10m) && High[0] < ParabolicSAR_10m[0]) && ((Use5m) && High[0] < ParabolicSAR_5m[0]) && ((Use3m) && High[0] < ParabolicSAR_3m[0])) 
//					|| (((Use10m) && High[0] < ParabolicSAR_10m[0]) && ((Use5m) && High[0] < ParabolicSAR_5m[0])) 
//					|| (((Use10m) && High[0] < ParabolicSAR_10m[0]) && ((Use3m) && High[0] < ParabolicSAR_3m[0])) 
//					|| (((Use10m) && High[0] < ParabolicSAR_10m[0]))
//					|| (((Use5m) && High[0] < ParabolicSAR_5m[0]) && ((Use3m) && High[0] < ParabolicSAR_3m[0])) 
//					|| (((Use5m) && High[0] < ParabolicSAR_5m[0]))
//					|| (((Use3m) && High[0] < ParabolicSAR_3m[0]))))
					

//				{
//					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
//				}
				
				
				
				// Enter long
//				if ((Low[0] > ParabolicSAR_Primary[0])
//					&& (Lows[4][0] > ParabolicSAR_15m[0])
//					&& (Lows[3][0] > ParabolicSAR_10m[0])
//					&& (Lows[2][0] > ParabolicSAR_5m[0])
//					&& (Lows[1][0] > ParabolicSAR_3m[0]))
//				{
//					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
//				}
											
				// Enter short
//				if ((High[0] < ParabolicSAR_Primary[0])
//					&& (High[0] < ParabolicSAR_15m[0])
//					&& (High[0] < ParabolicSAR_10m[0])
//					&& (High[0] < ParabolicSAR_5m[0])
//					&& (High[0] < ParabolicSAR_3m[0]))
//				{
//					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
//				}