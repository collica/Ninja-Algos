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
using System.Globalization;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.WorkingStrategies
{
	public class RenkoKing : Strategy
	{
		private ParabolicSAR ParabolicSAR1;
		private ADX ADX1;
		
		private double AvgPxLong;
		private double AvgPxShort;
		
		private bool[] EntryBarArray;
		private bool[] ExitBarArray;
		private bool   adhoc;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"The Renko Strategy as published in December 2019 Technical Analysis of Stocks and Commodities article titled 'Using Renko Charts' by John Devcic";
				Name										= "RenkoKing";
				Calculate									= Calculate.OnBarClose;
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
				BarsRequiredToTrade							= ADXPer;
				//
				Acceleration								= 0.02;
				AccelerationStep							= 0.02;
				AccelerationMax								= 0.05;
				
				ADXPer										= 14;
				ADXThreshold								= 30;
								

				EntryStrength								= 2;
				ExitStrength								= 3;
				AllowLong									= true;
				AllowShort									= true;
				UseExitStrength								= true;
				UseTrailStop								= true;
				
				StopLossValue								= 10;
				BE											= 1;
				
			}
			
			else if (State == State.Configure)
			{
				EntryBarArray = new bool[EntryStrength];
				for(int i = 0; i < EntryStrength; i++)
					EntryBarArray[i] = false;
				ExitBarArray = new bool[ExitStrength];
				for(int i = 0; i < ExitStrength; i++)
					ExitBarArray[i] = false;
				
				if(UseTrailStop)
					SetTrailStop(CalculationMode.Ticks, StopLossValue);
			}
			
						
			else if (State == State.DataLoaded)
			{				
				ParabolicSAR1								= ParabolicSAR(Close, Acceleration, AccelerationMax, AccelerationStep);
				ParabolicSAR1.Plots[0].Brush 				= Brushes.Goldenrod;
				AddChartIndicator(ParabolicSAR1);
				
				ADX1										= ADX(Close, Convert.ToInt32(ADXPer));
				ADX1.Plots[0].Brush 						= Brushes.DarkCyan;
				AddChartIndicator(ADX1);
			}
		}

		protected override void OnBarUpdate()
		{
			
			if(BarsPeriod.BarsPeriodType != BarsPeriodType.Renko)
			{
				Draw.TextFixed(this, "NinjaScriptInfo", "The RenkoKing must be ran on a Renko chart.", TextPosition.BottomRight);
				return;
			}
			
			if(CurrentBar < EntryStrength || CurrentBar < ExitStrength)
				return;
			
			// Entries
			if(Position.MarketPosition == MarketPosition.Flat)
			{
				for(int i = 0; i < EntryStrength; i++)
					EntryBarArray[i] = false;
				
				for(int i = EntryStrength-1; i >=0; i--)
				{
					if(Close[i] > Open[i])
					{
						EntryBarArray[i] = true;
					}
					else
					{
						EntryBarArray[i] = false;
					}
				}
				
				adhoc = true;
				foreach(bool b in EntryBarArray)
				{
					adhoc = adhoc && b;
				}
				
				if(adhoc 
					&& AllowLong
					//&& Close[0] > ParabolicSAR1[0]
					&& ADX1[0] > ADXThreshold
					&& IsRising(ADX1))
				{
					EnterLong("Long");
				}
				
				else
				{
					for(int i = 0; i < EntryStrength; i++)
						EntryBarArray[i] = false;
					
					for(int i = EntryStrength-1; i >=0; i--)
					{
						if(Close[i] < Open[i])
						{
							EntryBarArray[i] = true;
						}
						else
						{
							EntryBarArray[i] = false;
						}
					}
					
					adhoc = true;
					foreach(bool b in EntryBarArray)
					{
						adhoc = adhoc && b;
					}
					
					if(adhoc 
						&& AllowShort
						//&& Close[0] < ParabolicSAR1[0]
						&& ADX1[0] > ADXThreshold
						&& IsRising(ADX1))
					{
						EnterShort("Short");
					}
				}
				
			}
			
			// Exits
			for(int i = 0; i < ExitStrength; i++)
				ExitBarArray[i] = false;
			
			if(Position.MarketPosition == MarketPosition.Long && BarsSinceEntryExecution() >= 1)
			{
				for(int i = ExitStrength-1; i >=0; i--)
				{
					if(Close[i] < Open[i])
					{
						ExitBarArray[i] = true;
					}
				}
				
				adhoc = true;
				foreach(bool b in ExitBarArray)
				{
					adhoc = adhoc && b;
				}
				
				if(adhoc && UseExitStrength)
				{
					ExitLong();
				}
				
			}
			
			else if(Position.MarketPosition == MarketPosition.Short  && BarsSinceEntryExecution() >= 1)
			{
				for(int i = ExitStrength-1; i >=0; i--)
				{
					if(Close[i] > Open[i])
					{
						ExitBarArray[i] = true;
					}
				}
				
				adhoc = true;
				foreach(bool b in ExitBarArray)
				{
					adhoc = adhoc && b;
				}
				
				if(adhoc && UseExitStrength)
				{
					ExitShort();
				}
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
		
		protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
		{

		    if (order.Name == "Long")
		      	AvgPxLong = order.AverageFillPrice;
		  	if (order.Name == "Short")
		      	AvgPxShort = order.AverageFillPrice;
									
		}
		
//		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
//		{
//			if (Position.MarketPosition == MarketPosition.Long
//				&& GetCurrentBid() > AvgPxLong + (BE * TickSize))
//			{
//				ExitLongStopMarket(0, true, Convert.ToInt32(DefaultQuantity), AvgPxLong + (BE * TickSize), @"BE", @"Long");
//			}
//			
//
//			if (Position.MarketPosition == MarketPosition.Short
//				&& GetCurrentAsk() < AvgPxShort - (BE * TickSize))
//			{
//				ExitShortStopMarket(0, true, Convert.ToInt32(DefaultQuantity), AvgPxShort - (BE * TickSize), @"BE", @"Short");
//			}
 
//		}
		
		#region Properties
		[NinjaScriptProperty]
		[Range(0.01, double.MaxValue)]
		[Display(Name="Acceleration", Order=1, GroupName="SAR")]
		public double Acceleration
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.01, double.MaxValue)]
		[Display(Name="AccelerationStep", Order=2, GroupName="SAR")]
		public double AccelerationStep
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.01, double.MaxValue)]
		[Display(Name="AccelerationMax", Order=3, GroupName="SAR")]
		public double AccelerationMax
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="ADXPer", Order=2, GroupName="ADX")]
		public int ADXPer
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="ADXThreshold", Order=2, GroupName="ADX")]
		public int ADXThreshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="Stop Loss Value", Description="The value for the stop loss order.", Order=0, GroupName="Stop")]
		public double StopLossValue
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="BE", Description="BE Ticks", Order=1, GroupName="Stop")]
		public int BE
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Strength for entry", Description="The number of subseaquent up or down bars needed for an entry.", Order=2, GroupName="Renko")]
		public int EntryStrength
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Strength for exit", Description="The number of subseaquent up or down bars needed for an exit.", Order=3, GroupName="Renko")]
		public int ExitStrength
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Use Exit Strength", Description="Enable Exit Strength Exits", Order=4, GroupName="Renko")]
		public bool UseExitStrength
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Use Trail Stop", Description="Enable Trail Stop Exits", Order=5, GroupName="Stop")]
		public bool UseTrailStop
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Allow Long Trades", Description="Enable Long Trades", Order=6, GroupName="Renko")]
		public bool AllowLong
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Allow Short Trades", Description="Enable Short Trades", Order=7, GroupName="Renko")]
		public bool AllowShort
		{ get; set; }
		

  
		#endregion

	}
}
