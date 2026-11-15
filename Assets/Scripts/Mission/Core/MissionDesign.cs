// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;

namespace MissionArchitect
{
    /// <summary>
    /// The editable spacecraft design. Plain data so both the terminal UI and the
    /// handbook read the very same object (no duplicated logic, spec section 3).
    /// </summary>
    [Serializable]
    public sealed class MissionDesign
    {
        public string VehicleId = "condor5";
        public string PropulsionId = "biprop";
        public double ArrayAreaM2 = 4.0;
        public double BatteryWh = 800.0;
        public string AntennaId = "x10";
        public double RfPowerW = 50.0;
        public string GroundStationId = "uni";
        public string RadiationId = "mil";
        public bool Redundancy = false;

        /// <summary>Enabled flags, parallel to MissionCatalog.Instruments.</summary>
        public bool[] InstrumentEnabled = new bool[MissionCatalog.Instruments.Length];

        /// <summary>Bumped on every edit so UIs can cheaply detect a change.</summary>
        public int Version;

        public static MissionDesign CreateDefault()
        {
            var d = new MissionDesign();
            d.InstrumentEnabled[MissionCatalog.InstrumentIndex("nac")] = true;
            d.InstrumentEnabled[MissionCatalog.InstrumentIndex("msi")] = true;
            d.InstrumentEnabled[MissionCatalog.InstrumentIndex("ns")] = true;
            return d;
        }

        public void Touch() { Version++; }

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
                double m = 0.0;
                for (int i = 0; i < InstrumentEnabled.Length; i++)
                    if (InstrumentEnabled[i]) m += MissionCatalog.Instruments[i].MassKg;
                return m;
            }
        }

        public double InstrumentPowerW
        {
            get
            {
                double p = 0.0;
                for (int i = 0; i < InstrumentEnabled.Length; i++)
                    if (InstrumentEnabled[i]) p += MissionCatalog.Instruments[i].PowerW;
                return p;
            }
        }

        public double InstrumentCostMillions
        {
            get
            {
                double c = 0.0;
                for (int i = 0; i < InstrumentEnabled.Length; i++)
                    if (InstrumentEnabled[i]) c += MissionCatalog.Instruments[i].CostMillions;
                return c;
            }
        }

        public double ScienceRawGbPerDay
        {
            get
            {
                double g = 0.0;
                for (int i = 0; i < InstrumentEnabled.Length; i++)
                    if (InstrumentEnabled[i]) g += MissionCatalog.Instruments[i].RawGbPerDay;
                return g;
            }
        }

        public float InstrumentPoints
        {
            get
            {
                float p = 0f;
                for (int i = 0; i < InstrumentEnabled.Length; i++)
                    if (InstrumentEnabled[i]) p += MissionCatalog.Instruments[i].Points;
                return p;
            }
        }

        public bool HasInstrument(string id)
        {
            int i = MissionCatalog.InstrumentIndex(id);
            return i >= 0 && i < InstrumentEnabled.Length && InstrumentEnabled[i];
        }

        public void SetInstrument(string id, bool on)
        {
            int i = MissionCatalog.InstrumentIndex(id);
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

        public MissionDesign Clone()
        {
            var d = (MissionDesign)MemberwiseClone();
            d.InstrumentEnabled = (bool[])InstrumentEnabled.Clone();
            return d;
        }

        public void CopyFrom(MissionDesign other)
        {
            if (other == null) return;
            VehicleId = other.VehicleId;
            PropulsionId = other.PropulsionId;
            ArrayAreaM2 = other.ArrayAreaM2;
            BatteryWh = other.BatteryWh;
            AntennaId = other.AntennaId;
            RfPowerW = other.RfPowerW;
            GroundStationId = other.GroundStationId;
            RadiationId = other.RadiationId;
            Redundancy = other.Redundancy;
            InstrumentEnabled = (bool[])other.InstrumentEnabled.Clone();
            Touch();
        }

        public void ResetToDefault()
        {
            var d = CreateDefault();
            CopyFrom(d);
        }
    }
}
