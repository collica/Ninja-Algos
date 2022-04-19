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
	public class SARxADX : Strategy
	{
		private ParabolicSAR ParabolicSAR1;
		private ParabolicSAR ParabolicSAR2;
		private ADX ADX1;
		
		private Order 	LongStop				= null; 
		private Order 	ShortStop				= null; 

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "SARxADX";
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
				
				//SAR
				Acceleration								= 0.02;
				AccelerationStep							= 0.2;
				AccelerationMax								= 0.02;
				
				//ADX
				ADXPer										= 14;
				ADXThreshold								= 30;
				
			}
			
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Minute, 15);
			}
			
			else if (State == State.DataLoaded)
			{				
				ParabolicSAR1								= ParabolicSAR(BarsArray[0], Acceleration, AccelerationMax, AccelerationStep);
				ParabolicSAR1.Plots[0].Brush 				= Brushes.Goldenrod;
				AddChartIndicator(ParabolicSAR1);
				
				ParabolicSAR2								= ParabolicSAR(BarsArray[1], Acceleration, AccelerationMax, AccelerationStep);
				ParabolicSAR2.Plots[0].Brush 				= Brushes.DarkCyan;
				AddChartIndicator(ParabolicSAR2);
				
				ADX1										= ADX(Close, Convert.ToInt32(ADXPer));
				ADX1.Plots[0].Brush 						= Brushes.DarkCyan;
				AddChartIndicator(ADX1);
			}
		}

		protected override void OnBarUpdate()
		{	
			if (CurrentBars[0] < 1 || CurrentBars[1] < 1)
				return;
			
			// Active hours
			if (ToTime(Time[0]) > 170000 || ToTime(Time[0]) < 145900)
			{
				// Enter long
				if ((Low[0] > ParabolicSAR1[0])
					&& (Lows[1][0] > ParabolicSAR2[0])
					&& (ADX1[0] > ADXThreshold)
					&& (IsRising(ADX1)))
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// Enter short
				if ((High[0] < ParabolicSAR1[0])
					&& (Highs[1][0] < ParabolicSAR2[0])
					&& (ADX1[0] > ADXThreshold)
					&& (IsRising(ADX1)))
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				
				// Update long stop
				if ((Position.MarketPosition == MarketPosition.Long)
					&& (LongStop != null)
					&& (LongStop.OrderState == OrderState.Accepted))
				{
					ChangeOrder(LongStop, LongStop.Quantity, 0, ParabolicSAR1[0]);
				}
				
				// Update short stop
				if ((Position.MarketPosition == MarketPosition.Short)
					&& (ShortStop != null)
					&& (ShortStop.OrderState == OrderState.Accepted))
				{
					ChangeOrder(ShortStop, ShortStop.Quantity, 0, ParabolicSAR1[0]);
				}
				
				// Exit long
				if ((Position.MarketPosition == MarketPosition.Long)
					&& (Close[0] < ParabolicSAR1[0]))
				{
					ExitLong();
				}
				
				// Exit short
				if ((Position.MarketPosition == MarketPosition.Short)
					&& (Close[0] > ParabolicSAR1[0]))
				{
					ExitShort();
				}
				
				if (Position.MarketPosition == MarketPosition.Long)
				{
					BackBrushAll = Brushes.MediumAquamarine;
				}
				
				if (Position.MarketPosition == MarketPosition.Short)
				{
					BackBrushAll = Brushes.Thistle;
				}
				
			}
			
			// Close only
			else if (ToTime(Time[0]) > 145900 && ToTime(Time[0]) < 170000)
			{
				
				if (Position.MarketPosition == MarketPosition.Long)
				{
					ExitLong();
				}
				
				if (Position.MarketPosition == MarketPosition.Short)
				{
					ExitShort();
				}
					
			}
			
		}
		
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			if (Position.MarketPosition == MarketPosition.Long)
			{
				ExitLongStopMarket(0, true, Convert.ToInt32(DefaultQuantity), ParabolicSAR1[0], "LongStop", "Long");
			}
				
			else if (Position.MarketPosition == MarketPosition.Short)
			{
				ExitShortStopMarket(0, true, Convert.ToInt32(DefaultQuantity), ParabolicSAR1[0], "ShortStop", "Short");
			}
			
		}
		
		protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
		{
	  	
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
		[Range(0, double.MaxValue)]
		[Display(Name="Acceleration", Order=1, GroupName="SAR")]
		public double Acceleration
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="AccelerationStep", Order=2, GroupName="SAR")]
		public double AccelerationStep
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="AccelerationMax", Order=3, GroupName="SAR")]
		public double AccelerationMax
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="ADXPer", Order=5, GroupName="ADX")]
		public int ADXPer
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="ADXThreshold", Order=6, GroupName="ADX")]
		public int ADXThreshold
		{ get; set; }

		#endregion

	}
}
