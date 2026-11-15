// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;

namespace MissionArchitect
{
    /// <summary>Editable asteroid sample-return design.</summary>
    [Serializable]
    public sealed class AsteroidDesign
    {
        public string TargetId = "bennu";
        public string BusId = "large";
        public string PropulsionId = "biprop";
        public double ArrayAreaM2 = 8.0;
        public double BatteryWh = 1000.0;
        public string AntennaId = "x15";
        public double RfPowerW = 100.0;
        public string GroundStationId = "dss";
        public string RadiationId = "cots";

        public bool[] InstrumentEnabled = new bool[AsteroidCatalog.Instruments.Length];

        public string SamplerId = "tagsam";
        public string CapsuleId = "standard";

        public string TrajectoryType = "ega";      // "direct" | "ega"
        public string VehicleId = "condor9";
        public double CampaignYears = 0.5;
        public double OrbitRadiusM = AsteroidConstants.ReferenceOrbitRadiusM;

        public string SiteId = "nightingale";
        public double ContactSpeedCmS = AsteroidConstants.TagContactSpeedCmS;
        public bool Navigation = true;
        public int Attempts = 1;

        public int Version;

        public static AsteroidDesign CreateDefault()
        {
            var d = new AsteroidDesign();
            for (int i = 0; i < d.InstrumentEnabled.Length; i++) d.InstrumentEnabled[i] = true;
            return d;
        }

        public void Touch() { Version++; }

        public bool IsElectric => PropulsionId == "electric";

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
                    if (InstrumentEnabled[i]) m += AsteroidCatalog.Instruments[i].MassKg;
                return m;
            }
        }

        public double InstrumentPowerW
        {
            get
            {
                double p = 0;
                for (int i = 0; i < InstrumentEnabled.Length; i++)
                    if (InstrumentEnabled[i]) p += AsteroidCatalog.Instruments[i].PowerW;
                return p;
            }
        }

        public double InstrumentDataGbPerDay
        {
            get
            {
                double v = 0;
                for (int i = 0; i < InstrumentEnabled.Length; i++)
                    if (InstrumentEnabled[i]) v += AsteroidCatalog.Instruments[i].DataGbPerDay;
                return v;
            }
        }

        public double InstrumentCostM
        {
            get
            {
                double c = 0;
                for (int i = 0; i < InstrumentEnabled.Length; i++)
                    if (InstrumentEnabled[i]) c += AsteroidCatalog.Instruments[i].CostM;
                return c;
            }
        }

        public float InstrumentPoints
        {
            get
            {
                float p = 0;
                for (int i = 0; i < InstrumentEnabled.Length; i++)
                    if (InstrumentEnabled[i]) p += AsteroidCatalog.Instruments[i].Points;
                return p;
            }
        }

        public bool HasInstrument(string id)
        {
            int i = AsteroidCatalog.InstrumentIndex(id);
            return i >= 0 && i < InstrumentEnabled.Length && InstrumentEnabled[i];
        }

        public void SetInstrument(string id, bool on)
        {
            int i = AsteroidCatalog.InstrumentIndex(id);
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

        public AsteroidDesign Clone()
        {
            var d = (AsteroidDesign)MemberwiseClone();
            d.InstrumentEnabled = (bool[])InstrumentEnabled.Clone();
            return d;
        }

        public void CopyFrom(AsteroidDesign other)
        {
            if (other == null) return;
            TargetId = other.TargetId;
            BusId = other.BusId;
            PropulsionId = other.PropulsionId;
            ArrayAreaM2 = other.ArrayAreaM2;
            BatteryWh = other.BatteryWh;
            AntennaId = other.AntennaId;
            RfPowerW = other.RfPowerW;
            GroundStationId = other.GroundStationId;
            RadiationId = other.RadiationId;
            InstrumentEnabled = (bool[])other.InstrumentEnabled.Clone();
            SamplerId = other.SamplerId;
            CapsuleId = other.CapsuleId;
            TrajectoryType = other.TrajectoryType;
            VehicleId = other.VehicleId;
            CampaignYears = other.CampaignYears;
            OrbitRadiusM = other.OrbitRadiusM;
            SiteId = other.SiteId;
            ContactSpeedCmS = other.ContactSpeedCmS;
            Navigation = other.Navigation;
            Attempts = other.Attempts;
            Touch();
        }

        public void ResetToDefault()
        {
            CopyFrom(CreateDefault());
        }
    }
}
