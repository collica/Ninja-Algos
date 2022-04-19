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
namespace NinjaTrader.NinjaScript.Strategies.WorkingStrategies
{
	public class SpreadScalper : Strategy
	{
		private OIR 	OIR1;
		private Order 	Long			= null; 
		private Order 	Short 			= null; 
		private double  CurrentSpread;
		private double  LongLimit;
		private double  ShortLimit;
		private double 	AvgPxLong;
		private double 	AvgPxShort;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Wait until spread and imbalance/spread conditions signal an entry, attempt to scalp 1 tick";
				Name										= "SpreadScalper";
				Calculate									= Calculate.OnEachTick;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.High;
				OrderFillResolutionType						= BarsPeriodType.Tick;
				OrderFillResolutionValue					= 1;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Day;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 1;
				IsInstantiatedOnEachOptimizationIteration	= true;
				//
				MinSpread									= 4;
				OIR_Threshold								= .5;
				Offset										= 1;
				PT											= 1;
				Stop										= 10;
				
			}
			
			else if (State == State.Configure)
			{
				//Add tick data series for backtesting
				//AddDataSeries(Data.BarsPeriodType.Tick, 1);
			}
			
			else if (State == State.DataLoaded)
			{				
				OIR1										= OIR();
				OIR1.Plots[0].Brush 						= Brushes.DarkCyan;
				AddChartIndicator(OIR1);
			}
			
		}
		
			protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
			{
		  	
				if (order.Name == "Long")
			      	Long = order;
				if (order.Name == "Short")
					Short = order;
				
				if (order.Name == "Long")
					AvgPxLong = order.AverageFillPrice;
				if (order.Name == "Short")
					AvgPxShort = order.AverageFillPrice;
			}
			
			protected override void OnMarketDepth(MarketDepthEventArgs marketDepthUpdate)
			{
				
				CurrentSpread = (GetCurrentAsk() - GetCurrentBid());
				LongLimit = (GetCurrentBid() + (Offset * TickSize));
				ShortLimit = (GetCurrentAsk() + (-Offset * TickSize));
					
				if (Position.MarketPosition == MarketPosition.Flat)
					
					// Long
					if ((CurrentSpread  >= (MinSpread * TickSize)
						&& ((OIR1[0] > OIR_Threshold))))
				  
					{
						EnterLongLimit(0, true, Convert.ToInt32(DefaultQuantity), LongLimit, @"Long");
						//Print("OIR: " + OIR1[0]);
					}
					
					// Short
					else if ((CurrentSpread >= (MinSpread * TickSize)
						&& ((OIR1[0] < -OIR_Threshold))))
				  
					{
						EnterShortLimit(0, true, Convert.ToInt32(DefaultQuantity), ShortLimit, @"Short");
						//Print("OIR: " + OIR1[0]);
					}
				
				// Stop long
				if ((Position.MarketPosition == MarketPosition.Long)
					&& GetCurrentAsk() <= AvgPxLong)
				{
					ExitLong(0, @"Stop", @"Long");
				}	
				
				// Stop short
				if ((Position.MarketPosition == MarketPosition.Short)
					&& GetCurrentBid() >= AvgPxShort)
				{
					ExitShort(0, @"Stop", @"Short");
				}	
					
					
				// Cancel long
				if ((Long != null)
					&& (Long.OrderState == OrderState.Working)
					&& (LongLimit < GetCurrentBid()))
				{
					CancelOrder(Long);
					Print(DateTime.Now.ToString("hh:mm:ss") + " -- Cancel, limit away");
				}
				
				if ((Long != null)
					&& (Long.OrderState == OrderState.Working)
					&& (OIR1[0] < OIR_Threshold))
				{
					CancelOrder(Long);
					Print(DateTime.Now.ToString("hh:mm:ss") + " -- Cancel, OIR no longer valid");
				}
				
				// Cancel short
				if ((Short != null)
					&& (Short.OrderState == OrderState.Working)
					&& (ShortLimit > GetCurrentAsk()))
				{
					CancelOrder(Short);
					Print(DateTime.Now.ToString("hh:mm:ss") + " -- Cancel, limit away");
				}
				
				if ((Short != null)
					&& (Short.OrderState == OrderState.Working)
					&& (OIR1[0] > -OIR_Threshold))
				{
					CancelOrder(Short);
					Print(DateTime.Now.ToString("hh:mm:ss") + " -- Cancel, OIR no longer valid");
				}
							
			}
			
			protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
			{
			
				// PT long
				if (Position.MarketPosition == MarketPosition.Long)
				{
					ExitLongLimit(0, true, Convert.ToInt32(DefaultQuantity), AvgPxLong + (PT * TickSize), @"PT", @"Long");
				}			
								
				// PT short
				if (Position.MarketPosition == MarketPosition.Short)
				{
					ExitShortLimit(0, true, Convert.ToInt32(DefaultQuantity), AvgPxShort - (PT * TickSize), @"PT", @"Short");
				}
			
			}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MinSpread", Order=1, GroupName="Parameters")]
		public int MinSpread
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="OIR_Threshold", Order=2, GroupName="Parameters")]
		public double OIR_Threshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Offset", Order=3, GroupName="Parameters")]
		public int Offset
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PT", Order=4, GroupName="Parameters")]
		public int PT
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Stop", Order=5, GroupName="Parameters")]
		public int Stop
		{ get; set; }
		#endregion

	}
}