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
	public class RSIxOBV : Strategy
	{
		private OBV OBV1;
		private OrderFlowCumulativeDelta OrderFlowCumulativeDelta1;
		
		private RSI RSI_OBV;
		private RSI RSI_OFCD;
		
		private Order 	LongPT					= null;
		private Order 	ShortPT					= null;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "RSIxOBV";
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
				//
				OBThreshold									= 80;
				TPThreshold									= 50;
				Per											= 14;
				
			}
			
			else if (State == State.Configure)
			{
			}
			
			else if (State == State.DataLoaded)
			{				
				OBV1										= OBV(Close);
				OBV1.Plots[0].Brush 						= Brushes.Goldenrod;
				//AddChartIndicator(OBV1);
				
				OrderFlowCumulativeDelta1					= OrderFlowCumulativeDelta(Close, NinjaTrader.NinjaScript.Indicators.CumulativeDeltaType.BidAsk, NinjaTrader.NinjaScript.Indicators.CumulativeDeltaPeriod.Session, 0);
				OrderFlowCumulativeDelta1.Plots[3].Brush 	= Brushes.White;
				//AddChartIndicator(OrderFlowCumulativeDelta1);
				
				RSI_OBV										= RSI(OBV1, Convert.ToInt32(Per), 3);
				RSI_OBV.Plots[0].Brush 						= Brushes.DodgerBlue;
				AddChartIndicator(RSI_OBV);
				
				RSI_OFCD									= RSI(OrderFlowCumulativeDelta1.DeltaClose, Convert.ToInt32(Per), 3);
				RSI_OBV.Plots[0].Brush 						= Brushes.DodgerBlue;
				AddChartIndicator(RSI_OFCD);
				
			}
		}

		protected override void OnBarUpdate()
		{
			
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;

			// Active hours
			if (ToTime(Time[0]) > 170000 || ToTime(Time[0]) < 145900)
			{
				// Enter long
				if (RSI_OBV.Default[0] < (100 - OBThreshold)
					&& RSI_OFCD.Default[0] < (100 - OBThreshold))
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
				}
				
				// Enter short
				if ((RSI_OBV.Default[0] > OBThreshold)
					&& (RSI_OFCD.Default[0] > OBThreshold))
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
				}
				
				// Exit long
				if ((Position.MarketPosition == MarketPosition.Long)
					&& ((CrossAbove(RSI_OBV.Default, (100 - TPThreshold), 1)) || (CrossAbove(RSI_OFCD.Default, (100 - TPThreshold), 1))))
				{
					ExitLong();
				}
				
				// Exit short
				if ((Position.MarketPosition == MarketPosition.Short)
					&& ((CrossBelow(RSI_OBV.Default, TPThreshold, 1)) || (CrossBelow(RSI_OFCD.Default, TPThreshold, 1))))
				{
					ExitShort();	
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

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="OBThreshold", Order=1, GroupName="Parameters")]
		public int OBThreshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TPThreshold", Order=2, GroupName="Parameters")]
		public int TPThreshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Per", Order=3, GroupName="Parameters")]
		public int Per
		{ get; set; }
		#endregion

	}
}
