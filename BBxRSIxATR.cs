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
	public class BBxRSIxATR : Strategy
	{
		private RSI RSI1;
		private Bollinger Bollinger1;
		private ADX ADX1;
		private ATR ATR1;
		
		private Order 	LongStop				= null; 
		private Order 	ShortStop				= null; 
		private Order 	LongPT					= null;
		private Order 	ShortPT					= null;
		
		private double AvgPxLong;
		private double AvgPxShort;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "BBxRSIxATR";
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
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.IgnoreAllErrors;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				IsInstantiatedOnEachOptimizationIteration	= true;
				
				// BB
				Std											= 3;
				Per											= 14;

				// RSI
				OBThreshold									= 70;
				Smooth										= 3;
				
				// ADX
				ADXThreshold								= 30;
				
				// Exits
				ATRMult										= 3;

				
			}
			
			else if (State == State.Configure)
			{
			}
			
			else if (State == State.DataLoaded)
			{				
				RSI1										= RSI(Close, Per, Smooth);
				RSI1.Plots[0].Brush 						= Brushes.DodgerBlue;
				RSI1.Plots[1].Brush 						= Brushes.Goldenrod;
				AddChartIndicator(RSI1);
				
				Bollinger1									= Bollinger(Close, Std, Convert.ToInt32(Per));
				Bollinger1.Plots[0].Brush					= Brushes.Goldenrod;
				Bollinger1.Plots[1].Brush 					= Brushes.Goldenrod;
				Bollinger1.Plots[2].Brush 					= Brushes.Goldenrod;
				AddChartIndicator(Bollinger1);
				
				ADX1										= ADX(Close, Convert.ToInt32(Per));
				ADX1.Plots[0].Brush 						= Brushes.DarkCyan;
				AddChartIndicator(ADX1);
				
				ATR1										= ATR(Close, Per);
				ATR1.Plots[0].Brush 						= Brushes.DarkCyan;
				AddChartIndicator(ATR1);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;
			
			if (ToTime(Time[0]) > 170000 || ToTime(Time[0]) < 154000)
	
			 // Enter long
			if ((Close[0] < Bollinger1.Middle[0])
				&& RSI1[0] <= (100 - OBThreshold)
				&& ADX1[0] <= ADXThreshold)
			{
				EnterLongLimit(Convert.ToInt32(DefaultQuantity), Bollinger1.Lower[0], @"LowerBand");
			}
			
			// Enter short
			if ((Close[0] > Bollinger1.Middle[0])
				&& RSI1[0] >= OBThreshold
				&& ADX1[0] <= ADXThreshold)
			{
				EnterShortLimit(Convert.ToInt32(DefaultQuantity), Bollinger1.Upper[0], @"UpperBand");
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

		    if (order.Name == "LowerBand")
		      	AvgPxLong = order.AverageFillPrice;
		  	if (order.Name == "UpperBand")
		      	AvgPxShort = order.AverageFillPrice;
			
			if (order.Name == "LongStop")
				LongStop = order;
			if (order.Name == "ShortStop")
				ShortStop = order;
			if (order.Name == "LongPT")
				LongPT = order;
			if (order.Name == "ShortPT")
				ShortPT = order;
									
		}
		
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			if (Position.MarketPosition == MarketPosition.Long)
			{
				ExitLongStopMarket(0, true, Convert.ToInt32(DefaultQuantity), AvgPxLong - (ATR1[0] * ATRMult), "LongStop", "LowerBand");
				ExitLongLimit(0, true, Convert.ToInt32(DefaultQuantity), AvgPxLong + (ATR1[0] * ATRMult), @"LongPT", @"LowerBand");
			}
			
			else if (Position.MarketPosition == MarketPosition.Short)
			{
				ExitShortStopMarket(0, true, Convert.ToInt32(DefaultQuantity), AvgPxShort + (ATR1[0] * ATRMult), "ShortStop", "UpperBand");
				ExitShortLimit(0, true, Convert.ToInt32(DefaultQuantity), AvgPxShort - (ATR1[0] * ATRMult), @"ShortPT", @"UpperBand");
			}
		}
				
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="OBThreshold", Order=1, GroupName="Params")]
		public int OBThreshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Smooth", Order=3, GroupName="Params")]
		public int Smooth
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Std", Order=2, GroupName="Params")]
		public double Std
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Per", Order=3, GroupName="Params")]
		public int Per
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="ATRMult", Order=4, GroupName="Params")]
		public double ATRMult
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ADXThreshold", Order=5, GroupName="Params")]
		public int ADXThreshold
		{ get; set; }
		
		#endregion

	}
}
