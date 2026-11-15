// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;

namespace MissionArchitect
{
    /// <summary>Editable Mars rover design, shared by the terminal and the handbook.</summary>
    [Serializable]
    public sealed class MarsDesign
    {
        public int LaunchOffsetDays = 0;          // -60..+60
        public string VehicleId = "condor9";
        public string BusId = "large";
        public string MobilityId = "heavy";
        public bool RoboticArm = true;

        public string PowerType = "mmrtg";        // "mmrtg" | "solar"
        public double SolarAreaM2 = 4.0;
        public double BatteryWh = 500.0;
        public string ThermalId = "active";
        public bool Trn = true;

        public bool[] InstrumentEnabled = new bool[MarsCatalog.Instruments.Length];

        public string EdlSystemId = "skycrane";
        public string HeatShieldId = "large";
        public bool Parachute = true;
        public string SiteId = "delta";

        public bool RelayEnabled = true;
        public int RelayPasses = 2;
        public bool DirectToEarth = true;

        public string RadiationId = "mil";

        // Daily policy in percent; normalised by the model/planner.
        public int DrivePct = 20;
        public int SciencePct = 35;
        public int DrillPct = 10;
        public int CommsPct = 15;
        public int RestPct = 20;

        public int Version;

        public static MarsDesign CreateDefault()
        {
            var d = new MarsDesign();
            d.InstrumentEnabled[MarsCatalog.InstrumentIndex("mastcam")] = true;
            d.InstrumentEnabled[MarsCatalog.InstrumentIndex("supercam")] = true;
            d.InstrumentEnabled[MarsCatalog.InstrumentIndex("pixl")] = true;
            d.InstrumentEnabled[MarsCatalog.InstrumentIndex("sherloc")] = true;
            d.InstrumentEnabled[MarsCatalog.InstrumentIndex("rimfax")] = true;
            d.InstrumentEnabled[MarsCatalog.InstrumentIndex("meda")] = true;
            return d;
        }

        public void Touch() { Version++; }

        public bool IsSolar => PowerType == "solar";

        public int ActiveInstrumentCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < InstrumentEnabled.Length; i++) if (InstrumentEnabled[i]) n++;
                return n;
            }
        }

        public double InstrumentMassKg
        {
            get
            {
                double m = 0;
                for (int i = 0; i < InstrumentEnabled.Length; i++)
                    if (InstrumentEnabled[i]) m += MarsCatalog.Instruments[i].MassKg;
                return m;
            }
        }

        public double InstrumentPowerW
        {
            get
            {
                double p = 0;
                for (int i = 0; i < InstrumentEnabled.Length; i++)
                    if (InstrumentEnabled[i]) p += MarsCatalog.Instruments[i].PowerW;
                return p;
            }
        }

        public double InstrumentDataMbPerHour
        {
            get
            {
                double v = 0;
                for (int i = 0; i < InstrumentEnabled.Length; i++)
                    if (InstrumentEnabled[i]) v += MarsCatalog.Instruments[i].DataMbPerHour;
                return v;
            }
        }

        public double InstrumentCostM
        {
            get
            {
                double c = 0;
                for (int i = 0; i < InstrumentEnabled.Length; i++)
                    if (InstrumentEnabled[i]) c += MarsCatalog.Instruments[i].CostM;
                return c;
            }
        }

        public float InstrumentPoints
        {
            get
            {
                float p = 0;
                for (int i = 0; i < InstrumentEnabled.Length; i++)
                    if (InstrumentEnabled[i]) p += MarsCatalog.Instruments[i].Points;
                return p;
            }
        }

        public bool HasInstrument(string id)
        {
            int i = MarsCatalog.InstrumentIndex(id);
            return i >= 0 && i < InstrumentEnabled.Length && InstrumentEnabled[i];
        }

        public void SetInstrument(string id, bool on)
        {
            int i = MarsCatalog.InstrumentIndex(id);
            if (i < 0) return;
            InstrumentEnabled[i] = on;
            Touch();
        }

        public void ToggleInstrument(int index)
        {
            if (index < 0 || index >= InstrumentEnabled.Length) return;
            InstrumentEnabled[index] = !InstrumentEnabled[index];
            Touch();
        }

        // ---- daily policy -----------------------------------------------------

        public int PolicyTotal => DrivePct + SciencePct + DrillPct + CommsPct + RestPct;

        public double DriveWeight => Weight(DrivePct);
        public double ScienceWeight => Weight(SciencePct);
        public double DrillWeight => Weight(DrillPct);
        public double CommsWeight => Weight(CommsPct);
        public double RestWeight => Weight(RestPct);

        double Weight(int value)
        {
            int total = PolicyTotal;
            if (total <= 0) return 0.2;
            if (value < 0) value = 0;
            return value / (double)total;
        }

        public void NormalizePolicy()
        {
            int total = PolicyTotal;
            if (total <= 0)
            {
                DrivePct = 20; SciencePct = 35; DrillPct = 10; CommsPct = 15; RestPct = 20;
                return;
            }
            if (total == 100) return;

            DrivePct = (int)Math.Round(DrivePct * 100.0 / total);
            SciencePct = (int)Math.Round(SciencePct * 100.0 / total);
            DrillPct = (int)Math.Round(DrillPct * 100.0 / total);
            CommsPct = (int)Math.Round(CommsPct * 100.0 / total);
            RestPct = 100 - DrivePct - SciencePct - DrillPct - CommsPct;
            if (RestPct < 0) RestPct = 0;
        }

        public MarsDesign Clone()
        {
            var d = (MarsDesign)MemberwiseClone();
            d.InstrumentEnabled = (bool[])InstrumentEnabled.Clone();
            return d;
        }

        public void CopyFrom(MarsDesign other)
        {
            if (other == null) return;
            LaunchOffsetDays = other.LaunchOffsetDays;
            VehicleId = other.VehicleId;
            BusId = other.BusId;
            MobilityId = other.MobilityId;
            RoboticArm = other.RoboticArm;
            PowerType = other.PowerType;
            SolarAreaM2 = other.SolarAreaM2;
            BatteryWh = other.BatteryWh;
            ThermalId = other.ThermalId;
            Trn = other.Trn;
            InstrumentEnabled = (bool[])other.InstrumentEnabled.Clone();
            EdlSystemId = other.EdlSystemId;
            HeatShieldId = other.HeatShieldId;
            Parachute = other.Parachute;
            SiteId = other.SiteId;
            RelayEnabled = other.RelayEnabled;
            RelayPasses = other.RelayPasses;
            DirectToEarth = other.DirectToEarth;
            RadiationId = other.RadiationId;
            DrivePct = other.DrivePct;
            SciencePct = other.SciencePct;
            DrillPct = other.DrillPct;
            CommsPct = other.CommsPct;
            RestPct = other.RestPct;
            Touch();
        }

        public void ResetToDefault()
        {
            CopyFrom(CreateDefault());
        }
    }
}
