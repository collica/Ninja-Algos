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
	public class EMAxADXxATR : Strategy
	{
		private EMA EMA1;
		private ADX ADX1;
		private ATR ATR1;
		
		private double 	AvgPxLong;
		private double 	AvgPxShort;
		
		private double MFELong;
		private double MFEShort;
		
		private Order 	LongStop				= null; 
		private Order 	ShortStop				= null; 

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "EMAxADXxATR";
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
				
				// General
				ADXThreshold								= 30;
				Per											= 14;
				EMASlope									= 1.0;
				Lookback									= 1;
				ATRMult										= 2;
				
			}
			
			else if (State == State.Configure)
			{
			}
			
			else if (State == State.DataLoaded)
			{				
				EMA1										= EMA(Close, Convert.ToInt32(Per));
				EMA1.Plots[0].Brush 						= Brushes.Goldenrod;
				AddChartIndicator(EMA1);
				
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
			if ((Position.MarketPosition == MarketPosition.Flat)
				&& (Close[0] > EMA1[0])
				&& (Slope(EMA1, Lookback, 0) > EMASlope)
				&& (IsRising(ADX1))
				&& (ADX1[0] > ADXThreshold))
				//&& (Slope(ADX1, Lookback, 0) > ADXSlope))
			{
				EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				Print(DateTime.Now.ToString("hh:mm:ss") + @" -- EMAxADX -- EMA Slope: " + Slope(EMA1, Lookback, 0));
			}
			
			 // Enter short
			if ((Position.MarketPosition == MarketPosition.Flat)
				&& (Close[0] < EMA1[0])
				&& (Slope(EMA1, Lookback, 0) < -EMASlope)
				&& (IsRising(ADX1))
				&& (ADX1[0] > ADXThreshold))
				//&& (Slope(ADX1, Lookback, 0) > ADXSlope))
			{
				EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				Print(DateTime.Now.ToString("hh:mm:ss") + @" -- EMAxADX -- EMA Slope: " + Slope(EMA1, Lookback, 0));
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
			
//			// Update long stop
//			if ((Position.MarketPosition == MarketPosition.Long)
//				&& (LongStop != null)
//				&& (LongStop.OrderState == OrderState.Accepted))
//			{
//				ChangeOrder(LongStop, LongStop.Quantity, 0, EMA1[0]);
//				BackBrushAll = Brushes.MediumAquamarine;
//			}
			
//			// Update short stop
//			if ((Position.MarketPosition == MarketPosition.Short)
//				&& (ShortStop != null)
//				&& (ShortStop.OrderState == OrderState.Accepted))
//			{
//				ChangeOrder(ShortStop, ShortStop.Quantity, 0, EMA1[0]);
//				BackBrushAll = Brushes.Thistle;
//			}
			
		}
		
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			if (Position.MarketPosition == MarketPosition.Long)
			{
				ExitLongStopMarket(0, true, Convert.ToInt32(DefaultQuantity), AvgPxLong - (ATR1[0] * ATRMult), "LongStop", "Long");
				ExitLongLimit(0, true, Convert.ToInt32(DefaultQuantity), AvgPxLong + (ATR1[0] * ATRMult), @"LongPT", @"Long");
			}
				
			if (Position.MarketPosition == MarketPosition.Short)
			{
				ExitShortStopMarket(0, true, Convert.ToInt32(DefaultQuantity), AvgPxShort + (ATR1[0] * ATRMult), "ShortStop", "Short");
				ExitShortLimit(0, true, Convert.ToInt32(DefaultQuantity), AvgPxShort - (ATR1[0] * ATRMult), @"ShortPT", @"Short");
			}
			
		}
		
		protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
		{

		    if (order.Name == "Long")
		      	AvgPxLong = order.AverageFillPrice;
		  	if (order.Name == "Short")
		      	AvgPxShort = order.AverageFillPrice;
		
	  	
			if (order.Name == "LongStop")
		      	LongStop = order;
			if (order.Name == "ShortStop")
				ShortStop = order;

			if (LongStop != null
				&& LongStop.OrderState == OrderState.Rejected)
			{
				ExitLong();
			}
			
			if (ShortStop != null
				&& ShortStop.OrderState == OrderState.Rejected)
			{
			    ExitShort();
			}
			  
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Per", Order=1, GroupName="Params")]
		public int Per
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ADXThreshold", Order=2, GroupName="Params")]
		public int ADXThreshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="EMASlope", Order=3, GroupName="Params")]
		public double EMASlope
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Lookback", Order=4, GroupName="Params")]
		public int Lookback
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="ATRMult", Order=5, GroupName="Params")]
		public double ATRMult
		{ get; set; }
		
		#endregion

	}
}
